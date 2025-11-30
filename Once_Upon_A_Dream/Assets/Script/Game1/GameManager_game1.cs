using UnityEngine;
using UnityEngine.UI;

public class GameManager_game1 : MonoBehaviour
{


    public static GameManager_game1 instance;


    public PlayerController[] players; // 플레이어 목록
    public int[] PlayerPoint = new int[2]; // 플레이어 점수

    public GameObject PointStar; // 점수 프리펩

    public float GameSec; // 게임 시간
    public bool isGameStart; // 게임 시작 여부

    public Vector3[] firstPosition; // 플레이어 첫 위치

    [SerializeField] Text timerTxt;
    [SerializeField] Text[] PointTxt = new Text[2];

    private void Awake()
    {
        instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
        Game1UpdateTimer(GameSec);
    }

    // Update is called once per frame
     void Update()
    {
        // players가 2명이 될 때까지 기다림
        if (!isGameStart)
        {
            if (players != null && players.Length == 2
                && players[0] != null && players[1] != null)
            {
                StartGame();
            }
            return;
        }

        // 시간 감소
        GameSec -= Time.deltaTime;
        OnReceiveTimerUpdate(GameSec);

        // 게임 종료 조건
        if (GameSec <= 0 || GameObject.FindGameObjectsWithTag("PointStar").Length <= 0)
        {
            EndGame();
        }
    }

    // t: 시간(초)
    public void Game1UpdateTimer(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);

        timerTxt.text = $"{minutes:00}:{seconds:00}";

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
        timerTxt.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartGame()
    {

        GameSec = 60; // 게임 시간 설정
        int r = Random.Range(10, 21); // 생성 될 점수 수량

        // 점수 소환
        for (int i = 0; i < r; i++)
        {
            Instantiate(PointStar, new Vector3(Random.Range(-8.12f, 8.12f), Random.Range(-4.5f, 4.5f), 0), new Quaternion(0, 0, 0,0));
        }

        isGameStart = true; // 게임 시작 여부 결정

        for (int i = 0; i < PlayerPoint.Length; i++)
        {
            PlayerPoint[i] = 0;
            PointTxt[i].text = "0";
        }

    }

    public void AddPoint(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= PlayerPoint.Length) return;

        PlayerPoint[playerIndex]++;

        // 서버에 점수 업데이트 전송
        NetworkManager.I.SendScoreGame1Update(PlayerPoint[0], PlayerPoint[1]);
    }

    public void OnScoreUpdate(int scoreA, int scoreB)
    {
        PlayerPoint[0] = scoreA;
        PlayerPoint[1] = scoreB;

        PointTxt[0].text = $"{scoreA}";
        PointTxt[1].text = $"{scoreB}";
    }

    public void EndGame()
    {
        // 플레이어 멈추기
        for (int i = 0; i < players.Length; i++)
        {
            players[i].isMyTurn = false;

            players[i].GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, 0);

            players[i].GetComponent<Animator>().SetFloat("DirX", 0);
            players[i].GetComponent<Animator>().SetFloat("DirY", 0);
        }
        isGameStart = false;

        GameObject[] point = GameObject.FindGameObjectsWithTag("PointStar"); // 이미 생성 되어있는 별 찾기

        // 남아있는 별 제거
        for (int i = 0; i < point.Length; i++)
        {
            Destroy(point[i]);
        }

        if (PlayerPoint[0] > PlayerPoint[1]) // 달 승리 
        {
            GameManager.Instance.OnGameModeFinished("RoleA");
            StartCoroutine(ScoreUI.I.SceneGO());
        }
        else if (PlayerPoint[0] < PlayerPoint[1]) // 태양 승리
        {
            GameManager.Instance.OnGameModeFinished("RoleB");
            StartCoroutine(ScoreUI.I.SceneGO());
        }
        else 
        {
            GameManager.Instance.OnGameModeFinished(null);
        }


        timerTxt.text = "";
    }
    
}
