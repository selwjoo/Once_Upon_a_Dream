using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ===== 기존 변수 =====
    public string role;           // 1, 2 등 유저 역할 (기존 게임용)
    public bool isMyTurn = false;
    public float speed = 5f;
    private Rigidbody2D rb;

    // ===== ChaseGame 전용 변수 =====
    public enum GameRole { Chaser, Runner }
    public GameRole gameRole;     // ChaseGame 역할
    public bool HasWon = false;   // ChaseGame 승리 여부


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (!isMyTurn) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v).normalized * speed * Time.fixedDeltaTime;
        Vector2 newPos = rb.position + move;

        // 화면 범위 제한
        float minX = -8f;
        float maxX = 8f;
        float minY = -4.5f;
        float maxY = 2.8f;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // 이동
        rb.MovePosition(newPos);

        // 위치 서버에 보내기
        NetworkManager.I.SendMove(GameManager.Instance.username, rb.position);
    }



    // ==== ChaseGame에서 호출하는 역할 세팅 함수 ====
    public void SetRole(GameRole newRole)
    {
        gameRole = newRole;
        HasWon = false;
    }

    public void Win()
    {
        HasWon = true;
    }


    // ==== 기존 포인트 게임 충돌 처리 ====
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game1")
        {
            if (!collision.CompareTag("PointStar")) return;
            if (!GameManager_game1.instance.isGameStart) return;

            Destroy(collision.gameObject);
            GameManager_game1.instance.PlayerPoint[int.Parse(role) - 1]++;
        }

        // ChaseGame - Runner가 Light와 충돌하면 승리
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game3")
        {
            if (gameRole == GameRole.Runner && collision.CompareTag("Light"))
            {
                Win();
            }
        }

        if ((gameRole == GameRole.Chaser && collision.gameObject.CompareTag("Player")))
        {
            Win();
        }
    }
}
