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

        // UI 업데이트
        if (ui != null)
        {
            ui.UpdateRoles(
                playerAController.role + "는 " + playerAController.gameRole.ToString(),
                playerBController.role + "는 " + playerBController.gameRole.ToString()
            );
        }
    }


    IEnumerator GameLoop()
    {
        float t = roundTime;
        blackout?.StartBlackouts();

        Debug.Log("GameLoop 호출됨");

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
        blackout?.StopBlackouts();
        Debug.Log("이겨서 나옴");
        GameManager.Instance.OnGameModeFinished(winnerName);
        if (gameLoop != null)
        {
            StopCoroutine(gameLoop);
            gameLoop = null;
        }
    }
}