using UnityEngine;

public class DummyDamage : MonoBehaviour
{
    int hitTimes = 0;

    void Update()
    {
        if (hitTimes >= 5)
        {
            Debug.Log($"{gameObject.name}�� �׾����ϴ�!");
            ObjectPooler.Instance.ReturnToPool("Dummy", gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        hitTimes++;
        Debug.Log($"����������! {gameObject.name}�� {damage}��ŭ�� �������� �Ծ����ϴ�! ����Ƚ��: {hitTimes}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerTakeDamage(10f, gameObject);
            }
        }
    }
}
