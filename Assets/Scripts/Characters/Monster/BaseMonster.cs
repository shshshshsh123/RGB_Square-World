using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("몬스터 공통 설정")]

    [Tooltip("공격 사거리")]
    [SerializeField] protected float _attackRange = 3f;

    [Tooltip("공격 쿨타임")]
    [SerializeField] protected float _attackCoolDown = 3f;

    [Tooltip("플레이어 위치 갱신 주기")]
    [SerializeField] protected float _pathUpdateTime = 0.2f;

    protected float _playerDistance; // 플레이어와 몬스터 사이의 거리
    protected bool _canAttack = true; // 공격 가능 여부
    protected bool _isAttacking = false; // 공격 중 여부
    
    protected Transform _player;
    protected NavMeshAgent _agent;
    protected Animator _animator;

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                _player = playerObject.transform;
        }

        // NavMeshAgent 활성화
        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.isStopped = false;
        }

        // 공격 상태 초기화
        _canAttack = true;
        _isAttacking = false;

        // 몬스터 기본 행동 로직 실행
        StartCoroutine(EnemyLogic());
    }

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        
        if (playerObject != null)
            _player = playerObject.transform;
    }

    protected virtual void OnDisable()
    {
        // 모든 코루틴 즉시 정지
        StopAllCoroutines();

        // NavMeshAgent 정지 및 경로 초기화
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
        }
    }

    protected virtual void Update()
    {
        if (_animator == null || _agent == null)
            return;

        // NavMeshAgnet의 현재 속도
        float currentSpeed = _agent.velocity.magnitude;

        bool isMoving = currentSpeed > 0.1f;

        _animator.SetBool("isRunning", isMoving);
    }

    /// <summary>
    /// 몬스터 기본 행동 로직
    /// </summary>
    /// <returns></returns>
    protected IEnumerator EnemyLogic()
    {
        // 플레이어 위치 갱신 주기(0.2초) 마다 반복
        WaitForSeconds wait = new WaitForSeconds(_pathUpdateTime);

        while (true)
        {
            // 예외 처리
            if (_player == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                yield return wait;
                continue;
            }

            // 공격이 진행 중이라면 다른 행동을 할 수 없음
            if (_isAttacking)
            {
                yield return wait;
                continue;
            }

            _playerDistance = Vector3.Distance(transform.position, _player.position);

            // 플레이어가 공격 범위 안에 있고, 공격할 수 있다면 공격
            if (_playerDistance <= _attackRange && _canAttack)
                StartAttack();

            // 플레이어가 공격 범위 밖에 있고, 공격할 수 없다면 추격
            else
                Chase();

            yield return wait;
        }
    }

    /// <summary>
    /// 공격 시작
    /// </summary>
    protected virtual void StartAttack()
    {
        _canAttack = false;
        _isAttacking = true;

        Vector3 lookDir = (_player.position - transform.position).normalized;
        lookDir.y = 0;
        
        // 공격 직전 플레이어를 바라보도록 즉시 회전
        transform.rotation = Quaternion.LookRotation(lookDir);

        // 공격 애니메이션 재생
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }

        Attack();

        StartCoroutine(AttackCoolDown());
    }

    /// <summary>
    /// 플레이어 추격
    /// </summary>
    protected virtual void Chase()
    {
        _agent.updateRotation = true;

        if (_agent.isStopped)
            _agent.isStopped = false;

        _agent.SetDestination(_player.position);
    }

    /// <summary>
    /// 공격 쿨타임 계산
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(_attackCoolDown);
        _canAttack = true;
    }

    /// <summary>
    /// 공격 종료
    /// </summary>
    protected void FinishAttack()
    {
        _isAttacking = false;
    }

    /// <summary>
    /// 실제 공격 (상속받는 자식 클래스에서 직접 구현)
    /// </summary>
    protected abstract void Attack();
}