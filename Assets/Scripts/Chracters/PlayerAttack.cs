using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("# 무기 데이터 리스트")]
    public List<WeaponData> weaponDataList; // 게임 내 존재하는 모든 무기 데이터 리스트

    [Header("# 현재 무기 데이터")]
    public WeaponData currentWeapon; // 현재 장착된 무기 데이터

    private float _lastAttackTime; // 마지막 공격 시점
    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (currentWeapon == null) return;

        if (Time.time >= _lastAttackTime + currentWeapon.attackDelay)
        {
            if (Input.GetMouseButton(0)) // 마우스 좌클릭을 누르고 있을 때 공격
            {
                Attack();
                _lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        // TODO: 애니메이션 받으면 호출하기
        if (currentWeapon.projectilePrefab != null)
        {
            GameObject effect = ObjectPooler.Instance.SpawnFromPool(
                currentWeapon.weaponTag,
                transform.position + currentWeapon.attackPositionOffset,
                _playerController.Rotation
            );
            effect.GetComponent<AttackEffect>().InitialValues(currentWeapon.damage, currentWeapon.weaponTag, currentWeapon.lifeTime);
        }
    }
}
