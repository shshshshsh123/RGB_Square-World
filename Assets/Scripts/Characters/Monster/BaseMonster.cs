using UnityEngine;
using UnityEngine.AI;

public class BaseMonster : MonoBehaviour
{
    protected Transform _player;
    protected NavMeshAgent _agent;

    protected void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            _player = playerObject.transform;
        }
    }

    void FixedUpdate()
    {
        if (_player != null && _agent.isActiveAndEnabled)
        {
            _agent.SetDestination(_player.position);
        }
    }
}