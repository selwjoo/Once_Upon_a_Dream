using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public void SetPosition(float x, float y)
    {
        transform.position = new Vector2(x, y);
    }

    public void SetRole(string role)
    {
        // 스프라이트/색상 변경 가능
        Debug.Log(name + " 역할 = " + role);
    }

    public void SetAnim(string anim)
    {
        // 애니메이션 적용
        Debug.Log(name + " 애니 = " + anim);
    }
}
