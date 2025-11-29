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

        StartCoroutine(GameLoop());
        

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

        Debug.Log("Roles assigned after waiting for playerAController.");
    }



    public void ApplyRoles(bool playerAIsChaser, bool isLocal = false)
    {
        if (isLocal)
        {
            // 로컬 playerA만 즉시 적용
            playerAController.SetRole(playerAIsChaser ? PlayerController.GameRole.Chaser : PlayerController.GameRole.Runner);
        }

        // playerB는 항상 playerA 반대 역할
        playerBController.SetRole(playerAIsChaser ? PlayerController.GameRole.Runner : PlayerController.GameRole.Chaser);

        Debug.Log($"역할 배정 - PlayerA({playerAController.playerName}): {playerAController.gameRole}, PlayerB({playerBController.playerName}): {playerBController.gameRole}");

        if (ui != null)
        {
            ui.UpdateRoles(
                playerAController.playerName + "는 " + playerAController.gameRole.ToString(),
                playerBController.playerName + "는 " + playerBController.gameRole.ToString()
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
                FinishRound(playerAController.playerName);
                yield break;
            }
            if (playerBController.HasWon)
            {
                FinishRound(playerBController.playerName);
                yield break;
            }
            yield return null;
        }
        FinishRound(null);
    }

    void FinishRound(string winnerName)
    {
        blackout?.StopBlackouts();
        GameManager.Instance.OnGameModeFinished(winnerName);
        if (gameLoop != null)
        {
            StopCoroutine(gameLoop);
            gameLoop = null;
        }
    }
}