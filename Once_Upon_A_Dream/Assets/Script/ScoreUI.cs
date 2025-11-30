using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        UpdateScoreUI();
    }

    // 서버로부터 받은 점수 업데이트
    public void OnReceiveScoreUpdate(int scoreA, int scoreB)
    {
        GameManager.Instance.scoreA = scoreA;
        GameManager.Instance.scoreB = scoreB;

        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        // A 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            // 슬롯 안 오브젝트 삭제
            for (int c = slotsA[i].transform.childCount - 1; c >= 0; c--)
            {
                Destroy(slotsA[i].transform.GetChild(c).gameObject);
            }

            // 점수만큼 승리 프리팹 생성
            if (i < GameManager.Instance.scoreA)
            {
                GameObject obj = Instantiate(winAPrefab); // 먼저 씬에 생성
                obj.transform.SetParent(slotsA[i].transform, false); // 부모 슬롯에 붙이고 위치 유지
                obj.transform.localPosition = Vector3.zero; // 안전하게 초기화
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
        }

        // B 점수 갱신
        for (int i = 0; i < 3; i++)
        {
            for (int c = slotsB[i].transform.childCount - 1; c >= 0; c--)
            {
                Destroy(slotsB[i].transform.GetChild(c).gameObject);
            }

            if (i < GameManager.Instance.scoreB)
            {
                GameObject obj = Instantiate(winBPrefab);
                obj.transform.SetParent(slotsB[i].transform, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
        }

        

        

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

    // 서버로부터 받은 타이머 업데이트
    public void OnReceiveTimerUpdate(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }


    public void OnReceiveChaseRoles(bool playerAIsChaser)
    {
        Debug.Log($"[ScoreUI] Chase Roles 수신 - PlayerA Chaser: {playerAIsChaser}");

        string myRole = GameManager.Instance.chosenRole;

        if (myRole == "RoleA")
        {
            if (playerAIsChaser)
            {
                StartCoroutine(ShowMyRole(roleTextB)); //Chaser
            }
            else
            {
                StartCoroutine(ShowMyRole(roleTextA)); //Ruuner
            }
        }
        // PlayerB라면
        else if (myRole == "RoleB")
        {
            if (playerAIsChaser)
            {
                StartCoroutine(ShowMyRole(roleTextA)); //Runner
            }
            else
            {
                StartCoroutine(ShowMyRole(roleTextB)); //Chaser
            }
        }
    }

    System.Collections.IEnumerator ShowMyRole(Text roleText)
    {
        if (roleText == null) yield break;

        roleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        roleText.gameObject.SetActive(false);
    }

    public System.Collections.IEnumerator SceneGO()
    {

        yield return new WaitForSeconds(2f);
        // 이겼다고 나와야행

        if (SceneManager.GetActiveScene().name == "Game1")
        {
            SceneManager.LoadScene("Game2");
        }
        else if (SceneManager.GetActiveScene().name == "Game2")
        {
            SceneManager.LoadScene("Game3");
        }
        else if (SceneManager.GetActiveScene().name == "Game3")
        {
            SceneManager.LoadScene("Story2"); 
        }
        
    }


}
