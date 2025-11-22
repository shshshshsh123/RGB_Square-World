using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 수비형 원거리 몬스터 - 기본 원거리 몬스터와는 달리, 전투 중 플레이어와의 거리를 일정하게 유지

public class Monster_Range_Defensive : BaseMonster
{
    [Header("수비형 원거리 몬스터 설정")]

    [Tooltip("플레이어와 유지할 최소 거리")]
    [SerializeField] private float _minSafeDistance = 7f;

    [Tooltip("도망치는 속도")]
    [SerializeField] private float _runAwaySpeed = 5f;

    [Tooltip("발사체의 발사 위치")]
    [SerializeField] private Transform _firePos;

    [Tooltip("풀 타입(오브젝트 풀러)")]
    [SerializeField] private PoolType _projectilePoolType;

    private float _fireAnimationTime = 0.5f; // 공격 애니메이션 총 재생 시간
    private float _fireDelay = 0.3f; // 애니메이션 시작 후, 실제로 발사체가 나가는 순간
    private float _normalSpeed; // NavMeshAgent 기본 속도
    private Rigidbody _rigidbody;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        if (_agent != null)
        {
            _normalSpeed = _agent.speed;
        }
    }

    protected override void StartAttack()
    {
        // 플레이어가 너무 가까이 왔다면
        if (_playerDistance < _minSafeDistance)
        {
            Retreat();
        }
        // 2. [공격] 스윗 스팟 안에 있다면 (공격 사거리 내, 최소 도망 거리 밖)
        else
        {
            _agent.isStopped = true;
            if (_rigidbody != null) _rigidbody.isKinematic = true;
            base.StartAttack();
        }
    }

    // --- 3. 실제 공격 구현 (Monster_Range의 Fire() 코루틴 복사) ---
    protected override void Attack()
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        // Monster_Range의 Fire() 코드를 복사해서 사용합니다.

        // 1. 발사까지 대기
        yield return new WaitForSeconds(_fireDelay);

        // 2. 발사
        if (_firePos == null)
            Debug.LogError(gameObject.name + ": 발사 위치가 설정되지 않았습니다!");
        else
        {
            GameObject projectileObject = ObjectPooler.Instance.SpawnFromPool(_projectilePoolType, _firePos.position, _firePos.rotation);
        }

        // 3. 나머지 애니메이션 시간 대기
        float remainAnimationTime = _fireAnimationTime - _fireDelay;
        if (remainAnimationTime > 0)
        {
            yield return new WaitForSeconds(remainAnimationTime);
        }

        // 4. 물리 기능 복구
        if (_rigidbody != null) _rigidbody.isKinematic = false;

        // 5. 공격 완료 보고
        FinishAttack();
    }

    private void Retreat()
    {
        // 1. 플레이어 반대 방향 계산
        Vector3 retreatDirection = (transform.position - _player.position).normalized;

        // 2. 현재 위치에서 안전 거리 밖으로 도망갈 위치 계산
        Vector3 safePosition = transform.position + retreatDirection * 10f;

        // 3. NavMeshAgent 설정 변경 및 이동 명령
        _agent.speed = _runAwaySpeed;
        _agent.isStopped = false;

        if (NavMesh.SamplePosition(safePosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
        // 이 함수는 공격 상태(_isAttacking = true)를 시작하지 않으므로, 
        // BaseMonster의 EnemyLogic이 다음 루프에서 거리를 확인하고 다시 Chase()를 부릅니다.
    }

    // --- 5. Chase() 로직 오버라이드 (도망 속도 복구) ---
    protected override void Chase()
    {
        // 도망치는 중이 아니라면, 원래 속도로 복구
        if (_agent.speed != _normalSpeed)
        {
            _agent.speed = _normalSpeed;
        }

        base.Chase(); // 원래 추격 로직 실행
    }
}
