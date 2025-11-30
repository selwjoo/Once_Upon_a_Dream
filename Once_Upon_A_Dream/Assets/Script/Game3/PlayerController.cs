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

    // 화면 범위 제한
    private float minX = -8f;
    private float maxX = 8f;
    private float minY = -4.5f;
    private float maxY = 2.8f;

    Animator ani;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game3")
        {
            // 화면 범위 제한
            minX = -12f;
            maxX = 12f;
            minY = -12f;
            maxY = 12f;
        }
        else
        {
            // 화면 범위 제한
            minX = -8f;
            maxX = 8f;
            minY = -4.5f;
            maxY = 4.5f;
        }
        ani = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isMyTurn) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 move = new Vector2(h, v).normalized * speed * Time.fixedDeltaTime;
        Vector2 newPos = rb.position + move;
        

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        // 이동
        rb.MovePosition(newPos);

        ani.SetFloat("DirX", h);
        ani.SetFloat("DirY", v);

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

            int roleIndex = role switch
            {
                "RoleA" => 0,
                "RoleB" => 1,
                _ => -1
            };

            if (roleIndex >= 0)
                GameManager_game1.instance.AddPoint(roleIndex);

        }

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
