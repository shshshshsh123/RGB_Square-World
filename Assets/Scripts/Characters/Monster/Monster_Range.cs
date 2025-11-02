using UnityEngine;
using System.Collections;

public class Monster_Range : BaseMonster
{
    [Header("원거리 몬스터")]
    [SerializeField] private Transform _firePos;
    [SerializeField] private PoolType projectilePoolType;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Attack()
    {
        StartCoroutine(Fire());
    }

    private IEnumerator Fire()
    {
        
        if (_firePos == null)
        {
            Debug.LogError(gameObject.name + ": 발사 위치가 설정되지 않았습니다!");
        }
        else
        {
            GameObject projectileObject = ObjectPooler.Instance.SpawnFromPool(projectilePoolType, _firePos.position, _firePos.rotation); // 발사
        }
        // 쿨타임 대기
        yield return new WaitForSeconds(_attackCooldown);

        // 공격 완료
        FinishAttack(); // BaseEnemy의 _isAttacking = false로 설정
    }
}
