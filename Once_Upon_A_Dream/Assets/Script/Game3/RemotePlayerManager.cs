using UnityEngine;
using System.Collections.Generic;

public class RemotePlayerManager : MonoBehaviour
{
    public static RemotePlayerManager I;
    public GameObject otherPlayerPrefab; //상대방 캐릭터

    Dictionary<string, OtherPlayer> players = new Dictionary<string, OtherPlayer>();

    void Awake() => I = this;

    public void OnMove(string json)
    {
        MoveMsg msg = JsonUtility.FromJson<MoveMsg>(json);

        if (!players.ContainsKey(msg.username))
        {
            var obj = Instantiate(otherPlayerPrefab);
            var remote = obj.GetComponent<OtherPlayer>();
            players[msg.username] = remote;
        }

        players[msg.username].SetPosition(msg.x, msg.y);
    }

    public void OnRole(string json)
    {
        RoleMsg msg = JsonUtility.FromJson<RoleMsg>(json);

        if (!players.ContainsKey(msg.username))
        {
            // 새로운 OtherPlayer 생성
            var obj = Instantiate(otherPlayerPrefab);
            var other = obj.GetComponent<OtherPlayer>();
            players[msg.username] = other;
        }

        players[msg.username].SetRole(msg.role);

        // 자기 자신이면 isMyTurn 설정
        if (msg.username == GameManager.Instance.username)
        {
            var player = players[msg.username].GetComponent<PlayerController>();
            player.isMyTurn = true;
        }
    }

    public void OnAnim(string json)
    {
        AnimMsg msg = JsonUtility.FromJson<AnimMsg>(json);
        if (!players.ContainsKey(msg.username)) return;
        players[msg.username].SetAnim(msg.animState);
    }
}
