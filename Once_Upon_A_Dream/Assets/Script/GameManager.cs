using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string username;   // 내 닉네임
    public string roomId;     // 방 번호
    public string chosenRole; // 내가 선택한 역할

    // 누가 어떤 역할인지 저장
    public Dictionary<string, string> playerRoles = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 역할 정보 업데이트
    public void SetPlayerRole(string username, string role)
    {
        if (playerRoles.ContainsKey(username))
            playerRoles[username] = role;
        else
            playerRoles.Add(username, role);

        Debug.Log($"[역할 설정] {username} -> {role}");

    }


}