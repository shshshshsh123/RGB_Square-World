using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerMeleeChargeAttack : PlayerChargeAttackBase, IAttackOwner
{
    [Header("# 근접 차지공격 설정")]
    public int chargeAttackDamage = 50; // 근접 차지 공격 기본 데미지
    public ParticleSystem chargeAttackTrail; // 근접 공격용 트레일 파티클
    public PoolType chargeSlashEffectTag = PoolType.ChargeSlash; // 근접 공격용 슬래시 이펙트 태그
    public float effectLifetime = 1.5f; // 슬래시 이펙트 지속 시간
    public float effectScale = 1f; // 슬래시 이펙트 크기
    public float teleportDelay = 0.1f; // 각 타겟 이동 사이 딜레이
    public float returnDelay = 0.2f; // 복귀 후 데미지 주기 전 딜레이
    public SkillCutInUI skillCutInUI;

    private float _criticalChance;
    private float _criticalDamage;

    protected override void Start()
    {
        base.Start();
        if (chargeAttackTrail != null && chargeAttackTrail.isPlaying)
        {
            chargeAttackTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    /// <summary>
    /// 근접 차지 공격(순간이동)의 실제 로직을 구현합니다.
    /// </summary>
    protected override IEnumerator PerformChargeAttack()
    {
        // 일단 발동성공했으면 시간 멈추고 시작
        TimeManager.Instance.RequestTimeScale(this, 0.0f);

        // 0. UI
        if (chargeAttackKeyDownImage != null)
        {
            Color originalColor = chargeAttackKeyDownImage.color;
            chargeAttackKeyDownImage.color = Color.red;
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one * 1.2f;
            if (skillCutInUI != null)
            {
                skillCutInUI.ShowCutIn(CutInType.Melee); // 컷인 표시
            }
            yield return new WaitForSecondsRealtime(0.8f);
            chargeAttackKeyDownImage.color = originalColor;
            chargeAttackKeyDownImage.fillAmount = 0f;
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one;
            if (skillCutInUI != null)
            {
                skillCutInUI.HideCutIn(); // 컷인 숨기기
            }
        }

        // 1. 범위 내의 적 탐색 (베이스 클래스 함수 사용)
        List<Transform> foundTargets = FindTargets();

        if (foundTargets.Count == 0)
        {
            Debug.Log("[차지공격] 타겟이 없습니다.");
            _isCharging = false; // 코루틴 종료 전 상태 초기화
            _chargeTimer = 0f;
            TimeManager.Instance.RestoreTimeScale(this);    // 발동실패했으니 시간 다시 정상화
            yield break;
        }

        // 2. 최종 공격 대상 리스트 생성 (베이스 클래스 함수 사용)
        List<Transform> finalTargets = GetFinalTargets(foundTargets);

        // 3. 발동준비
        Vector3 originalPosition = transform.position; 
        float fixedY = originalPosition.y; // Y축 위치 고정값 저장
        Collider playerCollider = GetComponent<Collider>(); // 콜라이더 미리 찾아두기
        if (playerCollider != null) playerCollider.enabled = false;
        if (chargeAttackTrail != null)
        {
            chargeAttackTrail.Clear();
            chargeAttackTrail.Play();
        }
        _criticalChance = GetComponent<PlayerAttack>().criticalChance;
        _criticalDamage = GetComponent<PlayerAttack>().criticalDamage;

        // 4. 타겟 위치로 순간이동하며 이펙트 생성
        foreach (Transform target in finalTargets)
        {
            // 타겟이 null이거나(파괴됨), 이미 비활성화되었다면(풀에 반납됨/죽음) 건너뛰기
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                continue;
            }
            Collider targetCollider = target.GetComponent<Collider>();
            Vector3 targetBounds = targetCollider != null ? targetCollider.bounds.extents : Vector3.one;
            Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
            Vector3 randomDirection = new Vector3(randomDirection2D.x, 0, randomDirection2D.y); // Y는 0
            float randomDistance = Random.Range(0.6f, targetBounds.magnitude + 1.0f);

            Vector3 teleportPosition = target.position + randomDirection * randomDistance;
            teleportPosition.y = fixedY; // Y축 위치를 원래 플레이어 높이로 고정

            // --- 순간 이동 ---
            transform.position = teleportPosition;

            Vector3 lookTargetPos = target.position;
            lookTargetPos.y = fixedY; // 바라보는 방향도 수평으로
            transform.LookAt(lookTargetPos);
            GameObject effect = ObjectPooler.Instance.SpawnFromPool(chargeSlashEffectTag, transform.position + Vector3.up, transform.rotation);
            if (effect != null)
            {
                AttackEffect attackEffect = effect.GetComponent<AttackEffect>();
                if (attackEffect != null)
                {
                    // 이펙트는 데미지 없음!!!
                    attackEffect.InitialValues(this, 0, chargeSlashEffectTag, effectLifetime, effectScale, 0f, 0f, PoolType.ChargeSlash);
                }
            }

            // 데미지처리 여기서 하기 (데미지 와바바박 띄우게 할라고)
            MonsterStatus monsterStatus = target.GetComponent<MonsterStatus>();
            if (monsterStatus != null)
            {
                // 각 타격마다 크리티컬 계산 적용하여 데미지 줌
                monsterStatus.TakeDamage(chargeAttackDamage, _criticalChance, _criticalDamage);
            }

            yield return new WaitForSecondsRealtime(teleportDelay); // 각 이동 사이 대기
        }

        // 5. 원래 위치로 돌아오기 + 데미지주기
        transform.position = originalPosition;

        yield return new WaitForSecondsRealtime(returnDelay); // 돌아오고 데미지 주기 전 대기

        if (playerCollider != null) playerCollider.enabled = true; // 콜라이더 다시 활성화

        TimeManager.Instance.RestoreTimeScale(this); // 시간 다시 정상화
        if (chargeAttackTrail != null)
        {
            chargeAttackTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        GameManager.Instance.IncreaseChargeAttack(-100);    // 게이지 정상화

        // 코루틴 종료 후 상태 초기화 (GetKeyUp에서도 처리하지만 안전하게)
        _isCharging = false;
        _chargeTimer = 0f;
    }

    void IAttackOwner.NotifyHit()
    {
        // 근접 차지공격은 타격 시 별도 처리 없음
    }
}