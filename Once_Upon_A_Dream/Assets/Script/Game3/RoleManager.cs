using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoleManager : MonoBehaviour
{
    public PlayerController roleAPlayer; // RoleA 캐릭터
    public PlayerController roleBPlayer; // RoleB 캐릭터

    // 버튼에서 호출
    public void ChooseRole(string role)
    {
        RoleRequest req = new RoleRequest
        {
            username = GameManager.Instance.username,
            room = GameManager.Instance.roomId,
            chosenRole = role
        };

        StartCoroutine(SendRoleRequest(req));
    }

    IEnumerator SendRoleRequest(RoleRequest req)
    {
        string json = JsonUtility.ToJson(req);
        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/choose_role/", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            RoleResponse res = JsonUtility.FromJson<RoleResponse>(request.downloadHandler.text);

            if (res.success)
            {
                Debug.Log("내 역할: " + res.role);

                // ★ 선택한 역할 캐릭터만 움직임 가능
                roleAPlayer.isMyTurn = (res.role == "RoleA");
                roleBPlayer.isMyTurn = (res.role == "RoleB");

                GameManager.Instance.chosenRole = res.role;

                if (res.role == "RoleA")
                {
                    NetworkManager.I.otherPlayer = roleBPlayer.gameObject;
                }
                else
                {
                    NetworkManager.I.otherPlayer = roleAPlayer.gameObject;
                }


                yield return new WaitForSeconds(1f);
            }
            else
            {
                Debug.Log("선택 실패: " + res.message);
            }
        }
        else
        {
            Debug.Log("요청 실패: " + request.error);
        }
    }
}

[System.Serializable]
public class RoleRequest
{
    public string username;
    public string room;
    public string chosenRole;
}

[System.Serializable]
public class RoleResponse
{
    public bool success;
    public string role;
    public string message;
}
