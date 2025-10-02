using UnityEngine;

public class DummyDamage : MonoBehaviour
{
    int hitTimes = 0;

    void Update()
    {
        if (hitTimes >= 5)
        {
            Debug.Log($"{gameObject.name}°¡ Á×¾ú½À´Ï´Ù!");
            ObjectPooler.Instance.ReturnToPool("Dummy", gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerTakeDamage(10f, gameObject);
            }
            else if (other.CompareTag("PlayerAttack"))
            {
                hitTimes++;
                Debug.Log($"³¢¿¡¿¡¿¡¿¢! {gameObject.name}°¡ ¸ÂÀºÈ½¼ö: {hitTimes}");
            }
        }
    }
}
