using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LoginData
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public bool success;
    public bool error;
    public string message;
}

public class LoginManager : MonoBehaviour
{
    public InputField nameInput;
    public InputField passwordInput;

    public void Login()
    {
        LoginData data = new LoginData
        {
            username = nameInput.text,
            password = passwordInput.text
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendLoginRequest(json));
    }

    public void Register()
    {
        LoginData data = new LoginData
        {
            username = nameInput.text,
            password = passwordInput.text
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendRegisterRequest(json));
    }

    private IEnumerator SendLoginRequest(string json)
    {
        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/login/", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP 요청 실패: " + request.error);
            yield break;
        }

        LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

        if (response.success)
        {
            Debug.Log("로그인 성공!");
            GameManager.Instance.username = nameInput.text;
            SceneManager.LoadScene("Play");
        }
        else
        {
            Debug.LogWarning("로그인 실패: " + response.message);
        }
    }

    private IEnumerator SendRegisterRequest(string json)
    {
        UnityWebRequest request = new UnityWebRequest("http://127.0.0.1:8000/unity/register/", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("HTTP 요청 실패: " + request.error);
            yield break;
        }

        LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

        if (response.success)
        {
            Debug.Log("회원가입 성공: " + response.message);
            SceneManager.LoadScene("LogIn");
        }
        else
        {
            Debug.LogWarning("회원가입 실패: " + response.message);
        }
    }
}
