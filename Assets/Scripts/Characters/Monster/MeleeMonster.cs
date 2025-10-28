using UnityEngine;
using System.Collections; // 코루틴 사용
/*
public class MeleeMonster : BaseMonster
{
    [Header("근접 몬스터")]
    public float dashSpeed = 15f; // 박치기 속도
    public float dashDuration = 0.5f; // 박치기 지속 시간

    private bool isDashing = false; // 현재 박치기 중인지 확인

    void Start()
    {
        // BaseMonster의 Start() 실행
        base.Start();
        // 근접 몬스터의 쿨타임 설정 (예: 3초)
        attackCooldown = 3f;
    }

    /// <summary>
    /// 근접 몬스터의 공격 행동 (재정의)
    /// </summary>
    protected override void Attack()
    {
        // 이미 박치기 중이거나 쿨타임이 안됐으면 아무것도 안 함
        if (isDashing || Time.time < lastAttackTime + attackCooldown)
        {
            // 쿨타임 중일 때는 플레이어를 향해 천천히 이동할 수도 있음 (선택)
            // base.PerformMovement(); 

            // 혹은 그냥 멈춰서 기다릴 수도 있음
            Stop();
            Vector3 directionToPlayer = (_player.position - transform.position).normalized;
            Rotate(directionToPlayer);
            return;
        }

        // 쿨타임 완료: 박치기 시작
        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
    {
        isDashing = true;
        lastAttackTime = Time.time;

        // 1. 플레이어 방향으로 고정
        Vector3 dashDirection = (_player.position - transform.position).normalized;

        // 2. 장애물 회피를 잠시 무시하고 해당 방향으로 고속 이동
        // (y속도는 현재 속도 유지)
        _rigidBody.linearVelocity = new Vector3(dashDirection.x * dashSpeed, _rigidBody.linearVelocity.y, dashDirection.z * dashSpeed);

        // 3. 정해진 시간(dashDuration) 동안 대기
        yield return new WaitForSeconds(dashDuration);

        // 4. 박치기 종료 및 상태 초기화
        isDashing = false;

        // 박치기 후 속도를 0으로 초기화 (선택 사항)
        // StopMovement(); 
    }

    // 박치기 중에는 일반 이동/회피 로직이 돌지 않도록 FixedUpdate를 재정의
    void FixedUpdate()
    {
        if (isDashing)
        {
            // 박치기 중일 때는 BaseMonster의 FixedUpdate 로직(회피, 거리체크)을
            // 실행하지 않고 그냥 리턴합니다. (속도는 코루틴에서 제어 중)
            return;
        }

        // 박치기 중이 아닐 때만 부모의 일반 로직(거리 체크, 이동/공격)을 실행
        base.FixedUpdate();
    }
}
*/