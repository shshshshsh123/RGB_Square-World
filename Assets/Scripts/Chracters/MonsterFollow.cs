using UnityEngine;
using UnityEngine.AI;

public class MonsterFollow : MonoBehaviour
{
    [SerializeField] private Transform _player;
    private NavMeshAgent _agent;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (_player != null)
        {
            _agent.SetDestination(_player.position); // 플레이어 위치로 이동
        }
    }

}
