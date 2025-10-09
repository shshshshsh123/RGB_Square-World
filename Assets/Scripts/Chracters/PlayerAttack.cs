using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    [Header("# 무기 데이터 리스트")]
    public List<WeaponData> weaponDataList; // 게임 내 존재하는 모든 무기 데이터 리스트

    [Header("# 현재 무기 데이터")]
    public WeaponData currentWeapon; // 현재 장착된 무기 데이터

    [Header("# 차지공격(근접)")]
    public KeyCode chargeAttackKey = KeyCode.E; // 차지공격 키 (나중에는 PlayerInput등으로 처리할듯?)
    public float chargeAttackTimeRequire = 2.0f; // 몇초 눌러야지 발동??
    public int chargeAttackTarget = 3; // 몇명 때리나요? (강화하면 올라감)
    public float chargeAttackRange = 10.0f; // 범위
    public string chargeSlashEffectTag = "ChargeSlashEffect"; // 차지공격 이펙트 태그 (오브젝트풀링용)
    public TrailRenderer chargeAttackTrail; // 차지공격시 나오는 궤적 이펙트
    private bool _isCharging = false;
    private float _chargeTimer = 0.0f;

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

        HandleChargeAttack();
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

    /// <summary>
    /// 차지공격 발동확인
    /// </summary>
    void HandleChargeAttack()
    {
        // 차지 공격 키 처음 누르면 타이머 시작
        if (Input.GetKeyDown(chargeAttackKey))
        {
            _isCharging = true;
            _chargeTimer = 0.0f;
        }

        // 차지 공격 키를 누르고 있는 동안
        if (Input.GetKey(chargeAttackKey) && _isCharging)
        {
            // Time.timeScale에 영향을 받지 않는 unscaledDeltaTime을 사용합니다.
            _chargeTimer += Time.unscaledDeltaTime;

            if (_isCharging && _chargeTimer >= chargeAttackTimeRequire)
            {
                // 차지 성공! 공격 실행
                StartCoroutine(PerformChargeAttack());
                _isCharging = false;
                _chargeTimer = 0f;
            }
            // TODO: 차지공격 UI에 표시하기
        }

        if (Input.GetKeyUp(chargeAttackKey))
        {
            // 차지 실패 또는 취소
            _isCharging = false;
            _chargeTimer = 0f;
        }

        IEnumerator PerformChargeAttack()
        {
            // 1. 범위 내의 적 탐색
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, chargeAttackRange, LayerMask.GetMask("Enemy"));

            // 2. 플레이어에게서 가장 가까운 적부터 정렬후 타겟 수 만큼 선택
            List<Transform> targets = enemiesInRange
                .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
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
                ObjectPooler.Instance.SpawnFromPool(chargeSlashEffectTag, transform.position + Vector3.up, transform.rotation);

                yield return new WaitForSecondsRealtime(0.1f); // 잠깐 대기 (RealTime써서 TimeScale 무시하기 - 시간 멈춰있음)
            }

            // 5. 원래 위치로 돌아오기 + 데미지주기
            transform.position = originalPosition;
            GetComponent<Collider>().enabled = true; // 콜라이더 다시 활성화

            foreach (Transform uniqueTarget in targets)
            {
                // 최종 타겟 리스트에서 해당 타겟이 몇 번 포함되었는지 계산하여 데미지 배율 적용
                int hitCount = finalTargets.Count(t => t == uniqueTarget);
                float totalDamage = currentWeapon.damage * 2 * hitCount;

                uniqueTarget.GetComponent<DummyDamage>()?.TakeDamage(totalDamage);
                Debug.Log($"{uniqueTarget.name}에게 총 {totalDamage}의 데미지 ({hitCount}회)!");
            }

            Time.timeScale = 1.0f; // 시간 다시 정상화
            chargeAttackTrail.emitting = false; // 궤적 이펙트 종료
        }
    }
}