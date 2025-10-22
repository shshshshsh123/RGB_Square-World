using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(PlayerChargeAttack))]
public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("플레이어 이동속도")]
    public float moveSpeed = 7.5f;
    [Tooltip("플레이어 회전속도")]
    public float rotationSpeed = 15.0f;

    [Header("# Mouse Settings")]
    [Tooltip("마우스 위치를 인식할 바닥 레이어")]
    public LayerMask groundLayer;

    private Rigidbody _rigidBody;
    private Animator _animator;
    private float _animatorMoveX; // 애니메이터에 전달될 현재 X값
    private float _animatorMoveZ; // 애니메이터에 전달될 현재 Z값
    public float animationSmoothTime = 0.1f; // 값이 변하는데 걸리는 시간 (조절 가능)

    private Vector3 _movement;
    private Quaternion _rotation;
    public Quaternion Rotation => _rotation; // 외부에서 읽기 전용으로 _rotation 값 접근

    private Camera _mainCamera;

    void Awake()
    {
        // 컴포넌트 가져오기
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _mainCamera = Camera.main;

        _rigidBody.freezeRotation = true; // 물리엔진에 의한 회전 방지
    }

    void Update()
    {
        CalculateMovement();
        CalculateRotation();

        // Test
        TestFunction();
    }

    void FixedUpdate()
    {
        PlayerMove();
        PlayerRotate();
    }

    /// <summary>
    /// 움직임에 관련된 Input 감지 및 이동방향 설정
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
    /// 플레이어 이동(rigidBody를 이용한 처리) -> FixedUpdate에서 사용할 것
    /// </summary>
    void PlayerMove()
    {
        // 입력이 있을 때만 이동 속도를 적용 (미끄러짐 방지)
        if (_movement.magnitude > 0.01f)
        {
            _rigidBody.linearVelocity = _movement * moveSpeed;

            // 현재 Rigidbody 속도를 로컬 좌표계로 변환 (목표 애니메이션 값 계산용)
            Vector3 localVelocity = transform.InverseTransformDirection(_rigidBody.linearVelocity);

            // 목표 MoveX, MoveZ 값 결정 (이동 중일 때만 localVelocity 사용, 멈추면 0)
            float targetMoveX = (_movement.magnitude > 0.1f) ? localVelocity.x / moveSpeed : 0f; // 속도로 나누어 -1 ~ 1 범위로 정규화
            float targetMoveZ = (_movement.magnitude > 0.1f) ? localVelocity.z / moveSpeed : 0f; // 속도로 나누어 -1 ~ 1 범위로 정규화

            // Mathf.Lerp를 사용하여 현재 애니메이터 값을 목표 값으로 부드럽게 변경
            _animatorMoveX = Mathf.Lerp(_animatorMoveX, targetMoveX, Time.fixedDeltaTime * (1f / animationSmoothTime));
            _animatorMoveZ = Mathf.Lerp(_animatorMoveZ, targetMoveZ, Time.fixedDeltaTime * (1f / animationSmoothTime));

            // 애니메이션 설정 (부드럽게 보간된 값 사용)
            _animator.SetBool("isRunning", _movement.magnitude > 0.1f);
            _animator.SetFloat("MoveX", _animatorMoveX);
            _animator.SetFloat("MoveZ", _animatorMoveZ);
        }
        else
        {
            // 입력이 없을 때는 속도를 0으로 설정하여 멈춤 (but! y축 움직임은 유지 ex) 점프, 떨어짐 등)
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(0, currentVelocity.y, 0);

            // 애니메이션 설정
            _animator.SetBool("isRunning", false);
            _animator.SetFloat("MoveX", 0f);
            _animator.SetFloat("MoveZ", 0f);
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

            // 목표 회전 값을 계산해서 저장 (마우스가 플레이어 위치와 거의 같다면 회전하지 않음)
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                _rotation = Quaternion.LookRotation(lookDirection);
            }
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

    void TestFunction()
    {
        if (Input.GetMouseButtonDown(2))
        {
            ObjectPooler.Instance.SpawnFromPool(PoolType.Dummy, transform.position + Vector3.one * Random.Range(5, 30), Quaternion.identity);
        }
    }
}