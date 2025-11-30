using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public string username;   // 내 닉네임
    public string roomId;     // 방 번호
    public string chosenRole; // 내가 선택한 역할


    public int scoreA = 0; // Player A
    public int scoreB = 0; // Player B

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

    public void FinalWinner(string winner)
    {
        
        // 이후 처리(다음 라운드 or 씬 이동 등)
    }

    // winnerName이 null이면 무승부
    public void OnGameModeFinished(string winnerName)
    {
        if (winnerName == "RoleA") scoreA++;
        else if (winnerName == "RoleB") scoreB++;

        // UI 갱신 요청
        ScoreUI.I.UpdateScoreUI();
    }
}