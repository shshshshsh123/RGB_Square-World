using UnityEngine;
using System.Collections;

public class PlayerRangeChargeAttack : PlayerChargeAttackBase
{
    [Header("# 원거리 차지공격 설정")]
    public int chargeAttackDamage = 40; // 원거리 차지 공격 기본 데미지
    public float chargeAttackSpeed = 10f; // 원거리 차지 공격 투사체 속도
    public float chargeAttackLifetime = 5f; // 원거리 차지 공격 투사체 수명
    public PoolType projectilePoolTag = PoolType.ChargeArrow;   // 투사체 풀 태그
    public Vector3 launchOffset = new Vector3(0, 0.5f, 1.0f);   // 투사체 발사 위치 오프셋
    public float knockbackForce = 10f;  // 넉백 힘
    public float knockbackDuration = 0.2f;  // 넉백 지속 시간

    public SkillCutInUI skillCutInUI;

    private PlayerController _playerController; // 방향 참고용
    private PlayerAttack _playerAttack; // IAttackOwner 구현용

    private float _criticalChance;
    private float _criticalDamage;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerAttack = GetComponent<PlayerAttack>();
    }

    protected override void Start()
    {
        base.Start();
        _criticalChance = _playerAttack.criticalChance;
        _criticalDamage = _playerAttack.criticalDamage;
    }

    /// <summary>
    /// 실제 원거리 차지 공격의 로직을 구현
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator PerformChargeAttack()
    {
        // 0. UI 반짝임
        TimeManager.Instance.RequestTimeScale(this, 0.0f);
        if (chargeAttackKeyDownImage != null)
        {
            Color originalColor = chargeAttackKeyDownImage.color;
            chargeAttackKeyDownImage.color = Color.cyan; // 다른 색상으로 구분
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one * 1.2f;
            if (skillCutInUI != null)
            {
                skillCutInUI.ShowCutIn(CutInType.Range); // 컷인 표시
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
        TimeManager.Instance.RestoreTimeScale(this);

        // 1. 발사 준비
        Quaternion launchRotation = _playerController.Rotation;
        Vector3 launchPosition = transform.position + (launchRotation * launchOffset);

        // 2. 투사체 풀에서 가져오기
        GameObject projectile = ObjectPooler.Instance.SpawnFromPool(projectilePoolTag, launchPosition, launchRotation);
        if (projectile != null)
        {
            RangedProjectile rangedProjectile = projectile.GetComponent<RangedProjectile>();
            if (rangedProjectile != null)
            {
                rangedProjectile.Initialize(
                    _playerAttack,
                    chargeAttackDamage,
                    chargeAttackSpeed,
                    -100, // 무한 관통
                    projectilePoolTag,
                    chargeAttackLifetime,
                    _criticalChance,
                    _criticalDamage,
                    PoolType.ChargeArrrowHitEffect,
                    knockbackForce,
                    knockbackDuration
                );
                // 넉백처리는 RangedProjectile에서 합니둥
            }
        }

        // 3. 여러가지 정상화들
        GameManager.Instance.IncreaseChargeAttack(-100);    // 게이지 정상화_isCharging = false;
        _chargeTimer = 0f;

        yield break; // 코루틴 종료
    }
}
