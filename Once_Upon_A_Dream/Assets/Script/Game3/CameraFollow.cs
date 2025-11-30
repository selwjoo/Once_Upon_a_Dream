using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothTime = 0.3f; // 목표 위치에 도달하는 데 걸리는 시간

    private Vector3 velocity = Vector3.zero; // SmoothDamp용 속도 변수

    void LateUpdate()
    {
        if (target == null)
        {
            FindMyPlayer();
            return;
        }

        Vector3 targetPosition = target.position + offset;

        // SmoothDamp - 가속/감속이 자연스러움
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    void FindMyPlayer()
    {
        if (NetworkManager.I != null && NetworkManager.I.myPlayer != null)
        {
            target = NetworkManager.I.myPlayer.transform;
            Debug.Log("카메라 타겟 설정: " + target.name);
        }
    }
}
