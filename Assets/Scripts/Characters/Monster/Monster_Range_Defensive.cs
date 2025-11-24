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
    [SerializeField] private float _runAwaySpeed = 4f;

    [Tooltip("발사체의 발사 위치")]
    [SerializeField] private Transform _firePos;

    [Tooltip("풀 타입(오브젝트 풀러)")]
    [SerializeField] private PoolType _projectilePoolType;

    [Tooltip("도망갈 때 벽으로 인식할 레이어 (Player, Monster 제외 권장)")]
    [SerializeField] private LayerMask _obstacleMask = -1;

    private float _fireAnimationTime = 0.5f; // 공격 애니메이션 총 재생 시간
    private float _fireDelay = 0.3f; // 애니메이션 시작 후, 실제로 발사체가 나가는 순간
    private float _normalSpeed; // NavMeshAgent 기본 속도
    private bool _isRetreating = false; // 도망 중인지 체크
    private float _retreatBuffer = 3.0f; // 
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
        // 1. 도망 로직 (Cornered Beast 패턴 적용)
        if (_isRetreating)
        {
            // 충분히 멀어졌으면 도망 끝
            if (_playerDistance > _minSafeDistance + _retreatBuffer)
            {
                _isRetreating = false;
            }
            else
            {
                // 계속 도망 시도, 하지만 벽에 막혔다면(!Retreat) -> 도망 포기하고 공격
                if (!Retreat())
                {
                    _isRetreating = false; // 도망 모드 강제 종료
                    // 여기서 return을 안 하므로 -> 아래 공격 로직이 바로 실행됨 (무조건 공격)
                }
                else
                {
                    return; // 도망 잘 가고 있으니 공격 스킵
                }
            }
        }
        else if (_playerDistance < _minSafeDistance)
        {
            // 도망 시도, 하지만 벽에 막혔다면 -> 바로 공격
            if (!Retreat())
            {
                // 아무것도 안 함 (아래 공격 로직으로 흐름)
            }
            else
            {
                _isRetreating = true;
                return; // 도망 시작했으니 공격 스킵
            }
        }

        // 2. 시야 체크 (벽 뒤에 숨어서 도망도 못 가는데 안 보이기까지 하면 -> 앞으로 나가야 함)
        if (!HasLineOfSight())
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.updateRotation = true;
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }
            return;
        }

        // 3. 공격 실행 (벽에 막혔을 때 여기로 옴)
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.updateRotation = false;
        }

        Vector3 lookDir = (_player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (_rigidbody != null) _rigidbody.isKinematic = true;

        base.StartAttack();
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

    private bool Retreat()
    {
        Vector3 retreatDir = (transform.position - _player.position).normalized;
        float checkDistance = 10f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        // 벽 감지
        if (Physics.Raycast(rayOrigin, retreatDir, out RaycastHit hit, checkDistance, _obstacleMask))
        {
            // [핵심] 벽이 2m보다 가까이 있으면 "도망 갈 공간 없음"으로 판단
            if (hit.distance < 2.0f)
            {
                return false; // 도망 실패! (이제 공격해야 함)
            }

            // 벽이 있지만 공간은 좀 있다면? -> 벽 앞까지만 이동
            float finalDistance = hit.distance - 1.0f;
            if (finalDistance < 0.5f) finalDistance = 0.5f;

            Vector3 safePosition = transform.position + retreatDir * finalDistance;

            // 이동 실행
            _agent.speed = _runAwaySpeed;
            _agent.updateRotation = true;
            _agent.isStopped = false;

            if (NavMesh.SamplePosition(safePosition, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
                return true; // 도망 성공
            }
        }
        else // 벽이 감지되지 않음 (뻥 뚫림)
        {
            Vector3 safePosition = transform.position + retreatDir * 10f;
            _agent.speed = _runAwaySpeed;
            _agent.updateRotation = true;
            _agent.isStopped = false;

            if (NavMesh.SamplePosition(safePosition, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
                return true; // 도망 성공
            }
        }

        return false; // 혹시라도 경로 계산 실패 시
    }

    // --- 5. Chase() 로직 오버라이드 (도망 속도 복구) ---
    protected override void Chase()
    {
        if (_agent.speed != _normalSpeed) _agent.speed = _normalSpeed;
        float dist = Vector3.Distance(transform.position, _player.position);

        // A. 도망 로직
        if (_isRetreating)
        {
            if (dist > _minSafeDistance + _retreatBuffer) _isRetreating = false;
            else
            {
                // 벽에 막히면? -> 도망 포기하고 대기 모드(B)로 전환
                if (!Retreat()) _isRetreating = false;
                else return;
            }
        }
        // 안전거리 침범 시 도망 시도
        else if (dist < _minSafeDistance)
        {
            // 벽에 막히면? -> 도망 포기하고 대기 모드(B)로 전환
            if (!Retreat()) _isRetreating = false;
            else
            {
                _isRetreating = true;
                return;
            }
        }

        // B. 대기 모드 (사거리 안 + 시야 있음) OR (벽에 막혀서 도망 못 감)
        if (dist <= _attackRange && HasLineOfSight())
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
                _agent.updateRotation = false;
            }
            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
        }
        // C. 추격 모드
        else
        {
            _isRetreating = false;
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.updateRotation = true;
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }
        }
    }

    private bool HasLineOfSight()
    {
        if (_player == null) return false;

        // 몬스터 눈높이 (바닥보다 1m 위)
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        // 플레이어 눈높이
        Vector3 target = _player.position + Vector3.up * 1.0f;

        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        // 레이저를 쏴서 장애물에 걸리는지 확인
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _obstacleMask))
        {
            // 부딪힌 게 플레이어가 아니면 -> 벽이 가로막고 있다는 뜻
            if (hit.transform != _player)
            {
                return false; // 시야 막힘
            }
        }

        return true; // 시야 뚫림 (공격 가능)
    }
}
