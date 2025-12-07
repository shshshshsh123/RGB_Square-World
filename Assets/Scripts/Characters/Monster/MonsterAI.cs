using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.radius = AIDirector.Instance.CurrentMonsterSpacingRadius;
        }
    }
}
