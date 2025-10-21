using UnityEngine;

public class RangedMonster : BaseMonster
{
    public GameObject projectilePrefab;
    public Transform firePoint;

    protected override void Start()
    {
        base.Start();
        attackRange = 4f;
        attackCooldown = 1.2f;
    }

    protected override void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (projectilePrefab && firePoint)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector3 dir = (player.position - firePoint.position).normalized;

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = dir * 10f;

            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
                p.SetDamage(attackDamage);
        }
    }
}
