using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    public float lifetime = 3f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.PlayerTakeDamage(damage, gameObject);
            Destroy(gameObject);
        }
    }
}
