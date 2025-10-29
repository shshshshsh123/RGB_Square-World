using UnityEngine;
using System.Collections;

public class PlayerRangeChargeAttack : PlayerChargeAttackBase
{
    [Header("# 원거리 차지공격 설정")]
    public GameObject projectilePrefab; // 발사체 프리팹
    public float knockbackForce = 10f; // 넉백 힘

    protected override IEnumerator PerformChargeAttack()
    {
        yield return null;
    }
}
