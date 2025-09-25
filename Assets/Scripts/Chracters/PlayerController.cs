using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("플레이어 이동속도")]
    public float moveSpeed = 5.0f;

    private Rigidbody _rigidBody;
    private Vector3 _movement;

    void Awake()
    {
        // 컴포넌트 가져오기
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CalculateMovement();
    }

    void FixedUpdate()
    {
        PlayerMove();
    }

    /// <summary>
    /// 움직임에 관련된 Input 감지및 이동방향 설정
    /// </summary>
    void CalculateMovement()
    {
        // 입력감지
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        // 이동방향만 계산
        _movement = new Vector3(hAxis, 0, vAxis).normalized;
    }

    /// <summary>
    /// 플레이어 이동(rigidBody를 이용한 처리) -> FixedUpdate에서 사용할것
    /// </summary>
    void PlayerMove()
    {
        _rigidBody.linearVelocity = _movement * moveSpeed;
    }
}
