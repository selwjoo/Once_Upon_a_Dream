using UnityEngine;
using System.Collections;

public class ChaseGame : MonoBehaviour
{
    public PlayerController playerAController;
    public PlayerController playerBController;
    public BlackOut blackout;
    public ScoreUI ui;
    public float roundTime = 120f;
    private Coroutine gameLoop;

    public GameObject prefab; // 생성할 오브젝트

    // 범위
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 2.8f;
    void Start()
    {
        StartMode();
    }

    public void SetPlayers(PlayerController playerA, PlayerController playerB)
    {
        playerAController = playerA;
        playerBController = playerB;

    }

    public void StartMode()
    {
        Debug.Log("StartMode 호출됨");

        StartCoroutine(WaitForPlayerAAndAssignRoles());

    }

    private IEnumerator WaitForPlayerAAndAssignRoles()
    {
        // playerAController가 할당될 때까지 대기
        while (playerAController == null)
        {
            yield return null; // 다음 프레임까지 대기
        }

        // 이제 할당됨, 역할 결정 진행
        bool playerAIsChaser = Random.value > 0.5f;

        // 서버로 역할 전송 (playerB에게 전달될 예정)
        NetworkManager.I.SendChaseRoles(playerAIsChaser);

        // 로컬 적용 (playerA는 즉시 적용)
        ApplyRoles(playerAIsChaser, isLocal: true);

        ScoreUI.I.OnReceiveChaseRoles(playerAIsChaser);

        RequestRandomSpawn();

        gameLoop = StartCoroutine(GameLoop());

    }



    public void ApplyRoles(bool playerAIsChaser, bool isLocal = false)
    {
        if (isLocal)
        {
            // playerA (호스트)에서 둘 다 설정
            playerAController.SetRole(playerAIsChaser ? PlayerController.GameRole.Chaser : PlayerController.GameRole.Runner);
            playerBController.SetRole(playerAIsChaser ? PlayerController.GameRole.Runner : PlayerController.GameRole.Chaser);

            Debug.Log($"[로컬] playerA 역할: {playerAController.gameRole}");
            Debug.Log($"[로컬] playerB 역할: {playerBController.gameRole}");
        }
        // else 블록 삭제! playerB에서는 아무것도 안 함

    }


    IEnumerator GameLoop()
    {
        float t = roundTime;
        blackout.StartBlackouts();


        while (t > 0)
        {
            t -= Time.deltaTime;
            ui?.UpdateTimer(t);

            if (playerAController.HasWon)
            {
                FinishRound(playerAController.role);
                yield break;
            }
            if (playerBController.HasWon)
            {
                FinishRound(playerBController.role);
                yield break;
            }
            yield return null;
        }
        FinishRound(null);
    }

    void FinishRound(string winnerName)
    {
        blackout.StopBlackouts();
        Debug.Log("이겨서 나옴");
        GameManager.Instance.OnGameModeFinished(winnerName);
        if (gameLoop != null)
        {
            StopCoroutine(gameLoop);
            gameLoop = null;
        }
    }

    // 스폰 요청 (아무 클라이언트에서 호출 가능)
    public void RequestRandomSpawn()
    {
        // PlayerA만 요청
        if (GameManager.Instance.chosenRole == "RoleA")
        {
            NetworkManager.I.RequestSpawn();
            Debug.Log("스폰 요청 전송 (PlayerA)");
        }

    }

    public void SpawnAtPosition(float x, float y)
    {
        Vector3 spawnPos = new Vector3(x, y, 0);

        // 이미 생성됐는지 체크 (선택사항)
        // if (existingObject != null) return;

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        Debug.Log($"오브젝트 생성: ({x}, {y})");
    }


}