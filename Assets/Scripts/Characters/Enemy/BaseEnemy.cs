using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("기본 스탯")]
    protected float _attackRange = 2.5f;
    protected float _attackCooldown = 2f;
    protected float _pathUpdateTime = 0.2f; // 플레이어 위치 갱신 시간

    protected float _playerDistance;
    protected bool _canAttack = true; // 공격 가능 여부
    protected bool _isAttacking = false; // 공격 중 여부
    
    protected Transform _player;
    protected NavMeshAgent _agent;

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        
        if (playerObject != null)
            _player = playerObject.transform;

        if (_agent == null) _agent = GetComponent<NavMeshAgent>();

        StartCoroutine(EnemyLogic());
    }

    /// <summary>
    /// 몬스터 기본 행동 로직
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemyLogic()
    {
        WaitForSeconds wait = new WaitForSeconds(_pathUpdateTime);

        while (true)
        {
            if (_player == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                yield return wait;
                continue;
            }

            if (_isAttacking)
            {
                yield return wait;
                continue;
            }

            _playerDistance = Vector3.Distance(transform.position, _player.position);

            if (_playerDistance <= _attackRange && _canAttack)
                Stop();
            else
                Chase();

            yield return wait;
        }
    }

    /// <summary>
    /// 공격을 시작할 때 호출되는 함수
    /// </summary>
    protected virtual void Stop()
    {
        _canAttack = false;
        _agent.isStopped = true;
        _isAttacking = true;

        Vector3 lookDir = (_player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        Attack();

        StartCoroutine(AttackCooldown());
    }

    /// <summary>
    /// 플레이어를 추격할 때 호출되는 함수
    /// </summary>
    protected virtual void Chase()
    {
        _agent.updateRotation = true;

        if (_agent.isStopped)
            _agent.isStopped = false;

        _agent.SetDestination(_player.position);
    }

    /// <summary>
    /// 공격 쿨타임을 계산하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(_attackCooldown);
        _canAttack = true;
    }

    /// <summary>
    /// 
    /// </summary>
    protected void FinishAttack()
    {
        _isAttacking = false;
    }

    /// <summary>
    /// 상속 받는 자식 클래스에서 구현해야 하는 함수
    /// </summary>
    protected abstract void Attack();
}