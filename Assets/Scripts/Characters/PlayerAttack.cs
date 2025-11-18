using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

public class PlayerAttack : MonoBehaviour, IAttackOwner
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
    public float hitStopCooldown = 0.1f; // 히트스톱 쿨타임 (최소 시간)
    public float hitStopDuration = 0.1f; // 히트스톱 지속 시간
    public LayerMask groundLayer; // 마법 공격 시 지면 레이어

    private float _lastAttackTime; // 마지막 공격 시점
    private float _lastHitStopTime; // 마지막 히트스톱 시점
    private bool _canAttackHitStop = true; // 히트스톱 가능 여부
    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        equippedWeapons = new List<EquippedWeapon>();
    }

    void Start()
    {
        // 내부 변수 초기화
        _lastAttackTime = Time.time;
        _canAttackHitStop = true;
    }

    void Update()
    {
        // 기본공격
        HandleAutoAttacks();

        // 테스트용!!!!!!!!
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddOrUpgradeWeapon(weaponDatas[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2)) AddOrUpgradeWeapon(weaponDatas[1]);
        if (Input.GetKeyDown(KeyCode.Alpha3)) AddOrUpgradeWeapon(weaponDatas[2]);
    }

    void HandleAutoAttacks()
    {
        if (Input.GetMouseButtonUp(0)) _canAttackHitStop = true;    // 좌클릭 뗄 때 히트스톱 가능하도록 리셋
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

        // 1. 공격 기준 회전 결정 (플레이어의 목표 회전 값)
        Quaternion playerBaseRotation = _playerController.Rotation;

        // 2. WeaponData에 정의된 회전 오프셋을 Quaternion으로 변환
        Quaternion weaponRotationOffset = Quaternion.Euler(weapon.weaponData.attackRotationOffset);

        // 3. 플레이어의 기본 회전에 무기 자체의 회전 오프셋을 먼저 적용하여 최종 기준 회전을 만듭니다.
        // 이는 이펙트가 플레이어가 바라보는 방향 + 무기 자체의 기울어진 방향으로 나가게 합니다.
        Quaternion combinedBaseRotation = playerBaseRotation * weaponRotationOffset;


        // 4. 공격 시작 위치 결정
        Vector3 rotatedPositionOffset = playerBaseRotation * weapon.weaponData.attackPositionOffset;
        Vector3 spawnPosition = transform.position + rotatedPositionOffset;

        // 발사체가 1개면 정면으로 발사
        if (projectileCount <= 1)
        {
            SpawnProjectile(weapon, levelData, spawnPosition, combinedBaseRotation);
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

                // 콤바인된 기준 회전에 부채꼴 각도를 더하여 최종 발사 방향을 계산
                // Quaternion.Euler(0, currentAngle, 0)은 Y축 기준 회전이므로, combinedBaseRotation의 로컬 회전으로 적용
                Quaternion projectileRotation = combinedBaseRotation * Quaternion.Euler(0, currentAngle, 0);

                SpawnProjectile(weapon, levelData, spawnPosition, projectileRotation);
            }
        }
    }

    /// <summary>
    /// 실제 발사체를 스폰하고 초기화하는 함수
    /// </summary>
    void SpawnProjectile(PlayerAttack.EquippedWeapon weapon, LevelData levelData, Vector3 position, Quaternion rotation)
    {
        // 생성된 인스턴스 초기화 (무기 타입에 따라 다른 컴포넌트 접근)
        if (weapon.weaponData.weaponType == WeaponType.Ranged)
        {
            GameObject instance = ObjectPooler.Instance.SpawnFromPool(
                weapon.weaponData.weaponTag,
                position,
                rotation
            );

            RangedProjectile projectile = instance.GetComponent<RangedProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    this,
                    levelData.damage,
                    levelData.projectileSpeed,
                    levelData.penetrationCount,
                    weapon.weaponData.weaponTag,
                    weapon.weaponData.lifeTime,
                    PoolType.ArrowHitEffect
                );
                instance.transform.localScale = Vector3.one * levelData.scale;
            }
            else
            {
                Debug.LogError($"[Player Attack] 원거리 프리팹에 RangedProjectile 스크립트가 없습니다: {weapon.weaponData.weaponTag} - {instance.name}");
            }
        }
        else if (weapon.weaponData.weaponType == WeaponType.Melee)
        {
            GameObject instance = ObjectPooler.Instance.SpawnFromPool(
                weapon.weaponData.weaponTag,
                position,
                rotation
            );

            AttackEffect attackEffect = instance.GetComponent<AttackEffect>();
            if (attackEffect != null)
            {
                attackEffect.InitialValues(
                    this,
                    levelData.damage, 
                    weapon.weaponData.weaponTag, 
                    weapon.weaponData.lifeTime, 
                    levelData.scale, 
                    PoolType.BasicSlashHitEffect
                );
            }
            else
            {
                Debug.LogError($"[Player Attack] 근접 프리팹에 AttackEffect 스크립트가 없습니다: {weapon.weaponData.weaponTag} - {instance.name}");
            }
        }
        else if (weapon.weaponData.weaponType == WeaponType.Magic)
        {
            PerformMagicAttack(weapon, levelData);
        }
        else
        {
            Debug.LogError($"[Player Attack] 알 수 없는 무기 타입입니다: {weapon.weaponData.weaponTag} - {weapon.weaponData.weaponType}");
        }
    }

    /// <summary>
    /// 마법 공격 (운석)을 수행하는 함수
    /// </summary>
    void PerformMagicAttack(EquippedWeapon weapon, LevelData levelData)
    {
        if (weapon.weaponData.projectilePrefab == null)
        {
            Debug.LogWarning($"[Player Attack] 마법 공격 이펙트 프리팹이 할당되지 않았습니다: {weapon.weaponData.weaponTag}");
            return;
        }

        // 1. 마우스 위치를 월드 좌표의 지면으로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetGroundPosition;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            targetGroundPosition = hit.point; // 레이캐스트에 맞은 지점
        }
        else
        {
            // 지면이 없으면 플레이어 앞 일정 거리 지면으로 가정 (높이는 플레이어 높이와 동일)
            // 쿼터뷰에서 너무 높은 Y 값으로 생성되지 않도록 Y축은 플레이어와 동일하게 고정
            Plane groundPlane = new Plane(Vector3.up, transform.position);
            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                targetGroundPosition = ray.GetPoint(distance);
                // 플레이어와 너무 멀어지지 않게 제한할 경우
                // targetGroundPosition = Vector3.Lerp(transform.position, targetGroundPosition, 0.5f); 
                targetGroundPosition.y = transform.position.y; // Y축은 플레이어 높이와 동일하게
            }
            else
            {
                Debug.LogWarning("[Player Attack] 지면을 찾을 수 없습니다. 마법 공격을 플레이어 위치에 발동합니다.");
                targetGroundPosition = transform.position;
            }
        }

        // 2. 마법 발사체 (운석) 생성
        GameObject instance = ObjectPooler.Instance.SpawnFromPool(
            weapon.weaponData.weaponTag,
            targetGroundPosition, // 일단 지면 위치로 스폰 (MagicProjectile이 시작 높이에서 다시 설정)
            Quaternion.identity
        );

        if (instance == null) return;

        // 생성된 인스턴스 초기화 (MagicProjectile 컴포넌트 접근)
        MagicProjectile magicProjectile = instance.GetComponent<MagicProjectile>();
        if (magicProjectile != null)
        {
            magicProjectile.Initialize(
                this,   // IAttackOwner
                levelData.damage,
                weapon.weaponData.lifeTime, // WeaponData의 lifeTime을 운석의 낙하 지속 시간으로 사용
                levelData.scale,
                weapon.weaponData.weaponTag, // 운석 발사체 자신을 풀에 반납할 태그
                PoolType.MagicHitEffect,
                Color.blue // 기본공격은 물이니까 파란색으로 합니다람쥐쥐쥐
            );

            magicProjectile.StartFall(targetGroundPosition); // 운석 낙하 시작
        }
        else
        {
            Debug.LogError($"[Player Attack] 마법 프리팹에 MagicProjectile 스크립트가 없습니다: {weapon.weaponData.weaponTag} - {instance.name}");
            // 스크립트가 없으면 풀에 즉시 반납
            ObjectPooler.Instance.ReturnToPool(weapon.weaponData.weaponTag, instance);
        }
    }

    public void AddOrUpgradeWeapon(WeaponData weaponData)
    {
        EquippedWeapon existingWeapon = equippedWeapons.FirstOrDefault(w => w.weaponData == weaponData); // 이미 장착된 무기인지 확인
        // 무기 획득시 일단 차지공격 다 없애고, 타입에 맞게 다시 획득 ㄱㄱ
        GetComponent<PlayerMeleeChargeAttack>().enabled = false;
        GetComponent<PlayerRangeChargeAttack>().enabled = false;
        GetComponent<PlayerMagicChargeAttack>().enabled = false;

        switch (weaponData.weaponType)
        {
            case WeaponType.Melee:
                GetComponent<PlayerMeleeChargeAttack>().enabled = true;
                break;
            case WeaponType.Ranged:
                GetComponent<PlayerRangeChargeAttack>().enabled = true;
                break;
            case WeaponType.Magic:
                GetComponent<PlayerMagicChargeAttack>().enabled = true;
                break;
        }

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

    void IAttackOwner.NotifyHit()
    {
        if (!_canAttackHitStop) return;
        if (Time.time < _lastHitStopTime + hitStopCooldown) return; // 쿨타임 체크

        // 히트스톱 실행
        TimeManager.Instance.RequestTimeScale(this, 0.3f, 0.2f);
        _canAttackHitStop = false; // 한 공격당 한 번만 히트스톱 가능
        _lastHitStopTime = Time.time;
    }
}