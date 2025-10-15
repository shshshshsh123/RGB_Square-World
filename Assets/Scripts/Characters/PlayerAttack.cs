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

    [Header("# 차지공격(근접)")]
    public int chargeAttackDamage = 20; // 기본 데미지 (강화하면 올라감)
    public float chargeAttackTimeRequire = 2.0f; // 몇초 눌러야지 발동??
    public int chargeAttackTarget = 3; // 몇명 때리나요? (강화하면 올라감)
    public float chargeAttackRange = 10.0f; // 범위
    public TrailRenderer chargeAttackTrail; // 차지공격시 나오는 궤적 이펙트
    public Image chargeAttackKeyDownImage; // 차지공격 키 누르고 있는 동안 채워지는 이미지 (UI)
    private bool _isCharging = false;
    private float _chargeTimer = 0.0f;

    private PlayerController _playerController;

    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        equippedWeapons = new List<EquippedWeapon>();
    }

    void Start()
    {
        chargeAttackKeyDownImage.fillAmount = 0f;
    }

    void Update()
    {
        // 기본공격
        HandleAutoAttacks();

        // 차지공격
        HandleChargeAttack();

        // 테스트용!!!!!!!!
        if (Input.GetKeyDown(KeyCode.Space)) AddOrUpgradeWeapon(weaponDatas[0]);
    }

    void HandleAutoAttacks()
    {
        if (!Input.GetMouseButton(0)) return;   // 좌클릭 중에만 발동

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

    /// <summary>
    /// 차지공격 발동확인
    /// </summary>
    void HandleChargeAttack()
    {
        // 게이지 다찼나?
        if (!GameManager.Instance.CanChargeAttack) return;

        // 차지 공격 키 처음 누르면 타이머 시작
        if (Input.GetMouseButtonDown(1))
        {
            _isCharging = true;
            _chargeTimer = 0.0f;
        }

        // 차지 공격 키를 누르고 있는 동안
        if (Input.GetMouseButton(1) && _isCharging)
        {
            // Time.timeScale에 영향을 받지 않는 unscaledDeltaTime을 사용합니다.
            _chargeTimer += Time.unscaledDeltaTime;
            chargeAttackKeyDownImage.fillAmount = Mathf.Clamp01(_chargeTimer / chargeAttackTimeRequire);

            if (_isCharging && _chargeTimer >= chargeAttackTimeRequire)
            {
                // 차지 성공! 공격 실행
                StartCoroutine(PerformChargeAttack());
                _isCharging = false;
                _chargeTimer = 0f;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            // 차지 실패 또는 취소
            _isCharging = false;
            _chargeTimer = 0f;
            chargeAttackKeyDownImage.fillAmount = 0f;
        }

        IEnumerator PerformChargeAttack()
        {
            // 0. UI 반짝임 한번만
            Color originalColor = chargeAttackKeyDownImage.color;
            chargeAttackKeyDownImage.color = Color.red;
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one * 1.2f;
            yield return new WaitForSecondsRealtime(0.3f);
            chargeAttackKeyDownImage.color = originalColor;
            chargeAttackKeyDownImage.fillAmount = 0f;
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one;

            // 1. 범위 내의 적 탐색
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, chargeAttackRange, LayerMask.GetMask("Enemy"));

            // 2. 랜덤 적부터 정렬후 타겟 수 만큼 선택
            List<Transform> targets = enemiesInRange
                .OrderBy(enemy => Random.value)
                .Take(chargeAttackTarget)
                .Select(enemy => enemy.transform)
                .ToList();

            if (targets.Count == 0)
            {
                Debug.Log("[차지공격] 타겟이 없습니다.");
                yield break; // 타겟이 없으면 종료
            }

            // 타겟이 부족할 경우, 이미 찾은 적들을 반복해서 추가
            List<Transform> finalTargets = new List<Transform>();
            for (int i = 0; i < chargeAttackTarget; i++)
            {
                finalTargets.Add(targets[i % targets.Count]);
            }

            // 3. 시간 정지하고 발동준비
            Vector3 originalPosition = transform.position;  // 원래 위치 저장
            Time.timeScale = 0.0f; // 시간 정지!!!!!!!!
            GetComponent<Collider>().enabled = false; // 콜라이더 잠깐 비활성화해서 끼거나 이상한거 방지하기
            chargeAttackTrail.emitting = true; // 궤적 이펙트 시작

            // 4. 타겟 위치로 순간이동하며 공격
            foreach (Transform target in finalTargets)
            {
                // 타겟의 콜라이더 경계를 가져오기
                Collider targetCollider = target.GetComponent<Collider>();
                Vector3 targetBounds = targetCollider != null ? targetCollider.bounds.extents : Vector3.one;

                // 타겟 주변 랜덤 방향 벡터 생성 (Y축은 제외)
                Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
                Vector3 randomDirection = new Vector3(randomDirection2D.x, 0, randomDirection2D.y);

                // 타겟 콜라이더 크기에 비례하여 랜덤한 거리만큼 떨어진 위치 계산
                float randomDistance = Random.Range(0.6f, targetBounds.magnitude + 1.0f);
                Vector3 teleportPosition = target.position + randomDirection * randomDistance;

                // 계산된 위치로 순간이동
                transform.position = teleportPosition;

                // 타겟을 바라보며 슬래시 이펙트 생성
                transform.LookAt(target);
                GameObject effect = ObjectPooler.Instance.SpawnFromPool(PoolType.ChargeSlash, transform.position + Vector3.up, transform.rotation);
                effect.GetComponent<AttackEffect>().InitialValues(0, PoolType.ChargeSlash, 1.5f, 1f); // 데미지는 0으로 설정 (데미지는 돌아와서 줌), scale은 1고정??

                yield return new WaitForSecondsRealtime(0.1f); // 잠깐 대기 (RealTime써서 TimeScale 무시하기 - 시간 멈춰있음)
            }

            // 5. 원래 위치로 돌아오기 + 데미지주기
            transform.position = originalPosition;

            // 돌아오고 잠깐 있다가 데미지 주기 (간지용)
            yield return new WaitForSecondsRealtime(0.2f);

            GetComponent<Collider>().enabled = true; // 콜라이더 다시 활성화

            foreach (Transform uniqueTarget in targets)
            {
                // 최종 타겟 리스트에서 해당 타겟이 몇 번 포함되었는지 계산하여 데미지 배율 적용
                int hitCount = finalTargets.Count(t => t == uniqueTarget);
                float totalDamage = chargeAttackDamage * hitCount;

                uniqueTarget.GetComponent<MonsterStatus>()?.TakeDamage(totalDamage);
            }

            Time.timeScale = 1.0f; // 시간 다시 정상화
            chargeAttackTrail.emitting = false; // 궤적 이펙트 종료
            GameManager.Instance.IncreaseChargeAttack(-100);    // 게이지 정상화
        }
    }
}