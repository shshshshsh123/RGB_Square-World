using UnityEngine;

[RequireComponent(typeof(MonsterStatus))]
public abstract class BaseMonster : MonoBehaviour
{
    protected Transform player;
    protected MonsterStatus status;
    protected Rigidbody rb;

    [Header("이동 관련 설정")]
    public float attackRange = 1.5f;
    public float separationRadius = 1.0f;
    public float separationStrength = 3.0f;
    public LayerMask monsterLayer;

    protected float moveSpeed;
    protected float attackDamage;
    protected float attackCooldown = 1.0f;
    protected float lastAttackTime = 0f;

    // Update()가 물리 프레임(FixedUpdate)에 이동 여부를 알려주기 위한 플래그
    private bool isPlayerInRange;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        status = GetComponent<MonsterStatus>();
        rb = GetComponent<Rigidbody>();

        // [★땅 꺼짐 현상 해결★]
        // Rigidbody의 Y축 이동과 회전을 고정하고 중력을 끕니다.
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = false;
        }

        moveSpeed = status.monsterSpeed;
        attackDamage = status.monsterDamage;
    }

    protected virtual void Update()
    {
        if (player == null) return;

        // --- 1. 계산 및 방향 전환 ---
        Vector3 playerPosXZ = new Vector3(player.position.x, 0, player.position.z);
        Vector3 monsterPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float currentDistance = Vector3.Distance(playerPosXZ, monsterPosXZ);

        // 플레이어 바라보기
        Vector3 lookTarget = player.position;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        // --- 2. 상태 결정 ---
        if (currentDistance > attackRange)
        {
            isPlayerInRange = false; // 사거리 밖 (FixedUpdate가 이동시킬 것)
        }
        else
        {
            isPlayerInRange = true; // 사거리 안 (FixedUpdate가 멈추거나, MeleeMonster처럼 맴돌게 할 것)
            TryAttack(); // 공격 시도
        }
    }

    // [★떨림 현상 해결★]
    // 모든 물리 로직(이동/정지)은 FixedUpdate에서 처리합니다.
    protected virtual void FixedUpdate()
    {
        if (player == null || rb == null) return;

        if (isPlayerInRange)
        {
            // 사거리 안: 정지 로직 실행 (MeleeMonster는 이 부분을 재정의)
            StopMovement();
        }
        else
        {
            // 사거리 밖: 이동 로직 실행
            MoveToPlayer();
        }
    }

    protected virtual void MoveToPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0;
        dirToPlayer.Normalize();

        Vector3 separationDir = GetSeparationDirection();

        Vector3 finalDir = (dirToPlayer + separationDir).normalized;
        finalDir.y = 0;

        // 물리 프레임에 맞춰 이동
        rb.MovePosition(rb.position + finalDir * moveSpeed * Time.fixedDeltaTime);
    }

    protected virtual void StopMovement()
    {
        // RangedMonster 등은 여기서 멈춥니다.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private Vector3 GetSeparationDirection()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, monsterLayer);
        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector3 away = transform.position - neighbor.transform.position;
            away.y = 0;
            float dist = away.magnitude;

            if (dist > 0)
            {
                separation += away.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            separation /= count;
            return separation.normalized * separationStrength;
        }

        return Vector3.zero;
    }

    protected abstract void TryAttack();
}