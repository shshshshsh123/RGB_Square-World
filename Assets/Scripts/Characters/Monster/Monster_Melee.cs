using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Monster_Melee : BaseMonster
{
    [Header("근접 몬스터 설정")]

    [Tooltip("돌진 속도")]
    public float dashSpeed = 7f;

    [Tooltip("돌진 지속 시간")]
    public float dashDuration = 0.5f;

    [Tooltip("돌진 가속도")]
    public float dashAcceleration = 100f;

    [Tooltip("플레이어를 지나쳐 돌진할 추가 거리")]
    public float dashDistance = 3f;

    // NavMeshAgent의 기본 설정값
    private float _normalSpeed;
    private float _normalAcceleration;
    private float _normalStoppingDistance;

    private Rigidbody _rigidBody;
    private CapsuleCollider _collider; // 충돌용 콜라이더

    protected override void Awake()
    {
        // 부모의 Awake() 호출
        base.Awake();

        _rigidBody = GetComponent<Rigidbody>();

        // 몬스터의 모든 콜라이더를 가져와서 isTrigger가 아닌 콜라이더를 비활성화 시키기 위해 저장 (플레이어와 부딪히지 않고 통과하게 만들기 위해서)
        CapsuleCollider[] colliders = GetComponents<CapsuleCollider>();
        foreach (CapsuleCollider col in colliders)
        {
            if (!col.isTrigger)
            {
                _collider = col;
                break;
            }
        }

        // NavMeshAgent의 기본 설정값 저장
        _normalSpeed = _agent.speed;
        _normalAcceleration = _agent.acceleration;
        _normalStoppingDistance = _agent.stoppingDistance;
    }

    /// <summary>
    /// 실제 공격
    /// </summary>
    protected override void Attack()
    {
        StartCoroutine(Dash());
    }

    /// <summary>
    /// 플레이어 위치 너머로 돌진
    /// </summary>
    private IEnumerator Dash()
    {
        // 몬스터를 Rigidbody의 물리 효과를 받지 않는 상태로 만들고 NavMeshAgent의 움직임을 우선시
        if (_rigidBody != null)
            _rigidBody.isKinematic = true;
        if (_collider != null)
            _collider.enabled = false;

        // NavMeshAgent의 기본 설정값이 아닌 돌진 용도로 설정된 값으로 변경
        _agent.stoppingDistance = 0f;
        _agent.acceleration = dashAcceleration;
        _agent.speed = dashSpeed;
        _agent.updateRotation = true;  // 돌진 방향을 바라보도록 설정
        _agent.isStopped = false;

        // 돌진 방향, 거리 계산
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = _player.position;
        Vector3 dashDir = (targetPosition - startPosition).normalized;
        dashDir.y = 0;
        Vector3 dashDestination = targetPosition + dashDir * dashDistance;

        // SamplePosition을 사용해서 NavMesh가 없는 곳이 아닌 실제로 갈 수 있는 NavMesh의 위치 중 dashDestination과 가장 가까운 위치를 최종 목표 지점으로 설정
        NavMesh.SamplePosition(dashDestination, out NavMeshHit hit, dashDistance, NavMesh.AllAreas);
        Vector3 finalDestination = hit.position;

        // 최종 목표 지점으로 돌진
        _agent.SetDestination(finalDestination);

        // 돌진은 finalDestination에 도착해야 끝나는 것이 아니라 dashDuration(0.5초)만큼만 실행
        yield return new WaitForSeconds(dashDuration);

        // 돌진 종료
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            // Rigidbody 활성화 전에 NavMeshAgent의 속도를 강제로 0으로 만들어 미끄러짐 방지
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
            _agent.ResetPath(); // 경로 초기화
        }

        // 다시 물리 효과를 받고 충돌할 수 있도록 복구 (돌진이 끝났으면 다시 벽에 부딪히고 플레이어와 충돌할 수 있도록)
        if (_rigidBody != null)
            _rigidBody.isKinematic = false;
        if (_collider != null)
            _collider.enabled = true;

        // NavMeshAgent의 기본값으로 복구
        _agent.speed = _normalSpeed;
        _agent.acceleration = _normalAcceleration;
        _agent.stoppingDistance = _normalStoppingDistance;

        // 공격 종료(부모 클래스)
        FinishAttack();
    }
}