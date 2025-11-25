using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// 수비형 원거리 몬스터 - 기본 원거리 몬스터와는 달리, 전투 중 플레이어와의 거리를 일정하게 유지

public class Monster_Range_Defensive : BaseMonster
{
    [Header("수비형 원거리 몬스터 설정")]

    [Tooltip("플레이어와 유지하려는 최소 거리")]
    [SerializeField] private float _minSafeDistance = 7f;

    [Tooltip("도망치는 속도")]
    [SerializeField] private float _runAwaySpeed = 4f;

    [Tooltip("발사체의 발사 위치")]
    [SerializeField] private Transform _firePos;

    [Tooltip("풀 타입(오브젝트 풀러)")]
    [SerializeField] private PoolType _projectilePoolType;

    [Tooltip("도망갈 때 벽이 있는지 감지하기 위한 레이어")]
    [SerializeField] private LayerMask _obstacleMask = -1;

    private float _fireAnimationTime = 0.5f; // 공격 애니메이션 총 재생 시간
    private float _fireDelay = 0.3f; // 애니메이션 시작 후, 실제로 발사체가 나가는 순간
    private float _normalSpeed; // NavMeshAgent 기본 속도
    private bool _isRunAway = false; // 도망 중인지 체크
    private float _runAwayBuffer = 3.0f; // 여유 거리 (플레이어와 확실히 멀어지기 위해)
    private Rigidbody _rigidbody;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        if (_agent != null)
        {
            _normalSpeed = _agent.speed; // NavMeshAgent 기본 속도 저장
        }
    }

    /// <summary>
    /// 몬스터가 공격을 할지 도망을 갈지 판단
    /// </summary>
    protected override void StartAttack()
    {
        // [도망]
        if (_isRunAway)
        {
            // 플레이어에게서 충분히 멀어졌으면 더이상 도망가지 않음
            if (_playerDistance > _minSafeDistance + _runAwayBuffer)
            {
                _isRunAway = false;
            }

            // 플레이어에게서 충분히 멀어지지 못했다면
            else
            {
                // 도주 경로가 벽에 막혀있으므로 더이상 도망가지 않고 공격
                if (!RunAway())
                {
                    _isRunAway = false;
                }

                // 도주 경로가 벽에 막혀있지 않으므로 계속해서 도망
                else
                {
                    return;
                }
            }
        }

        // 도망 중이지는 않지만 플레이어와의 거리가 너무 가깝다면
        else if (_playerDistance < _minSafeDistance)
        {
            // 도주 경로가 벽에 막혀있으므로 더이상 도망가지 않고 공격
            if (!RunAway())
            {
                
            }

            // 도주 경로가 벽에 막혀있지 않으므로 계속해서 도망
            else
            {
                _isRunAway = true;
                return;
            }
        }

        // [시야 확인]
        // (플레이어가 보이는지 체크)
        if (!isObjectInFront())
        {
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.updateRotation = true;
                _agent.isStopped = false;
                _agent.SetDestination(_player.position); // 플레이어가 보이는 곳으로 이동
            }
            return;
        }

        // [공격]
        // (위의 로직을 모두 통과했을 때)

        // NavMeshAgent의 회전, 이동 기능 모두 비활성화
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.updateRotation = false;
        }

        // 플레이어가 있는 방향을 계산하고 회전
        Vector3 lookDir = (_player.position - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDir);

        // 공격 중 밀림 방지
        if (_rigidbody != null)
            _rigidbody.isKinematic = true;

        // 공격 애니메이션 재생
        base.StartAttack();
    }

    protected override void Attack()
    {
        StartCoroutine(Fire());
    }

    /// <summary>
    /// 플레이어에게 발사체(빵) 발사
    /// </summary>
    /// <returns></returns>
    private IEnumerator Fire()
    {
        // 0.3초 대기 (발사체가 발사 순간과 던지는 애니메이션을 일치)
        yield return new WaitForSeconds(_fireDelay);

        // 발사체 생성
        if (_firePos == null)
            Debug.LogError(gameObject.name + ": 발사 위치가 설정되지 않았습니다!");
        else
        {
            GameObject projectileObject = ObjectPooler.Instance.SpawnFromPool(_projectilePoolType, _firePos.position, _firePos.rotation);
        }

        // 나머지 시간(0.2초)를 기다려서 총 애니메이션 시간(0.5초)과 맞춤
        float remainAnimationTime = _fireAnimationTime - _fireDelay;
        if (remainAnimationTime > 0)
        {
            yield return new WaitForSeconds(remainAnimationTime);
        }

        // Rigidbody 활성화
        if (_rigidbody != null) _rigidbody.isKinematic = false;

        // 공격 종료(부모 클래스)
        FinishAttack();
    }

    /// <summary>
    /// 몬스터가 도망칠 경로를 계산
    /// </summary>
    /// <returns>도망칠 수 있으면 True, 도망칠 수 없으면 False를 반환</returns>
    private bool RunAway()
    {
        // 플레이어 반대 방향 계산
        Vector3 runAwayDir = (transform.position - _player.position).normalized;
        float checkDistance = 10f;

        // Ray는 눈높이 보다 살짝 위
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        // Raycast로 벽 감지
        if (Physics.Raycast(rayOrigin, runAwayDir, out RaycastHit hit, checkDistance, _obstacleMask))
        {
            // 벽이 2f 보다 가까이 있으면 도망칠 곳이 없다고 판단하고 공격
            if (hit.distance < 2.0f)
            {
                return false;
            }

            // 벽이 있지만 공간은 좀 있다면? -> 벽 앞까지만 이동
            float finalDistance = hit.distance - 1.0f;
            if (finalDistance < 0.5f) finalDistance = 0.5f;

            Vector3 safePosition = transform.position + runAwayDir * finalDistance;

            // 도망칠 곳으로 이동
            _agent.speed = _runAwaySpeed;
            _agent.updateRotation = true;
            _agent.isStopped = false;

            // NavMesh 위에서 갈 수 있는 유효한 좌표인지 확인 후 이동
            if (NavMesh.SamplePosition(safePosition, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
                return true;
            }
        }
        // 벽이 감지되지 않는다면 10f 이동
        else
        {
            Vector3 safePosition = transform.position + runAwayDir * 10f;
            _agent.speed = _runAwaySpeed;
            _agent.updateRotation = true;
            _agent.isStopped = false;

            // NavMesh 위에서 갈 수 있는 유효한 좌표인지 확인 후 이동
            if (NavMesh.SamplePosition(safePosition, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(navHit.position);
                return true;
            }
        }
        return false; // 경로 계산 실패 시
    }

    /// <summary>
    /// 플레이어 추격
    /// </summary>
    protected override void Chase()
    {
        // 기본 속도로 복구
        if (_agent.speed != _normalSpeed)
            _agent.speed = _normalSpeed;

        float dist = Vector3.Distance(transform.position, _player.position);

        if (_isRunAway)
        {
            // 플레이어에게서 충분히 멀어졌으면 더이상 도망가지 않음
            if (dist > _minSafeDistance + _runAwayBuffer)
                _isRunAway = false;

            // 도주 경로가 벽에 막혀있으므로 더이상 도망가지 않고 공격
            else
            {
                if (!RunAway())
                    _isRunAway = false;
                else
                    return;
            }
        }
        
        // 도망 중이지는 않지만 플레이어와의 거리가 너무 가깝다면
        else if (dist < _minSafeDistance)
        {
            if (!RunAway())
                _isRunAway = false;
            
            else
            {
                _isRunAway = true;
                return;
            }
        }

        // 사거리 안에 있고, 플레이어와 몬스터 사이에 장애물이 없다면 굳이 다가가지 않음
        if (dist <= _attackRange && isObjectInFront())
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
        
        // 사거리 밖이거나, 사거리 안이지만 플레이어와 몬스터 사이에 장애물이 있다면 플레이어를 향해 다가감
        else
        {
            _isRunAway = false;
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.updateRotation = true;
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
            }
        }
    }

    /// <summary>
    /// 몬스터 앞에 있는 장애물이 무엇인지 판단
    /// </summary>
    /// <returns>장애물이 없다면 True 반환, 장애물이 있다면 False 반환</returns>
    private bool isObjectInFront()
    {
        if (_player == null)
            return false;

        // Ray는 눈높이 보다 살짝 위
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 target = _player.position + Vector3.up * 1.0f;

        // 플레이어로의 거리와 방향
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        // Ray를 쏴서 장애물에 걸리는지 확인
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _obstacleMask))
        {
            // 장애물이 가로막고 있다면 false 반환
            if (hit.transform != _player)
            {
                return false; // 시야 막힘
            }
        }
        // 장애물이 가로막고 있지 않다면 true 반환
        return true;
    }
}
