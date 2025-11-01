using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    private float _damage = 0f;
    private PoolType _poolTag;
    private IAttackOwner _owner;

    // 이미 맞은 적들을 기록할 리스트
    private List<Collider> _hitEnemies;

    private PoolType _hitEffectTag = PoolType.BasicSlashHitEffect;

    private void Awake()
    {
        _hitEnemies = new List<Collider>();
    }

    // 오브젝트 풀에서 재사용될 때마다 맞은 적 리스트를 비웁니다.
    private void OnEnable()
    {
        _hitEnemies.Clear();
    }

    /// <summary>
    /// 이펙트 초기값 설정
    /// </summary>
    /// <param name="damage">데미지</param>
    /// <param name="poolTag">태그(오브젝트풀러)</param>
    /// <param name="lifeTime">이펙트 사라질 시간</param>
    public void InitialValues(IAttackOwner owner, float damage, PoolType poolTag, float lifeTime, float scale, PoolType hitEffectTag)
    {
        if (owner != null) _owner = owner;  // 차지공격에서는 null일 수 있음
        _damage = damage;
        _poolTag = poolTag;
        transform.localScale = Vector3.one * scale;
        _hitEffectTag = hitEffectTag;
        // Invoke 호출 전에 이전 Invoke 취소 (재사용 시 중복 호출 방지)
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return; // 중복 반납 방지

        CancelInvoke(nameof(ReturnToPool)); // 예약된 자동 반납 취소

        // 자체 파티클 시스템 정지 (있을 경우)
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 오브젝트 풀러에 반납
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 오브젝트 풀러 인스턴스가 없어 풀에 반납할 수 없습니다. 대신 비활성화합니다.");
            gameObject.SetActive(false); // 풀러 없으면 그냥 비활성화
        }
    }

    /// <summary>
    /// 충돌 지점에 히트 이펙트를 생성
    /// </summary>
    private void SpawnHitEffect(Collider other)
    {
        Vector3 collisionPoint = other.ClosestPoint(transform.position);
        Vector3 directionToOther = (other.transform.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(-directionToOther);
        Vector3 pos = collisionPoint + directionToOther;

        GameObject hitEffet = ObjectPooler.Instance.SpawnFromPool(_hitEffectTag, pos, rot);
        StartCoroutine(ReturnHitEffect(hitEffet));
    }

    IEnumerator ReturnHitEffect(GameObject hitEffect)
    {
        yield return new WaitForSeconds(0.2f);
        if (hitEffect != null)
        {
            ObjectPooler.Instance.ReturnToPool(_hitEffectTag, hitEffect);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            // 적 캐릭터에 닿았을 때
            if (other.CompareTag("Enemy"))
            {
                // --- 추가된 부분: 이미 맞은 적인지 확인 ---
                if (!_hitEnemies.Contains(other))
                {
                    other.GetComponent<MonsterStatus>()?.TakeDamage(_damage);

                    // --- 추가된 부분: 맞은 적 리스트에 추가 ---
                    _hitEnemies.Add(other);
                    SpawnHitEffect(other);

                    // 히트시 히트했다고 PlayerAttack에 알림
                    _owner?.NotifyHit();
                }
            }
        }
    }
}