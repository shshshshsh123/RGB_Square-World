using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MiddleBoss : MonoBehaviour
{
    // 보스 상태
    public enum bossState
    {
        Idle,   // 대기 (기본 상태)
        Chase,  // 플레이어 추적
        Attack  // 모든 공격 패턴을 포함하는 단일 상태 (내부에서 코루틴 실행)
    }
    public bossState currentState = bossState.Idle; // 보스 상태를 기본 상태로 설정

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigidBody;
    public Transform _playerTransform;

    [Header("보스 설정")]
    public float chaseRange = 15f;  // 플레이어 인식 범위
    public float attackRange = 10f; // 공격 사거리
    public float dashRange = 5f;   // 돌진 스킬 공격 시작 최소 거리

    [Header("돌진 스킬")]
    public float dashSpeed = 30f; // 돌진 속도

    [Header("광역 스킬")]

    [Header("집중 스킬")]

    [Header("소환 스킬")]
    public GameObject rangeMonsterPrefab; // 원거리 몬스터 프리팹
    public Transform[] summonPoints; // 소환 위치 배열

    // AI 제어용 변수
    private bool _isAttacking = false; // 현재 공격 중인지 확인 (중복 실행 방지)
    private float _decisionCooldown = 0.5f; // 0.5초마다 행동 결정 (성능 최적화)

    void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidBody = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        // NavMeshAgent가 Rigidbody를 제어하도록 초기 설정 (필수)
        _rigidBody.isKinematic = true;

        if (_playerTransform == null)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Update()와 별개로 작동하여 AI의 행동주기를 최적화
        StartCoroutine(AIBehaviorCoroutine());
    }

    IEnumerator AIBehaviorCoroutine()
    {
        while (true)
        {
            // 공격 중이 아닐때는 플레이어를 계속해서 바라 봄
            if (!_isAttacking && _playerTransform != null)
            {
                FacePlayer();
            }

            // 공격 중이거나 플레이어가 없으면 아무것도 하지 않음
            if (_isAttacking || _playerTransform == null)
            {
                yield return new WaitForSeconds(_decisionCooldown);
                continue;
            }

            // 플레이어와의 거리를 계산
            float distance = Vector3.Distance(transform.position, _playerTransform.position);

            if (distance > chaseRange)
            {
                // [대기]
                currentState = bossState.Idle;
                _navMeshAgent.isStopped = true;
            }
            else if (distance > attackRange)
            {
                // [추적]
                currentState = bossState.Chase;
                _navMeshAgent.isStopped = false;

                // NavMesh 위에서 목표 위치(플레이어)까지의 경로를 계산하고 이동을 시작합니다.
                _navMeshAgent.SetDestination(_playerTransform.position);
            }
            else
            {
                // [공격] - 플레이어가 공격 범위 내
                currentState = bossState.Attack;
                _navMeshAgent.isStopped = true;

                // 어떤 공격을 할지 결정
                ChooseAttackPattern(distance);
            }

            // 0.5초(_decisionCoolDown)마다 AI가 판단
            yield return new WaitForSeconds(_decisionCooldown);
        }
    }

    void ChooseAttackPattern(float distance)
    {
        _isAttacking = true; // 공격 상태 설정 (중복 실행 방지)

        // 플레이어가 돌진 범위 내에 있을 때 돌진
        if (distance >= dashRange)
        {
            StartCoroutine(DashAttack());
        }
    }

    /// <summary>
    /// 공격 시작 전 플레이어를 바라보도록 설정
    /// </summary>
    void FacePlayer()
    {
        Debug.Log("플레이어를 바라봅니다.");
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        direction.y = 0; // 보스가 위아래로 기울어지지 않도록 y축 고정

        // 지정된 '앞쪽' 방향을 바라보는 회전값(Quaternion)을 생성
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; // 즉시 회전 (Slerp를 사용해 부드럽게 회전 가능)
    }

    /// <summary>
    /// [돌진 공격]: 플레이어에게 순간적으로 돌진하여 근접에서 지휘봉으로 공격
    /// </summary>
    /// <returns></returns>
    IEnumerator DashAttack()
    {
        Debug.Log("플레이어에게 순간적으로 돌진하여 근접에서 지휘봉으로 공격");
        FacePlayer();
        yield return new WaitForSeconds(0.8f); // 0.8초 대기

        // NavMeshAgent -> Rigidbody
        _navMeshAgent.enabled = false;
        _rigidBody.isKinematic = false;

        // 돌진 실행
        // ForceMode.Impulse로 순간적인 돌진
        _rigidBody.AddForce(transform.forward * dashSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f); // 0.5초간 돌진

        // 돌진 정지 및 지휘봉 공격
        _rigidBody.linearVelocity = Vector3.zero; // 속도 강제 정지

        yield return new WaitForSeconds(1.0f); // 공격 후 딜레이;

        // Rigidbody -> NavMeshAgent
        _rigidBody.isKinematic = true;
        _navMeshAgent.enabled = true;

        _isAttacking = false; // 공격 상태 잠금 해제
    }

    // [스킬 공격]: 확성기를 통해 소리 지르며 광역 공격 
    IEnumerator SkillAoE()
    {
        Debug.Log("확성기를 통해 소리 지르며 광역 공격");
        yield return new WaitForSeconds(1.5f); // 1.5초 캐스팅
        yield return new WaitForSeconds(1.0f); // 후 딜레이
        _isAttacking = false;
    }

    // [패턴 3: 집중 공격 (거대 분필)]
    IEnumerator FocusAttack()
    {
        Debug.Log("거대 분필로 공격");
        yield return new WaitForSeconds(2.0f); // 2초 캐스팅)
        yield return new WaitForSeconds(1.5f); // 후 딜레이
        _isAttacking = false;
    }

    // [패턴 4: 부하 소환 (선도위원)]
    IEnumerator Summon()
    {
        Debug.Log("원거리 몬스터 소환");
        yield return new WaitForSeconds(1.0f); // 캐스팅

        foreach (Transform point in summonPoints)
        {
            if (rangeMonsterPrefab)
            {
                // 원거리 몬스터 프리팹(선도위원)을 소환
                Instantiate(rangeMonsterPrefab, point.position, point.rotation);
                // (성능: 역시 '오브젝트 풀링'을 사용하는 것이 좋습니다.)
            }
            yield return new WaitForSeconds(0.3f); // 0.3초 간격으로 순차 소환
        }

        yield return new WaitForSeconds(1.0f); // 후 딜레이
        _isAttacking = false;
    }
}