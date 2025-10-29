using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;

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
        {

        }
            
        else
        {

        }
            
    }

    
}