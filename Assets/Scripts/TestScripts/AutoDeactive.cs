using UnityEngine;

public class AutoDeactive : MonoBehaviour
{
    [Tooltip("�ڵ����� ��Ȱ��ȭ�� �������� �ð� (��)")]
    public float lifeTime = 1.0f;
    [Tooltip("������Ʈ Ǯ���� ��ϵ� �� ������Ʈ�� �±�")]
    public string poolTag;

    private void OnEnable()
    {
        // ������Ʈ�� Ȱ��ȭ�� ������ lifeTime �Ŀ� ReturnToPool �޼��带 ȣ���ϵ��� �����մϴ�.
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        // ������Ʈ Ǯ���� �����ϰ�, �±װ� �����Ǿ� ���� ���� Ǯ�� �ݳ��մϴ�.
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}�� �±װ� �̻��ؿ�!!!");
        }
    }
}
