using UnityEngine;
using System.Collections;

public class Enemy_Melee : BaseEnemy
{
    [Header("Melee Stats")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashAcceleration = 1000f;
    private float _normalSpeed;
    private float _normalAcceleration;

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

        _agent.updateRotation = false;

        while (_agent.pathPending)
        {
            Vector3 lookDir = (_player.position - transform.position).normalized;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);
            yield return null;
        }

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