using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MonsterStatus))]
public class MeleeMonster : BaseMonster
{
    [Header("박치기 설정")]
    public float dashSpeedMultiplier = 3f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 1.5f;

    private bool isDashing = false;
    private bool hasHitPlayer = false;
    private bool isReturning = false; // 박치기 후 복귀 중인지 확인

    public bool IsDashing => isDashing;

    // [CS0115 오류 수정 완료된 버전]
    protected void OnEnable()
    {
        isDashing = false;
        hasHitPlayer = false;
        lastAttackTime = 0f;
        isReturning = false; // 풀에서 다시 나올 때 초기화
    }

    protected override void Start()
    {
        base.Start();
        attackRange = 1.5f;
        attackCooldown = dashCooldown;
    }

    protected override void TryAttack()
    {
        // 돌진 중이거나 '복귀 중'일 때는 공격 시도 안 함
        if (Time.time - lastAttackTime < attackCooldown || isDashing || isReturning)
            return;

        lastAttackTime = Time.time;
        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
    {
        isDashing = true;
        hasHitPlayer = false;

        // 돌진 시작 위치 저장
        Vector3 startPosition = rb.position;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        Vector3 lookTarget = player.position;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        float dashSpeed = moveSpeed * dashSpeedMultiplier;
        float elapsed = 0f;
        float durationInSeconds = dashDuration;

        // --- 1. 돌진 ---
        // (FixedUpdate 기준으로 수정됨)
        while (elapsed < durationInSeconds && isDashing)
        {
            if (rb != null)
            {
                rb.MovePosition(rb.position + dir * dashSpeed * Time.fixedDeltaTime);
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isDashing = false; // 돌진 종료

        // --- 2. 원래 위치로 복귀 ---
        isReturning = true; // 복귀 시작
        float returnSpeed = moveSpeed;

        while (Vector3.Distance(rb.position, startPosition) > 0.1f)
        {
            Vector3 returnDir = (startPosition - rb.position).normalized;
            rb.MovePosition(rb.position + returnDir * returnSpeed * Time.fixedDeltaTime);

            Vector3 returnLookTarget = startPosition;
            returnLookTarget.y = transform.position.y;
            transform.LookAt(returnLookTarget);

            yield return new WaitForFixedUpdate();
        }

        isReturning = false; // 복귀 종료
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDashing) return;

        if (!hasHitPlayer && other.CompareTag("Player"))
        {
            hasHitPlayer = true;
            GameManager.Instance.PlayerTakeDamage(attackDamage, gameObject);
            isDashing = false;
        }
    }

    protected override void MoveToPlayer()
    {
        // 돌진 중이거나 '복귀 중'일 때는 이동 로직 실행 안 함
        if (isDashing || isReturning) return;
        base.MoveToPlayer();
    }

    // [★이상한 행동 해결★]
    protected override void StopMovement()
    {
        // 돌진 중이거나 '복귀 중'일 때는 멈추지 않음 (코루틴이 제어)
        if (isDashing || isReturning)
        {
            return;
        }

        // [수정] 맴도는 로직(MoveToPlayer) 대신,
        // 부모의 정지 로직(rb.linearVelocity = 0)을 호출하여
        // 쿨타임 동안 그 자리에 멈춰있게 함.
        base.StopMovement();
    }
}