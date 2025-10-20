using UnityEngine;

public class MonsterMove : MonoBehaviour
{
    private MonsterStatus _monsterStatus;
    private Rigidbody _rigidBody;
    private Transform _player;

    [Header("이동 설정")]
    [Tooltip("주변 몬스터와 유지할 거리")]
    public float desiredDistance = 2.5f;
    [Tooltip("몬스터끼리 서로 밀어내는 강도")]
    public float repulsionForce = 2f;

    void Awake()
    {
        _monsterStatus = GetComponent<MonsterStatus>();
        _rigidBody = GetComponent<Rigidbody>();

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (_player == null) return;

        // 플레이어 방향 계산
        Vector3 toPlayer = (_player.position - transform.position).normalized;

        // 주변 몬스터들과 거리 유지
        Vector3 separation = Vector3.zero;
        Collider[] neighbors = Physics.OverlapSphere(transform.position, desiredDistance, LayerMask.GetMask("Enemy"));

        foreach (var n in neighbors)
        {
            if (n.gameObject == gameObject) continue;
            Vector3 away = transform.position - n.transform.position;
            float dist = away.magnitude;
            if (dist > 0)
                separation += away.normalized / dist; // 가까울수록 강하게 밈
        }

        // 최종 방향 계산
        Vector3 moveDir = (toPlayer + separation * repulsionForce).normalized;

        Vector3 targetVelocity = moveDir * _monsterStatus.monsterSpeed;
        _rigidBody.linearVelocity = Vector3.Lerp(_rigidBody.linearVelocity, targetVelocity, Time.fixedDeltaTime * 5f);

        // 직(Y) 성분을 제거하여 수평 방향 벡터만 얻음
        Vector3 horizontalVelocity = _rigidBody.linearVelocity;
        horizontalVelocity.y = 0;

        // 바라보는 방향으로 회전
        if (_rigidBody.linearVelocity.sqrMagnitude > 0.1f)
        {
            // 회전 계산
            Quaternion lookRot = Quaternion.LookRotation(horizontalVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.fixedDeltaTime * 3f);
        }
    }
}
