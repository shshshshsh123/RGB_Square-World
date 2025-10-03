using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    private float _damage = 0f;
    private string _poolTag = "";

    /// <summary>
    /// 이펙트 초기값 설정
    /// </summary>
    /// <param name="damage">데미지</param>
    /// <param name="poolTag">태그(오브젝트풀러)</param>
    /// <param name="lifeTime">이펙트 사라질 시간</param>
    public void InitialValues(float damage, string poolTag, float lifeTime)
    {
        _damage = damage;
        _poolTag = poolTag;
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        // 오브젝트 풀러가 존재하고, 태그가 설정되어 있을 때만 풀에 반납합니다.
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(_poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 태그가 이상해요!!!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            // 적 캐릭터에 닿았을 때
            if (other.CompareTag("Enemy"))
            {
                other.GetComponent<DummyDamage>()?.TakeDamage(_damage);
            }
        }
    }
}
