using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager I;

    WebSocket ws;

    // 상대 플레이어 추적
    public string otherPlayerName;
    public GameObject otherPlayer;
    public GameObject myPlayer;

    // 위치 전송 주기 (초당 20회)
    private float sendInterval = 0.05f;
    private float sendTimer = 0f;

    // ★ 메인 스레드 큐 ★
    private Queue<Action> mainThreadQueue = new Queue<Action>();
    private object queueLock = new object();

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        ws = new WebSocket($"ws://localhost:8000/ws/game/{GameManager.Instance.roomId}/");

        ws.OnOpen += (s, e) =>
        {
            Debug.Log("WS 연결됨");
        };

        ws.OnMessage += (s, e) =>
        {
            // ★ 메시지를 메인 스레드 큐에 추가 ★
            string data = e.Data;
            EnqueueMainThread(() =>
            {
                //Debug.Log($"메시지 수신: {data}");
                OnMessage(data);
            });
        };

        ws.OnError += (s, e) =>
        {
            Debug.LogError($"WS 에러: {e.Message}");
        };

        ws.OnClose += (s, e) =>
        {
            Debug.Log($"WS 종료: {e.Reason}");
        };

        ws.Connect();
    }

    void Update()
    {
        // ★ 메인 스레드 큐 처리 ★
        ProcessMainThreadQueue();

        // 위치 전송
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendInterval)
        {
            sendTimer = 0f;
            SendMyPosition();
        }

        // otherPlayer 찾기 시도
        if (otherPlayer == null && !string.IsNullOrEmpty(otherPlayerName))
        {
            FindOtherPlayer();
        }
    }

    // ★ 메인 스레드 큐에 작업 추가 ★
    void EnqueueMainThread(Action action)
    {
        lock (queueLock)
        {
            mainThreadQueue.Enqueue(action);
        }
    }

    // ★ 메인 스레드에서 큐 처리 ★
    void ProcessMainThreadQueue()
    {
        lock (queueLock)
        {
            while (mainThreadQueue.Count > 0)
            {
                var action = mainThreadQueue.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"메인 스레드 작업 실행 에러: {e.Message}");
                }
            }
        }
    }

    void OnMessage(string json)
    {
        try
        {
            BaseMsg msg = JsonUtility.FromJson<BaseMsg>(json);
            switch (msg.type)
            {
                case "role_select":
                    var roleMsg = JsonUtility.FromJson<RoleMsg>(json);
                    GameManager.Instance.SetPlayerRole(roleMsg.username, roleMsg.role);
                    if (roleMsg.username != GameManager.Instance.username)
                    {
                        otherPlayerName = roleMsg.username;
                        Debug.Log($"[역할 선택] 상대방: {otherPlayerName}, 역할: {roleMsg.role}");
                        FindOtherPlayer();
                    }
                    RemotePlayerManager.I.OnRole(json);
                    break;
                case "move":
                    OnMove(json);
                    break;
                case "anim":
                    RemotePlayerManager.I.OnAnim(json);
                    break;
                case "score_update":
                    var scoreMsg = JsonUtility.FromJson<ScoreUpdateMsg>(json);
                    if (ScoreUI.I != null)
                    {
                        ScoreUI.I.OnReceiveScoreUpdate(scoreMsg.scoreA, scoreMsg.scoreB);
                    }
                    break;
                case "role_update":
                    var roleUpdateMsg = JsonUtility.FromJson<RoleUpdateMsg>(json);
                    if (ScoreUI.I != null)
                    {
                        ScoreUI.I.OnReceiveRoleUpdate(roleUpdateMsg.playerA, roleUpdateMsg.playerB);
                    }
                    break;
                case "timer_update":
                    var timerMsg = JsonUtility.FromJson<TimerUpdateMsg>(json);
                    if (ScoreUI.I != null)
                    {
                        ScoreUI.I.OnReceiveTimerUpdate(timerMsg.time);
                    }
                    break;
                case "chase_roles":
                    var chaseRolesMsg = JsonUtility.FromJson<ChaseRolesMsg>(json);
                    Debug.Log($"역할 수신 - PlayerA Chaser: {chaseRolesMsg.playerAIsChaser}");
                    break;

                // 3. OnMessage에 케이스 추가
                case "spawn":
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game3")
                    {
                        var spawnMsg = JsonUtility.FromJson<SpawnMsg>(json);

                        // NetworkManager에서
                        ChaseGame chaseGame = FindAnyObjectByType<ChaseGame>();
                        if (chaseGame != null)
                        {
                            chaseGame.SpawnAtPosition(spawnMsg.x, spawnMsg.y);
                        }

                    }
                    break;

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메시지 처리 에러: {e.Message}\nJSON: {json}");
        }
    }
   

    void OnMove(string json)
    {
        var msg = JsonUtility.FromJson<MoveMsg>(json);

        // 내가 보낸 메시지면 무시
        if (msg.username == GameManager.Instance.username)
        {
            return;
        }

        //Debug.Log($"[위치 수신] {msg.username} -> ({msg.x:F2}, {msg.y:F2})");

        // otherPlayer 찾기
        if (otherPlayer == null && !string.IsNullOrEmpty(otherPlayerName))
        {
            FindOtherPlayer();
        }

        if (otherPlayer != null)
        {
            // ★ 이제 메인 스레드에서 실행되므로 안전 ★
            otherPlayer.transform.position = new Vector2(msg.x, msg.y);
            //Debug.Log($"상대방 위치 업데이트 완료: {otherPlayer.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[위치 동기화 실패] otherPlayer를 찾을 수 없음! 찾는 이름: '{otherPlayerName}'");
        }
    }

    void FindOtherPlayer()
    {
        // 방법 1: 정확한 이름으로 찾기
        otherPlayer = GameObject.Find(otherPlayerName);
        if (otherPlayer != null)
        {
            Debug.Log($"[찾기 성공] 이름: {otherPlayer.name}");
            return;
        }

        // 방법 2: Player 태그로 찾기
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"[찾기 시도] Player 태그 오브젝트 수: {players.Length}");

        foreach (var p in players)
        {
            Debug.Log($"  - Player 오브젝트: {p.name}");

            // 내 캐릭터가 아닌 것 찾기
            if (p.name == otherPlayerName ||
                (p.name != GameManager.Instance.username && !p.name.Contains(GameManager.Instance.username)))
            {
                otherPlayer = p;
                Debug.Log($"[찾기 성공] Tag로 찾음: {otherPlayer.name}");
                return;
            }
        }

        Debug.LogWarning($"[찾기 실패] '{otherPlayerName}'를 찾을 수 없음");
    }

    void SendMyPosition()
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open || myPlayer == null)
            return;


        SendMove(GameManager.Instance.username, myPlayer.transform.position);
        
    }

    public void SendRole(string username, string role)
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open)
        {
            Debug.LogWarning("WebSocket이 연결되지 않음");
            return;
        }

        var msg = new RoleMsg { type = "role_select", username = username, role = role };
        string json = JsonUtility.ToJson(msg);
        Debug.Log($"[역할 전송] {json}");
        ws.Send(json);
    }

    public void SendMove(string username, Vector2 pos)
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open)
            return;

        var msg = new MoveMsg { type = "move", username = username, x = pos.x, y = pos.y };
        ws.Send(JsonUtility.ToJson(msg));
    }

    public void SendAnim(string username, string anim)
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open)
            return;

        var msg = new AnimMsg { type = "anim", username = username, animState = anim };
        ws.Send(JsonUtility.ToJson(msg));
    }

    public void SendScoreUpdate(int scoreA, int scoreB)
    {
        string json = JsonUtility.ToJson(new { type = "score_update", scoreA = scoreA, scoreB = scoreB });
        ws.Send(json);
    }

    public void SendRoleUpdate(string playerA, string playerB)
    {
        string json = JsonUtility.ToJson(new { type = "role_update", playerA = playerA, playerB = playerB });
        ws.Send(json);
    }

    public void SendTimerUpdate(float time)
    {
        string json = JsonUtility.ToJson(new { type = "timer_update", time = time });
        ws.Send(json);
    }
    public void SendChaseRoles(bool playerAIsChaser)
    {
        ChaseRolesMsg msg = new ChaseRolesMsg
        {
            playerAIsChaser = playerAIsChaser
        };
        string json = JsonUtility.ToJson(msg);
        ws.Send(json);
        Debug.Log($"역할 전송 - PlayerA Chaser: {playerAIsChaser}");
    }

    // 2. NetworkManager.cs에 메서드 추가
    // 클라이언트가 서버에 스폰 요청
    public void RequestSpawn()
    {
        if (ws == null || ws.ReadyState != WebSocketState.Open)
            return;

        var msg = new SpawnRequestMsg { type = "spawn_request" };
        ws.Send(JsonUtility.ToJson(msg));
        Debug.Log("스폰 요청 전송");
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }
}

[System.Serializable]
public class BaseMsg { public string type; }

[System.Serializable]
public class MoveMsg : BaseMsg
{
    public string username;
    public float x;
    public float y;
}

[System.Serializable]
public class RoleMsg : BaseMsg
{
    public string username;
    public string role;
}

[System.Serializable]
public class AnimMsg : BaseMsg
{
    public string username;
    public string animState;
}

// 메시지 클래스들
[System.Serializable]
public class ScoreUpdateMsg
{
    public string type = "score_update";
    public int scoreA;
    public int scoreB;
}

[System.Serializable]
public class RoleUpdateMsg
{
    public string type = "role_update";
    public string playerA;
    public string playerB;
}

[System.Serializable]
public class TimerUpdateMsg
{
    public string type = "timer_update";
    public float time;
}

[System.Serializable]
public class ChaseRolesMsg
{
    public string type = "chase_roles";
    public bool playerAIsChaser;
}

// 1. NetworkManager.cs에 메시지 클래스 추가
[System.Serializable]
public class SpawnRequestMsg
{
    public string type = "spawn_request";
}

[System.Serializable]
public class SpawnMsg
{
    public float x;
    public float y;
}



