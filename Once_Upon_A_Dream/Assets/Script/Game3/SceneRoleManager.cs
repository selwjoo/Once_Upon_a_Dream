using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneRoleManager : MonoBehaviour
{
    public GameObject roleAPrefab;
    public GameObject roleBPrefab;

    private GameObject myChar;
    private GameObject otherChar;
    public string myRole;
    public string otherRole;

    IEnumerator Start()
    {
        // 서버에서 역할 정보 가져오기
        yield return GetRolesFromServer();

        // 캐릭터 Instantiate
        if (myRole == "RoleA")
        {
            myChar = Instantiate(roleAPrefab);
            otherChar = Instantiate(roleBPrefab);
        }
        else
        {
            myChar = Instantiate(roleBPrefab);
            otherChar = Instantiate(roleAPrefab);
        }

        // PlayerController 참조
        PlayerController myCtrl = myChar.GetComponent<PlayerController>();
        PlayerController otherCtrl = otherChar.GetComponent<PlayerController>();


        // 씬이 Game3이면 ChaseGame에 할당
        if (SceneManager.GetActiveScene().name == "Game3")
        {
            var chaseGame = FindAnyObjectByType<ChaseGame>();
            if (chaseGame != null)
            {
                // role 값으로 A와 B 구분해서 전달
                PlayerController playerA, playerB;

                if (myCtrl.role == "RoleA")
                {
                    playerA = myCtrl;
                    playerB = otherCtrl;
                }
                else
                {
                    playerA = otherCtrl;
                    playerB = myCtrl;
                }

                
                chaseGame.SetPlayers(playerA, playerB);
            }
        }

        // 이동 가능 설정
        myCtrl.isMyTurn = true;
        otherCtrl.isMyTurn = false;

        // NetworkManager에 연결
        NetworkManager.I.myPlayer = myChar;
        NetworkManager.I.otherPlayer = otherChar;
    }

    IEnumerator GetRolesFromServer()
    {
        GetRoleRequest req = new GetRoleRequest
        {
            username = GameManager.Instance.username,
            room = GameManager.Instance.roomId
        };

        string json = JsonUtility.ToJson(req);

        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/get_role/", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GetRoleResponse res = JsonUtility.FromJson<GetRoleResponse>(request.downloadHandler.text);

            if (res.success)
            {
                myRole = res.role;        // 서버에서 보내는 이름 그대로
                otherRole = res.otherRole;
                Debug.Log("내 역할: " + myRole + ", 상대: " + otherRole);
            }
            else
            {
                Debug.LogError("역할 조회 실패");
            }
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
        }
    }

}

[System.Serializable]
public class GetRoleRequest
{
    public string username;
    public string room;
}
[System.Serializable]
public class GetRoleResponse
{
    public bool success;
    public string role;        // myRole → role
    public string otherRole;
    public bool both_selected;
}

