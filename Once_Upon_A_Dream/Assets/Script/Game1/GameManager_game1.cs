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


    public GameObject WinPlayer;
    private void Awake()
    {
        instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 게임이 시작되지 않았을 때 Tab 키 누를 경우 게임 시작
        if (Input.GetKeyDown(KeyCode.Tab) && !isGameStart) StartGame();


        if (isGameStart)
        {
            // 게임 시간 감소
            GameSec -= Time.deltaTime;
            timerTxt.text = GameSec.ToString("00");



            for (int i = 0; i < PlayerPoint.Length; i++)
            {
                PointTxt[i].text = $"P{i+1} : " + PlayerPoint[i].ToString("");
            }

            if (GameSec <= 0) EndGame(); // 시간 다 되면 게임 종료

            if (GameObject.FindGameObjectsWithTag("PointStar").Length <= 0) EndGame(); // 점수가 없으면 게임 종료
        }

        
    }



    public void StartGame()
    {
        WinPlayer = null;
        firstPosition = new Vector3[2];
        // 플레이어 움직일 수 있도록
        for (int i = 0; i < players.Length; i++)
        {
            players[i].isMyTurn = true;

            firstPosition[i] = players[i].transform.position; // 플레이어 첫 위치 기록
        }

        GameSec = 60; // 게임 시간 설정
        

        int r = Random.Range(10, 21); // 생성 될 점수 수량

        


        // 점수 소환
        for (int i = 0; i < r; i++)
        {
            Instantiate(PointStar, new Vector3(Random.Range(-8.12f, 8.12f), Random.Range(-4.5f, 4.5f), 0), new Quaternion(0, 0, 0,0));
        }


        isGameStart = true; // 게임 시작 여부 결정

    }


    public void EndGame()
    {
        // 플레이어 멈추기
        for (int i = 0; i < players.Length; i++)
        {
            players[i].isMyTurn = false;

            players[i].transform.position = firstPosition[i];
        }
        isGameStart = false;

        GameObject[] point = GameObject.FindGameObjectsWithTag("PointStar"); // 이미 생성 되어있는 별 찾기

        // 남아있는 별 제거
        for (int i = 0; i < point.Length; i++)
        {
            Destroy(point[i]);
        }


        if (PlayerPoint[0] > PlayerPoint[1]) WinPlayer = players[0].gameObject; // 플레이어1 승리
        else if (PlayerPoint[0] < PlayerPoint[1]) WinPlayer = players[1].gameObject; // 플레이어2 승리
        else 
        {
            // 비김
        }


        timerTxt.text = "";
    }
    
}
