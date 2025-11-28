using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class MidBoss : MonoBehaviour
{
    [Header("보스 설정")]

    [Tooltip("공격 쿨타임")]
    [SerializeField] public float _attackCoolDown = 3.0f;

    [Tooltip("일반 이동 속도")]
    [SerializeField] public float _moveSpeed = 4.0f;

    [Tooltip("공격 시 이동 속도")]
    [SerializeField] public float _dashSpeed = 12.0f;

    [Tooltip("플레이어를 추적하기 시작하는 거리")]
    [SerializeField] public float _chaseRange = 15f;

    [Tooltip("광폭화 발동 HP 비율")]
    [Range(0, 100)]
    [SerializeField] public float _ragePercentage = 30f;

    private float _lastAttackTime;
    private float _stoppingDistance = 4f; // 플레이어 앞에서 멈추는 거리

    private bool _isAttacking = false;
    private bool _isRaged = false; // 광폭화 상태인지 여부
    private bool _hasDealtDamage = false; // 공격으로 데미지를 주었는지 여부 (중복 방지)

    private Transform _player;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private CapsuleCollider _collider;
    private MonsterStatus _monsterStatus;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<CapsuleCollider>();
        _monsterStatus = GetComponent<MonsterStatus>();

        _navMeshAgent.speed = _moveSpeed;
        _navMeshAgent.stoppingDistance = _stoppingDistance;

        if (_player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
                _player = playerObject.transform;
        }
    }

    void Update()
    {
        if (!_isRaged && _monsterStatus != null && _monsterStatus.hpSlider != null)
        {
            // 최대 체력
            float maxHp = _monsterStatus.hpSlider.maxValue;

            // 현재 체력
            float currentHp = _monsterStatus.hpSlider.value;

            // 광폭화 발동 체력
            float thresholdHp = maxHp * (_ragePercentage / 100f);

            if (currentHp <= thresholdHp)
            {
                StartCoroutine(EnterRagePhase());
            }
        }

        if (_player == null)
            return;

        // 공격 중이거나 광폭화 중이면 다른 행동 X
        if (_isAttacking || (_isRaged && _animator.GetCurrentAnimatorStateInfo(0).IsName("Roar")))
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // 추격 범위 내이고 정지 거리 밖이면 추격
        if (distanceToPlayer <= _chaseRange && distanceToPlayer > _stoppingDistance)
        {
            ChasePlayer();
        }

        // 정지 거리 이내면 멈추고 공격
        else if (distanceToPlayer <= _stoppingDistance)
        {
            StopAndAttack();
        }

        // 추격 범위 밖이면 대기
        else
        {
            Idle();
        }
    }

    /// <summary>
    /// 플레이어 추격
    /// </summary>
    void ChasePlayer()
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
            return;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(_player.position);
        _animator.SetBool("isRunning", true);
    }

    /// <summary>
    /// 플레이어 공격
    /// </summary>
    void StopAndAttack()
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
            return;

        _navMeshAgent.isStopped = true;
        _animator.SetBool("isRunning", false);

        if (Time.time - _lastAttackTime > _attackCoolDown)
        {
            StartCoroutine(PerformAttack());
        }
    }

    /// <summary>
    /// 대기
    /// </summary>
    void Idle()
    {
        if (!_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
            return;
        _navMeshAgent.isStopped = true;
        _animator.SetBool("isRunning", false);
    }

    /// <summary>
    /// 실제 공격 실행 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator PerformAttack()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _hasDealtDamage = false;

        if (_player != null)
        {
            Vector3 targetPosition = new Vector3(_player.position.x, transform.position.y, _player.position.z);
            transform.LookAt(targetPosition); // 플레이어 방향으로 회전
        }

        int attackIndex = Random.Range(0, 2); // 랜덤 공격 (0이면 - 점프 후 내려찍기, 1이면 점프 후 발차기)
        _animator.SetInteger("attackIndex", attackIndex);
        _animator.SetTrigger("doAttack");

        // 딜레이 기본값
        float startDelay = 0.4f;
        float dashDuration = 0.4f;

        // 공격 패턴에 따라 애니메이션이 다르므로 딜레이 각각 조정
        switch (attackIndex)
        {
            case 0:
                startDelay = 0.4f;
                dashDuration = 0.4f;
                break;
            case 1:
                startDelay = 0.2f;
                dashDuration = 0.6f;
                break;
        }

        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(DashMove(dashDuration));
        yield return new WaitForSeconds(1.0f);

        _isAttacking = false;

        if (_navMeshAgent != null)
        {
            if (!_navMeshAgent.enabled)
            {
                _navMeshAgent.enabled = true;
            }

            if (_navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = false;
            }
        }
    }

    /// <summary>
    /// 가만히 서서 공격하지 않고 플레이어를 지나서 공격하기 위한 코루틴
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator DashMove(float duration)
    {
        // NavMeshAgent 비활성화
        _navMeshAgent.enabled = false;

        // 콜라이더를 트리거로 변경
        if (_collider != null)
            _collider.isTrigger = true;

        float elapsedTime = 0f;
        Vector3 dashDirection = transform.forward;

        while (elapsedTime < duration)
        {
            transform.Translate(dashDirection * _dashSpeed * Time.deltaTime, Space.World);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 콜라이더 설정 복구
        if (_collider != null)
            _collider.isTrigger = false;

        // NavMeshAgent 활성화
        _navMeshAgent.enabled = true;
    }

    /// <summary>
    /// 체력이 일정 비율 이하로 떨어지면 광폭화
    /// </summary>
    /// <returns></returns>
    IEnumerator EnterRagePhase()
    {
        _isRaged = true;
        _isAttacking = true;
        if (_navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
        }
        _animator.SetTrigger("doRoar");

        yield return new WaitForSeconds(2.5f);

        // 보스 스탯 강화
        _moveSpeed *= 1.5f;
        _dashSpeed *= 1.3f;
        _navMeshAgent.speed = _moveSpeed;
        _animator.speed = 1.3f;
        _attackCoolDown *= 0.7f;

        _isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isAttacking && !_hasDealtDamage && other.CompareTag("Player"))
        {
            _hasDealtDamage = true; // 데미지 처리 완료 (중복 방지)
        }
    }
}