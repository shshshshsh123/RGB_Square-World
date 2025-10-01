using UnityEngine;

public class DummyDamage : MonoBehaviour
{
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
