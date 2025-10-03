using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    [Header("# ���� ������ ����Ʈ")]
    public List<WeaponData> weaponDataList; // ���� �� �����ϴ� ��� ���� ������ ����Ʈ

    [Header("# ���� ���� ������")]
    public WeaponData currentWeapon; // ���� ������ ���� ������

    private float _lastAttackTime; // ������ ���� ����
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
            if (Input.GetMouseButton(0)) // ���콺 ��Ŭ���� ������ ���� �� ����
            {
                Attack();
                _lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        // TODO: �ִϸ��̼� ������ ȣ���ϱ�
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
