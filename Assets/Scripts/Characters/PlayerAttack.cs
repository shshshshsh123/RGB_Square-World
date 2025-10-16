using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    // 내부에서 사용할 현재 무기 데이터
    [System.Serializable]
    public class EquippedWeapon
    {
        public WeaponData weaponData;
        public int currentLevel = 1;
        public float lastAttackTime;

        public EquippedWeapon(WeaponData data)
        {
            weaponData = data;
            currentLevel = 1;
            lastAttackTime = 0f;
        }

        public LevelData GetCurrentLevelData()
        {
            // 레벨 1부터 시작하게 할거임 (보기 편하게)
            int index = Mathf.Clamp(currentLevel - 1, 0, weaponData.levelDataList.Count - 1);
            return weaponData.levelDataList[index];
        }
    }
    [Header("# 무기 관리")]
    public List<WeaponData> weaponDatas; // 게임 내 존재하는 모든 무기 데이터
    public List<EquippedWeapon> equippedWeapons;

    private float _lastAttackTime; // 마지막 공격 시점
    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        equippedWeapons = new List<EquippedWeapon>();
    }

    void Start()
    {

    }

    void Update()
    {
        // 기본공격
        HandleAutoAttacks();

        // 테스트용!!!!!!!!
        if (Input.GetKeyDown(KeyCode.Space)) AddOrUpgradeWeapon(weaponDatas[0]);
    }

    void HandleAutoAttacks()
    {
        if (!Input.GetMouseButton(0)) return;    // 좌클릭 중에만 발동

        foreach (EquippedWeapon weapon in equippedWeapons)
        {
            LevelData levelData = weapon.GetCurrentLevelData();
            // 공격 딜레이 체크
            if (Time.time >= weapon.lastAttackTime + levelData.attackDelay)
            {
                Attack(weapon);
                weapon.lastAttackTime = Time.time;
            }
        }
    }

    void Attack(EquippedWeapon weapon)
    {
        LevelData levelData = weapon.GetCurrentLevelData();

        if (weapon.weaponData.projectilePrefab == null)
        {
            Debug.LogWarning($"[Player Attack] 이잉? 이펙트가 없네요????? {weapon.weaponData.weaponTag}");
            return;
        }

        // 발사체 수(projectileCount)만큼 반복하여 공격
        for (int i = 0; i < levelData.projectileCount; i++)
        {
            GameObject effect = ObjectPooler.Instance.SpawnFromPool(
                weapon.weaponData.weaponTag,
                transform.position + weapon.weaponData.attackPositionOffset,
                _playerController.Rotation
            );

            // 이펙트/투사체에 데미지, 크기 등 레벨에 맞는 데이터 전달
            AttackEffect attackEffect = effect.GetComponent<AttackEffect>();
            if (attackEffect != null)
            {
                attackEffect.InitialValues(levelData.damage, weapon.weaponData.weaponTag, weapon.weaponData.lifeTime, levelData.scale);
            }

            // TODO: 여러 발사체를 쏠 때 방향을 다르게 하는 로직 추가 (예: 부채꼴, 전방위 등)
        }
    }

    public void AddOrUpgradeWeapon(WeaponData weaponData)
    {
        EquippedWeapon existingWeapon = equippedWeapons.FirstOrDefault(w => w.weaponData == weaponData); // 이미 장착된 무기인지 확인

        if (existingWeapon != null)
        {
            // 현재 레벨이 최대 레벨(levelDataList의 개수)보다 작은지 확인
            if (existingWeapon.currentLevel < weaponData.levelDataList.Count)
            {
                // 이미 있으면 레벨업
                existingWeapon.currentLevel++;
                Debug.Log($"{weaponData.name} 레벨 업! -> Lv.{existingWeapon.currentLevel}");
            }
            else
            {
                // 최대 레벨에 도달했을 경우
                Debug.Log($"{weaponData.name}은(는) 이미 최대 레벨(Lv.{existingWeapon.currentLevel})입니다!");
            }
        }
        else
        {
            // 없으면 새로 추가
            equippedWeapons.Add(new EquippedWeapon(weaponData));
            Debug.Log($"{weaponData.name} 새로 획득!");
        }
    }
}