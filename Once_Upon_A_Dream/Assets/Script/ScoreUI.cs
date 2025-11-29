using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI I;

    public GameObject[] slotsA; // A의 점수 UI 3개
    public GameObject[] slotsB; // B의 점수 UI 3개

    public GameObject winAPrefab;   // 승리
    public GameObject winBPrefab;   // 승리
    public Text timerText;

    public Text roleTextA;  // A의 역할 표시 텍스트
    public Text roleTextB;  // B의 역할 표시 텍스트

    void Awake()
    {
        I = this;
    }

    public void UpdateScoreUI()
    {
        // A 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            // 먼저 슬롯 안 회색 오브젝트 제거
            foreach (Transform child in slotsA[i].transform)
                Destroy(child.gameObject);

            // 점수만큼 승리 프리팹 생성
            if (i < GameManager.Instance.scoreA)
            {
                Instantiate(winAPrefab, slotsA[i].transform);
            }
        }

        // B 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            // 먼저 슬롯 안 회색 오브젝트 제거
            foreach (Transform child in slotsB[i].transform)
                Destroy(child.gameObject);

            // 점수만큼 승리 프리팹 생성
            if (i < GameManager.Instance.scoreB)
            {
                Instantiate(winBPrefab, slotsB[i].transform);
            }
        }
    }

    public void UpdateRoles(string playerA, string playerB)
    {
        // 플레이어 화면에다가 당신은 ... 입니다. 띄어ㅜ
    }

    // t: 시간(초)
    public void UpdateTimer(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";

        // 타이머를 서버에 전송 (매 프레임마다 보내면 부하가 크므로 1초마다 보내기)
        if (Mathf.FloorToInt(t) != Mathf.FloorToInt(t + Time.deltaTime))
        {
            NetworkManager.I.SendTimerUpdate(t);
        }
    }

    // 서버로부터 받은 점수 업데이트
    public void OnReceiveScoreUpdate(int scoreA, int scoreB)
    {
        GameManager.Instance.scoreA = scoreA;
        GameManager.Instance.scoreB = scoreB;
        UpdateScoreUILocal(); // 네트워크로 다시 보내지 않고 로컬만 업데이트
    }

    // 로컬 UI만 업데이트 (네트워크 전송 없음)
    void UpdateScoreUILocal()
    {
        // A 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            foreach (Transform child in slotsA[i].transform)
                Destroy(child.gameObject);
            if (i < GameManager.Instance.scoreA)
            {
                Instantiate(winAPrefab, slotsA[i].transform);
            }
        }

        // B 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            foreach (Transform child in slotsB[i].transform)
                Destroy(child.gameObject);
            if (i < GameManager.Instance.scoreB)
            {
                Instantiate(winBPrefab, slotsB[i].transform);
            }
        }
    }

    // 서버로부터 받은 역할 업데이트
    public void OnReceiveRoleUpdate(string playerA, string playerB)
    {
        if (roleTextA != null)
            roleTextA.text = playerA;
        if (roleTextB != null)
            roleTextB.text = playerB;
    }

    // 서버로부터 받은 타이머 업데이트
    public void OnReceiveTimerUpdate(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
