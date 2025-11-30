using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager_game2 : MonoBehaviour
{
    public static GameManager_game2 instance;

    public PlayerController[] players; // 플레이어 목록

    public float GameTimer = 0;

    public float TileSpeed = 3;

    public bool isGameStart = false;

    AudioSource audios;

    public List<float> RhytmTimes; // 오디오 타일을 설치하는 list

    [SerializeField] float TileStart = 5.6f; // 타일이 시작하는 지점
    [SerializeField] float TileEnd = -2.5f; // 타일이 도착해야 할 지점

    public GameObject RhythmTileObject;


    [SerializeField] bool isRecord = false; // 타일 찍을 때 사용



    public int[] PlayerPoints;

    public GameObject WinPlayer;

    [SerializeField] Text[] PointTxt;

    [Header("상호작용할 키")]
    public KeyCode InteractionKey = KeyCode.Space;
    private void Awake()
    {
        instance = this;
        audios = GetComponent<AudioSource>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("GameStart", 2); // 2초 후 게임 시작

        if (isRecord) RhytmTimes.Clear();

    }

    // Update is called once per frame
    void Update()
    {
        if (isGameStart)
        {
            for (int i = 0; i < players.Length; i++)
            {
                PointTxt[i].text = $"P{i + 1} : " + PlayerPoints[i].ToString("");
            }

            GameTimer += Time.deltaTime;
        } 
        else
        {
            if (!audios.isPlaying)
            {
                GameEnd();
                
            }
        }

        for (int i = 0; i < PlayerPoints.Length; i++)
        {
            PlayerPoints[i] = Mathf.Clamp(PlayerPoints[i], 0, 999);
        }

        // 타일 찍기
        if (isRecord)
        {
            if (isGameStart && Input.GetKeyDown(InteractionKey))
            {
                RhytmTimes.Add(GameTimer);
            }
        }

        // 게임 플레이
        if (!isRecord)
        {
            for (int i = RhytmTimes.Count - 1; i >= 0; i--)
            {
                float targetTime = RhytmTimes[i] - GetTravelTime(TileStart, TileEnd, TileSpeed);

                if (GameTimer >= targetTime)
                {
                    Instantiate(RhythmTileObject, new Vector3(-3.36f, TileStart, 0), Quaternion.identity);
                    Instantiate(RhythmTileObject, new Vector3(3.36f, TileStart, 0), Quaternion.identity);
                    RhytmTimes.RemoveAt(i);
                }
            }
        }

    }


    void GameStart()
    {
        isGameStart = true;
        audios.Play();
    }

    // 타일 생성 지연 시간 계산
    public float GetTravelTime(float startY, float targetY, float speed)
    {
        float distance = Mathf.Abs(startY - targetY); // 거리 구함
        return distance / speed; // 목표 위치 까지 가는 시간 계산
    }

    void GameEnd()
    {
        isGameStart = false;
        if (PlayerPoints[0] > PlayerPoints[1]) WinPlayer = players[0].gameObject; // 플레이어1 승리
        else if (PlayerPoints[0] < PlayerPoints[1]) WinPlayer = players[1].gameObject; // 플레이어2 승리
        else
        {
            // 비김
        }

        StartCoroutine(ScoreUI.I.SceneGO());
    }
    
}
