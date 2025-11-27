using UnityEngine;
using UnityEngine.AI;

public class SubCharacterAI : MonoBehaviour
{
    public static SubCharacterAI Instance;

    private NavMeshAgent _navMeshAgent;
    private Vector3 _destination;
    private bool _hasDestination = false;
    private Animator _animator;
    public SubCharacterBehavior currentBehavior;

    private void Awake()
    {
        Instance = this;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 목적지가 설정되어있고, 목적지까지의 거리가 0.1f 이하 -> 목적지 도착
        if (_hasDestination && _navMeshAgent.remainingDistance <= 0.1f && _navMeshAgent.pathPending)
        {
            _hasDestination = false;
            _navMeshAgent.isStopped = true;
            MissionManager.Instance.OnSubCharacterDestinationReached();
        }
    }

    // 애니메이션 처리
    private void LateUpdate()
    {
        if (!_navMeshAgent.isStopped)
        {
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        _destination = destination;
        _hasDestination = true;
        _navMeshAgent.isStopped = false;
        _navMeshAgent.SetDestination(_destination);
    }
}
