using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public GameObject button;
    public GameObject MatchFound;

    public void SendMatchRequest()
    {
        MatchRequest req = new MatchRequest();
        req.username = GameManager.Instance.username;   // 로그인된 유저명 사용

        string json = JsonUtility.ToJson(req);
        button.SetActive(false);
        StartCoroutine(MatchCoroutine(json));
    }

    IEnumerator MatchCoroutine(string json)
    {
        bool matched = false;

        while (!matched)
        {
            UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/ready/", "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MatchResponse res = JsonUtility.FromJson<MatchResponse>(request.downloadHandler.text);

                if (res.match)
                {
                    matched = true;
                    MatchFound.SetActive(true);
                    Debug.Log("매칭 완료! 방번호: " + res.room);

                    // ★ GameManager에 저장
                    GameManager.Instance.roomId = res.room;

                    yield return new WaitForSeconds(3f);
                    SceneManager.LoadScene("Loading");
                }
                else
                {
                    Debug.Log("상대방 기다리는 중...");
                }
            }
            else
            {
                Debug.Log("요청 실패: " + request.error);
            }

            yield return new WaitForSeconds(1f);
        }
    }
}

[System.Serializable]
public class MatchRequest
{
    public string username;
}

[System.Serializable]
public class MatchResponse
{
    public bool match;
    public string room;
    public string[] players;
}
