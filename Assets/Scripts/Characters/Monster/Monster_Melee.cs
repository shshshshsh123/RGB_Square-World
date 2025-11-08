using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Monster_Melee : BaseMonster
{
    [Header("근접 몬스터 돌진 관련 스탯")]
    public float dashSpeed = 7f; // 돌진 시 속도
    public float dashDuration = 0.5f; // 돌진 지속 시간
    public float dashAcceleration = 100f; // 돌진 시 가속도

    [Header("플레이어를 지나쳐 돌진할 추가 거리")]
    public float dashOvershootDistance = 3f;

    // NavMeshAgent의 기본 설정을 저장할 변수들
    private float _normalSpeed;
    private float _normalAcceleration;
    private float _normalStoppingDistance;

    // 물리 충돌을 제어하기 위한 컴포넌트
    private Rigidbody _rb;
    private CapsuleCollider _solidCollider; // 물리 충돌 콜라이더

    protected override void Awake()
    {
        base.Awake(); // 부모의 Awake() 먼저 호출

        // Rigidbody 컴포넌트 가져오기
        _rb = GetComponent<Rigidbody>();

        // 몬스터의 모든 캡슐 콜라이더를 가져와서
        CapsuleCollider[] colliders = GetComponents<CapsuleCollider>();
        foreach (CapsuleCollider col in colliders)
        {
            // 그 중에서 "트리거가 아닌" 콜라이더(물리 충돌용)를 _solidCollider로 저장
            if (!col.isTrigger)
            {
                _solidCollider = col;
                break;
            }
        }

        // NavMeshAgent의 원래 설정값들 저장
        _normalSpeed = _agent.speed;
        _normalAcceleration = _agent.acceleration;
        _normalStoppingDistance = _agent.stoppingDistance;
    }

    /// <summary>
    /// BaseMonster로부터 호출되는 실제 공격 구현부
    /// </summary>
    protected override void Attack()
    {
        StartCoroutine(Dash());
    }

    /// <summary>
    /// 플레이어를 지나쳐 돌진하는 공격 코루틴 (시간 기반)
    /// </summary>
    private IEnumerator Dash()
    {
        // --- 1. 돌진 준비 (물리 기능/충돌 끄기) ---
        if (_rb != null)
            _rb.isKinematic = true; // 물리 충돌이 NavMeshAgent를 방해하지 않도록 설정
        if (_solidCollider != null)
            _solidCollider.enabled = false; // 플레이어를 밀지 않고 통과하도록 함

        // NavMeshAgent를 돌진용 설정으로 변경
        _agent.stoppingDistance = 0f;
        _agent.acceleration = dashAcceleration;
        _agent.speed = dashSpeed;
        _agent.updateRotation = true; // 돌진 방향을 바라보도록 함
        _agent.isStopped = false;

        // --- 2. 돌진 목표 계산 (플레이어 너머) ---
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = _player.position;
        Vector3 dashDir = (targetPosition - startPosition).normalized;
        dashDir.y = 0;
        Vector3 overshootPosition = targetPosition + dashDir * dashOvershootDistance;
        NavMesh.SamplePosition(overshootPosition, out NavMeshHit hit, dashOvershootDistance, NavMesh.AllAreas);
        Vector3 finalDestination = hit.position;

        // --- 3. 돌진 실행 ---
        _agent.SetDestination(finalDestination);

        // --- 4. 돌진 종료 대기 (시간 기반) ---
        // 이 시간 동안 물리/충돌이 꺼진 상태로 돌진합니다.
        yield return new WaitForSeconds(dashDuration);

        // --- 5. 돌진 종료 및 정리 (★★ 핵심 수정 ★★) ---
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            // ★ Rigidbody를 켜기 전에 속도를 강제로 0으로 만들어 미끄러짐 방지
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
            _agent.ResetPath(); // 현재 경로 초기화
        }

        if (_rb != null)
            _rb.isKinematic = false; // 이제 속도가 0인 상태로 켜집니다.
        if (_solidCollider != null)
            _solidCollider.enabled = true; // 다시 물리 충돌이 가능하도록 복구

        // NavMeshAgent 설정을 원래대로 복구
        _agent.speed = _normalSpeed;
        _agent.acceleration = _normalAcceleration;
        _agent.stoppingDistance = _normalStoppingDistance;

        // [중요] 부모 클래스에 "공격 끝났음"을 알림
        FinishAttack();
    }
}