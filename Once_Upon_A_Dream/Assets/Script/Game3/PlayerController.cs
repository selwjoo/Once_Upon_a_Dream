using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string role; // 내 역할(RoleA, RoleB 등)
    public bool isMyTurn = false;

    public float speed = 5f;
    private Rigidbody2D rb;

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
        rb.MovePosition(rb.position + move);

        // 위치 서버에 보내기
        //NetworkManager.I.SendMove(GameManager.Instance.username, rb.position);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Game1 씬 이라면
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game1")
        {
            // 충돌한 오브젝트가 PointStar가 아니면 종료
            if (!collision.CompareTag("PointStar")) return;

            if (!GameManager_game1.instance.isGameStart) return;

            // 제거
            Destroy(collision.gameObject);
            GameManager_game1.instance.PlayerPoint[int.Parse(role) - 1]++; // 포인트 증가
            
            
            
        }

    }
}
