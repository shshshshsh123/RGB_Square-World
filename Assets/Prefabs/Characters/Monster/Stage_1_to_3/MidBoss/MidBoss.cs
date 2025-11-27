using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MidBoss : MonoBehaviour
{
    [Header("보스 설정")]

    [Tooltip("공격 쿨타임")]
    [SerializeField] public float _attackCoolDown = 3.0f;

    [Tooltip("이동 속도")]
    [SerializeField] public float _moveSpeed = 4.0f;

    [Tooltip("추적 시작 거리")]
    [SerializeField] public float _chaseRange = 15f;

    [Tooltip("최대 체력")]
    [SerializeField] public float _maxHealth = 100f;

    [Tooltip("보스 체력이 이 이하일 때 광폭화")]
    [SerializeField] public float _rageThreshold = 30f; // 체력이 이 수치 이하일 때 광폭화

    private float _lastAttackTime;
    private float _stoppingDistance = 2.0f; // 공격하기 전 멈추는 거리
    private float _currentHealth;

    private bool _isAttacking = false;
    private bool _isEnraged = false;

    private Transform _player;
    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

        _currentHealth = _maxHealth;

        // NavMeshAgent의 기본 설정값 저장
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
        if (_player == null)
            return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // 공격 중이거나 포효 중일 때는 움직임 중단
        if (_isAttacking || (_isEnraged && _animator.GetCurrentAnimatorStateInfo(0).IsName("Roar")))
        {
            _navMeshAgent.isStopped = true;
            return;
        }

        // 1. 추적 로직 (거리가 범위 안이고, 공격 범위보다 멀 때)
        if (distanceToPlayer <= _chaseRange && distanceToPlayer > _stoppingDistance)
        {
            ChasePlayer();
        }
        // 2. 공격 로직 (공격 범위 안일 때)
        else if (distanceToPlayer <= _stoppingDistance)
        {
            StopAndAttack();
        }
        // 3. 대기 (범위 밖)
        else
        {
            Idle();
        }
    }

    void ChasePlayer()
    {
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(_player.position);
        _animator.SetBool("isRunning", true); // Run 애니메이션 재생
    }

    void StopAndAttack()
    {
        // 멈춤
        _navMeshAgent.isStopped = true;
        _animator.SetBool("isRunning", false); // Idle 상태로 전환 후 공격 준비

        // 쿨타임 체크
        if (Time.time - _lastAttackTime > _attackCoolDown)
        {
            StartCoroutine(PerformAttack());
        }
    }

    void Idle()
    {
        _navMeshAgent.isStopped = true;
        _animator.SetBool("isRunning", false);
    }

    IEnumerator PerformAttack()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;

        // 플레이어를 바라봄 (갑자기 뒤돌아서 때리기 위해)
        transform.LookAt(_player);

        // 랜덤 공격 (0: 360도 공격, 1: 발차기)
        int randomPattern = Random.Range(0, 2);
        _animator.SetInteger("attackIndex", randomPattern);
        _animator.SetTrigger("doAttack");

        // 애니메이션이 시작될 틈을 줌
        yield return new WaitForSeconds(0.2f);

        // 공격 애니메이션 길이만큼 대기 (대략적인 시간, 필요시 조절)
        float attackDuration = 1.5f;
        yield return new WaitForSeconds(attackDuration);

        _isAttacking = false;
    }

    // 데미지 받는 함수 (외부에서 호출 필요)
    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        Debug.Log("Boss HP: " + _currentHealth);

        // 체력이 낮아지면 2페이즈 진입 (한 번만 실행)
        if (_currentHealth <= _rageThreshold && !_isEnraged)
        {
            StartCoroutine(EnterEnragePhase());
        }
    }

    IEnumerator EnterEnragePhase()
    {
        _isEnraged = true;
        _isAttacking = true; // 포효 중 다른 행동 금지

        // 움직임 멈춤 및 포효
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger("doRoar");
        Debug.Log("BOSS ENRAGED! Speed UP!");

        // 포효 애니메이션 시간만큼 대기 (예: 2.5초)
        yield return new WaitForSeconds(2.5f);

        // 속도 2배 증가
        _navMeshAgent.speed = _moveSpeed * 2;
        _animator.speed = 1.5f; // 애니메이션 속도도 약간 빠르게 (박진감)

        _isAttacking = false;
    }
}