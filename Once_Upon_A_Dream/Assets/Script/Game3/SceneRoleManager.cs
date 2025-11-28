using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SceneRoleManager : MonoBehaviour
{
    public string myRole;
    public GameObject roleAPrefab;
    public GameObject roleBPrefab;

    IEnumerator Start()
    {
        yield return GetMyRoleFromServer();

        if (myRole == "RoleA")
            Instantiate(roleAPrefab);
        else if (myRole == "RoleB")
            Instantiate(roleBPrefab);
        else
            Debug.LogError("역할 정보를 서버에서 가져오지 못함!");
    }

    IEnumerator GetMyRoleFromServer()
    {
        var requestData = new PlayerRoleRequest
        {
            username = GameManager.Instance.username,
            room = GameManager.Instance.roomId
        };

        string json = JsonUtility.ToJson(requestData);
        Debug.Log("요청 JSON: " + json);

        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/get_role/", "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            RoleResponse res = JsonUtility.FromJson<RoleResponse>(request.downloadHandler.text);
            if (res.success)
            {
                myRole = res.role;
                Debug.Log("서버에서 역할 조회 성공: " + myRole);
            }
            else
            {
                Debug.LogError("서버에서 역할 조회 실패: " + res.role);
            }
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
        }
    }
}

[System.Serializable]
public class PlayerRoleRequest
{
    public string username;
    public string room;
}



