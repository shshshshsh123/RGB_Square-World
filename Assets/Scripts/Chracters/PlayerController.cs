using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("플레이어 이동속도")]
    public float moveSpeed = 5.0f;
    [Tooltip("플레이어 회전속도")]
    public float rotationSpeed = 15.0f;

    [Header("# Mouse Settings")]
    [Tooltip("마우스 위치를 인식할 바닥 레이어")]
    public LayerMask groundLayer;

    private Rigidbody _rigidBody;

    private Vector3 _movement;
    private Quaternion _rotation;

    private Camera _mainCamera;

    void Awake()
    {
        // 컴포넌트 가져오기
        _rigidBody = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        CalculateMovement();
        CalculateRotation();
    }

    void FixedUpdate()
    {
        PlayerMove();
        PlayerRotate();
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
        // 입력이 있을 때만 이동 속도를 적용 (미끄러짐 방지)
        if (_movement.magnitude > 0.01f)
        {
            _rigidBody.linearVelocity = _movement * moveSpeed;
        }
        else
        {
            // 입력이 없을 때는 속도를 0으로 설정하여 멈춤 (but! y축 움직임은 유지 ex) 점프, 떨어짐등)
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(0, currentVelocity.y, 0);
        }
    }

    /// <summary>
    /// 마우스 커서 방향으로의 목표 회전 값을 계산합니다.
    /// </summary>
    void CalculateRotation()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 lookDirection = hit.point - transform.position;
            lookDirection.y = 0;

            // 목표 회전 값을 계산해서 변수에 저장만 해둡니다.
            _rotation = Quaternion.LookRotation(lookDirection).normalized;
        }
    }

    /// <summary>
    /// 계산된 방향으로 플레이어를 회전시킵니다. (Rigidbody를 이용)
    /// </summary>
    void PlayerRotate()
    {
        // Slerp를 사용하여 부드러운 회전
        Quaternion newRotation = Quaternion.Slerp(_rigidBody.rotation, _rotation, rotationSpeed * Time.fixedDeltaTime).normalized;
        _rigidBody.MoveRotation(newRotation);
    }
}
