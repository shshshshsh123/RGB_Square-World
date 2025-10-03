using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    private float _damage = 0f;
    private string _poolTag = "";

    /// <summary>
    /// ����Ʈ �ʱⰪ ����
    /// </summary>
    /// <param name="damage">������</param>
    /// <param name="poolTag">�±�(������ƮǮ��)</param>
    /// <param name="lifeTime">����Ʈ ����� �ð�</param>
    public void InitialValues(float damage, string poolTag, float lifeTime)
    {
        _damage = damage;
        _poolTag = poolTag;
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        // ������Ʈ Ǯ���� �����ϰ�, �±װ� �����Ǿ� ���� ���� Ǯ�� �ݳ��մϴ�.
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(_poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}�� �±װ� �̻��ؿ�!!!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            // �� ĳ���Ϳ� ����� ��
            if (other.CompareTag("Enemy"))
            {
                other.GetComponent<DummyDamage>()?.TakeDamage(_damage);
            }
        }
    }
}
