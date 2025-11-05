using UnityEngine;
using System.Collections;

public class Monster_Melee : BaseMonster
{
    [Header("근접 몬스터 돌진 관련 스탯")]
    public float dashSpeed = 10f; // 돌진 시 속도
    public float dashDuration = 0.5f; // 돌진 지속 시간
    public float dashAcceleration = 1000f; // 돌진 시 가속도

    private float _normalSpeed; // 기본 이동 속도
    private float _normalAcceleration; // 기본 가속도

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
}