using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MonsterKnockBack : MonoBehaviour
{
    private Rigidbody _rb;
    private NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 몬스터에게 넉백을 적용합니다.
    /// </summary>
    /// <param name="direction">넉백 방향 (정규화된 벡터).</param>
    /// <param name="force">넉백 힘.</param>
    /// <param name="duration">넉백이 지속되는 시간.</param>
    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        if (_rb == null || !gameObject.activeSelf) return;  // 막타로 해제되면 오류가 나더라구요

        StopCoroutine(nameof(KnockbackCoroutine)); // 기존 넉백 코루틴이 있다면 중지
        StartCoroutine(KnockbackCoroutine(direction, force, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, float force, float duration)
    {
        if (_navMeshAgent != null && _navMeshAgent.enabled)
        {
            _navMeshAgent.enabled = false; // 넉백 중 NavMeshAgent 비활성화
        }

        float timer = 0f;
        while (timer < duration)
        {
            // 시간에 따라 넉백 힘을 점차 줄여 자연스럽게 멈추도록 보간
            float progress = timer / duration;
            float currentForce = Mathf.Lerp(force, 0, progress);
            _rb.linearVelocity = direction * currentForce;

            timer += Time.deltaTime;
            yield return null;
        }

        _rb.linearVelocity = Vector3.zero; // 넉백 종료 후 속도 초기화

        if (_navMeshAgent != null)
        {
            _navMeshAgent.enabled = true; // 넉백 후 NavMeshAgent 재활성화
        }
    }
}
