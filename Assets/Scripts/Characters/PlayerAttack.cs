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

    [Header("# 공격 관리")]
    public float spreadAngle = 15f; // 투사체 퍼지는 각도

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
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddOrUpgradeWeapon(weaponDatas[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddOrUpgradeWeapon(weaponDatas[1]);
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

        int projectileCount = levelData.projectileCount;

        // 공격 방향 결정 (플레이어의 목표 회전 값)
        Quaternion baseRotation = _playerController.Rotation;

        // 공격 시작 위치 결정
        Vector3 basePosition = transform.position; // 플레이어 위치 기준
        Vector3 rotatedOffset = baseRotation * weapon.weaponData.attackPositionOffset; // 오프셋 적용
        Vector3 spawnPosition = basePosition + rotatedOffset;

        // 발사체가 1개면 정면으로 발사
        if (projectileCount <= 1)
        {
            SpawnProjectile(weapon, levelData, spawnPosition, baseRotation);
        }
        // 발사체가 2개 이상이면 부채꼴로 발사
        else
        {
            // 발사체 사이의 각도 계산
            float angleStep = spreadAngle / (projectileCount - 1);
            // 시작 각도 계산 (부채꼴 중앙 정렬)
            float startAngle = -spreadAngle / 2f;

            for (int i = 0; i < projectileCount; i++)
            {
                // 현재 발사체의 각도 계산
                float currentAngle = startAngle + (i * angleStep);
                // 기본 회전값에 현재 각도를 더하여 최종 발사 방향 계산 (Y축 기준 회전)
                Quaternion projectileRotation = baseRotation * Quaternion.Euler(0, currentAngle, 0);

                SpawnProjectile(weapon, levelData, spawnPosition, projectileRotation);
            }
        }
    }

    /// <summary>
    /// 실제 발사체를 스폰하고 초기화하는 함수
    /// </summary>
    void SpawnProjectile(PlayerAttack.EquippedWeapon weapon, LevelData levelData, Vector3 position, Quaternion rotation)
    {
        GameObject instance = ObjectPooler.Instance.SpawnFromPool(
            weapon.weaponData.weaponTag,
            position,
            rotation
        );

        if (instance == null) return;

        // 생성된 인스턴스 초기화 (무기 타입에 따라 다른 컴포넌트 접근)
        if (weapon.weaponData.weaponType == WeaponType.Ranged)
        {
            RangedProjectile projectile = instance.GetComponent<RangedProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(weapon.weaponData, levelData, weapon.weaponData.weaponTag); // 이 부분이 새 RangedProjectile을 초기화합니다.
                instance.transform.localScale = Vector3.one * levelData.scale;
            }
            else
            {
                Debug.LogError($"[Player Attack] 원거리 프리팹에 RangedProjectile 스크립트가 없습니다: {weapon.weaponData.weaponTag} - {instance.name}");
            }
        }
        else if (weapon.weaponData.weaponType == WeaponType.Melee)
        {
            AttackEffect attackEffect = instance.GetComponent<AttackEffect>();
            if (attackEffect != null)
            {
                attackEffect.InitialValues(levelData.damage, weapon.weaponData.weaponTag, weapon.weaponData.lifeTime, levelData.scale);
            }
            else
            {
                Debug.LogError($"[Player Attack] 근접 프리팹에 AttackEffect 스크립트가 없습니다: {weapon.weaponData.weaponTag} - {instance.name}");
            }
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