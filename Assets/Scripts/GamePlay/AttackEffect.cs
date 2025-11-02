using UnityEngine;
using System.Collections.Generic;

public class AttackEffect : MonoBehaviour
{
    private float _damage = 0f;
    private PoolType _poolTag;

    // 이미 맞은 적들을 기록할 리스트
    private List<Collider> _hitEnemies;

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
    public void InitialValues(float damage, PoolType poolTag, float lifeTime, float scale)
    {
        _damage = damage;
        _poolTag = poolTag;
        transform.localScale = Vector3.one * scale;
        // Invoke 호출 전에 이전 Invoke 취소 (재사용 시 중복 호출 방지)
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        // 오브젝트 풀러가 존재할 때만 풀에 반납합니다.
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 태그가 이상해요!!!");
            // 풀러가 없을 경우 비활성화 (선택적)
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            // 적 캐릭터에 닿았을 때
            if (other.CompareTag("Monster"))
            {
                // --- 추가된 부분: 이미 맞은 적인지 확인 ---
                if (!_hitEnemies.Contains(other))
                {
                    Debug.Log("AttackEffect가 Monster에 닿음");
                    other.GetComponent<MonsterStatus>()?.TakeDamage(_damage);

                    // --- 추가된 부분: 맞은 적 리스트에 추가 ---
                    _hitEnemies.Add(other);
                }
            }
        }
    }
}