using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float avoidanceDistance = 2f;
    public LayerMask obstacleLayer;

    [Header("공격")]
    public float attackRange = 3f;
    protected float _attackCoolDown = 2f;
    protected float _lastAttackTime = 0f;

    protected Transform _player;
    protected Rigidbody _rigidBody;

    protected void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            _player = playerObject.transform;
    }

    void FixedUpdate()
    {
        if (_player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        // 사거리 안에 있으면 공격, 사거리 밖에 있으면 이동
        if (distanceToPlayer < attackRange)
            Attack();
        else
            Movement();
    }

    /// <summary>
    /// 이동 로직
    /// </summary>
    protected virtual void Movement()
    {
        if (_player == null)
            return;

        // 방향
        Vector3 targetDirection = (_player.position - transform.position).normalized;

        // 장애물 회피
        Vector3 moveDirection = Avoid(targetDirection);

        // 이동
        Move(moveDirection);

        // 회전
        Rotate(moveDirection);
    }

    protected virtual void Attack()
    {

    }

    /// <summary>
    /// 몬스터 앞에 장애물이 있는지 확인하고, 장애물이 있으면 회피
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    protected Vector3 Avoid(Vector3 direction)
    {
        RaycastHit hit;

        // 몬스터가 가야 할 방향에 장애물이 있는지 Ray로 확인
        if (Physics.Raycast(transform.position, direction, out hit, avoidanceDistance, obstacleLayer))
        {
            // 장애물이 감지되면 좌우 방향을 탐색
            Vector3 rightDir = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 leftDir = -rightDir;

            if (!Physics.Raycast(transform.position, rightDir, avoidanceDistance, obstacleLayer))
            {
                return rightDir; // 오른쪽으로 회피
            }
            // 왼쪽이 비었는지 확인
            else if (!Physics.Raycast(transform.position, leftDir, avoidanceDistance, obstacleLayer))
            {
                return leftDir; // 왼쪽으로 회피
            }
            else return rightDir;
        }
        return direction;
    }

    protected void Move(Vector3 direction)
    {
        // y축 속도는 제외
        _rigidBody.linearVelocity = new Vector3(direction.x * moveSpeed, 0, direction.z * moveSpeed);
    }

    protected void Rotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _rigidBody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    protected void Stop()
    {
        _rigidBody.linearVelocity = new Vector3(0, 0, 0);
    }
}