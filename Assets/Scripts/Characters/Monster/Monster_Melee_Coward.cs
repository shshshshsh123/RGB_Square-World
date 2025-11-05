using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Monster_Melee_Coward : BaseMonster
{
    [Header("근접 몬스터 돌진 관련 스탯")]
    public float dashSpeed = 10f; // 돌진 시 속도
    public float dashDuration = 0.5f; // 돌진 지속 시간
    public float dashAcceleration = 1000f; // 돌진 시 가속도

    private float _normalSpeed; // 기본 이동 속도
    private float _normalAcceleration; // 기본 가속도

    private float _runAwayHpRatio = 0.5f; // 도망 체력 비율(50%)
    private float _runAwayDuration = 3f; // 도망 지속 시간(5초)

    private bool _hasRunAway = false; // 중복 도망 방지용 변수 (도망은 최초 1회만)


    protected override void Awake()
    {
        base.Awake();
        _normalSpeed = _agent.speed;
        _normalAcceleration = _agent.acceleration;
    }

    protected override void Attack()
    {
        StartCoroutine(Dash());
    }

    /// <summary>
    /// 돌진 공격(플레이어에게 빠른 속도로 돌진 후 다시 돌진 시작 위치로 돌아오는 공격) 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dash()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = _player.position;

        _agent.updateRotation = true;
        _agent.acceleration = dashAcceleration;
        _agent.speed = dashSpeed;
        _agent.isStopped = false;
        _agent.SetDestination(targetPosition);

        yield return new WaitForSeconds(dashDuration);

        _agent.speed = _normalSpeed;
        _agent.acceleration = _normalAcceleration;
        _agent.SetDestination(startPosition);

        // 회전을 직접 제어할 수 있도록 NavMeshAgent의 자동 회전을 비활성화
        _agent.updateRotation = false;

        // 경로 계산 중에도 플레이어를 향하도록 회전
        while (_agent.pathPending)
        {
            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
            yield return null;
        }

        // 복귀(플레이어에게 돌진 공격 후 돌아옴)중에도 계속해서 플레이어를 향하도록 회전
        while (_agent.remainingDistance > _agent.stoppingDistance + 0.1f)
        {
            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
            yield return null;
        }

        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;
        }

        FinishAttack();
    }
    /// <summary>
    /// 도망 가능 상태 체크 함수 (Coward_Melee 용)
    /// </summary>
    /// <param name="currentHp"></param>
    /// <param name="maxHp"></param>
    /// 이미 도망친 적이 있거나, 체력이 50%보다 많으면 도망치지 않음
    public void RunAwayStatus(float currentHp, float maxHp)
    {
        if (_hasRunAway || (currentHp / maxHp) > _runAwayHpRatio)
            return;
       
        StopAllCoroutines();

        StartCoroutine(RunAway());
    }

    /// <summary>
    /// 몬스터(Coward_Melee)가 다시 활성화될 때 호출되는 함수 
    /// </summary>
    public void ResetRunAwayStatus()
    {
        _hasRunAway = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunAway()
    {
        _hasRunAway = true;     // 1번만 도망치기 위해서 중복 도망 방지용 변수를 True로 변경
        _isAttacking = true; // BaseEnemy의 Chase(), Stop() 로직을 멈춤

        // 도망가는 방향
        _agent.isStopped = false;
        _agent.updateRotation = true;
        _agent.ResetPath();

        Debug.Log(gameObject.name + ": 도망 시작!");

        float runAwayTimer = 0f;
        while (runAwayTimer < _runAwayDuration)
        {
            // 플레이어가 없으면 도망 중지
            if (_player == null) yield break;

            // 플레이어 반대 방향의 위치 계산
            Vector3 runAwayDirection = (transform.position - _player.position).normalized;
            Vector3 targetPosition = transform.position + runAwayDirection * 10f;

            // NavMesh 상의 유효한 위치인지 확인
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position); // 유효한 도망 위치로 이동
            }

            runAwayTimer += _pathUpdateTime; // 0.2초마다 경로 갱신
            yield return new WaitForSeconds(_pathUpdateTime);
        }

        Debug.Log(gameObject.name + ": 도망 끝! 추격 재개.");

        // 다시 추격 시작
        _canAttack = true;
        FinishAttack();

        StartCoroutine(EnemyLogic());
    }
}
