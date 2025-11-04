using UnityEngine;
using System.Collections;

public class PlayerMagicChargeAttack : PlayerChargeAttackBase
{
    [Header("# 마법 차지공격 설정")]
    public int chargeAttackDamage = 30;         // 마법 차지 공격 기본 데미지 (각 투사체 착지 시 범위 데미지)
    public float projectileFallDuration = 1.0f; // 마법 투사체 낙하에 걸리는 시간
    public float projectileScale = 1.0f;        // 마법 투사체 크기
    public float spawnRadius = 3f;              // 플레이어 주변 투사체 생성 반경
    public float delayBetweenProjectiles = 0.05f; // 각 투사체 발사 사이의 딜레이
    public PoolType projectilePoolTag = PoolType.MagicChargeProjectile; // 마법 투사체 풀 태그

    private PlayerAttack _playerAttack; // IAttackOwner 구현용 (데미지 처리)
    private LayerMask _groundLayer; // 마법 투사체가 떨어질 지면을 찾기 위한 레이어

    private void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _groundLayer = LayerMask.GetMask("Ground");
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// 실제 마법 차지 공격의 로직을 구현합니다.
    /// 플레이어 주변의 랜덤한 지면 위치에 MagicProjectile을 떨어뜨립니다.
    /// </summary>
    protected override IEnumerator PerformChargeAttack()
    {
        // 0. UI 반짝임 (UI는 Time.unscaledDeltaTime/WaitForSecondsRealtime 사용)
        if (chargeAttackKeyDownImage != null)
        {
            Color originalColor = chargeAttackKeyDownImage.color;
            chargeAttackKeyDownImage.color = Color.blue; // 마법 공격은 파란색?
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one * 1.2f;
            yield return new WaitForSecondsRealtime(0.3f); // UI 연출은 Realtime으로
            chargeAttackKeyDownImage.color = originalColor;
            chargeAttackKeyDownImage.fillAmount = 0f;
            chargeAttackKeyDownImage.rectTransform.localScale = Vector3.one;
        }

        // 1. 투사체 발사 루프
        for (int i = 0; i < chargeAttackTarget; i++) // chargeAttackTarget 수만큼 투사체 발사
        {
            // 플레이어 주변 랜덤 위치 계산 (지면으로)
            Vector3 randomOffset2D = Random.insideUnitCircle * spawnRadius;
            Vector3 potentialTargetPosition = transform.position + new Vector3(randomOffset2D.x, 0, randomOffset2D.y);

            Vector3 targetGroundPosition;
            RaycastHit hit;
            // 플레이어 주변 랜덤 위치에서 아래로 Raycast하여 지면 위치
            if (Physics.Raycast(potentialTargetPosition + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity, _groundLayer))
            {
                targetGroundPosition = hit.point;
            }
            else
            {
                // 지면을 찾지 못하면 플레이어의 Y 위치를 사용 (안전 장치)
                targetGroundPosition = new Vector3(potentialTargetPosition.x, transform.position.y, potentialTargetPosition.z);
                Debug.LogWarning($"[PlayerMagicChargeAttack] 랜덤 위치에서 지면을 찾지 못했습니다. {potentialTargetPosition}. 플레이어 Y 위치 사용.");
            }

            // 2. 투사체 풀에서 가져오기
            GameObject projectileInstance = ObjectPooler.Instance.SpawnFromPool(projectilePoolTag, targetGroundPosition, Quaternion.identity);
            if (projectileInstance != null)
            {
                MagicProjectile magicProjectile = projectileInstance.GetComponent<MagicProjectile>();
                if (magicProjectile != null)
                {
                    magicProjectile.Initialize(
                        _playerAttack,          // IAttackOwner
                        chargeAttackDamage,     // 데미지
                        projectileFallDuration, // 낙하 시간
                        projectileScale,        // 크기
                        projectilePoolTag,       // 풀 태그 (반납용)
                        PoolType.MagicChargeHitEffect,  // 착지 이펙트 풀 태그
                        Color.yellow // 차지공격은 별이니까 노란색입니다람쥐쥐쥐
                    );
                    magicProjectile.StartFall(targetGroundPosition); // 낙하 시작
                }
                else
                {
                    Debug.LogError($"[PlayerMagicChargeAttack] MagicProjectile 프리팹에 MagicProjectile 스크립트가 없습니다! 태그: {projectilePoolTag}");
                    // 스크립트가 없다면 수동으로 풀에 반납 시도
                    ObjectPooler.Instance.ReturnToPool(projectilePoolTag, projectileInstance);
                }
            }
            else
            {
                Debug.LogError($"[PlayerMagicChargeAttack] {projectilePoolTag} 풀에서 오브젝트를 가져오지 못했습니다.");
            }

            yield return new WaitForSeconds(delayBetweenProjectiles); // 각 투사체 발사 사이 딜레이 (Time.timeScale 영향 받음)
        }

        // Debug.Log("[PlayerMagicChargeAttack] 모든 투사체 발사 완료. 정리 시작.");

        // 3. 여러가지 정상화들
        GameManager.Instance.IncreaseChargeAttack(-100);    // 게이지 초기화
        _isCharging = false;
        _chargeTimer = 0f;

        yield break; // 코루틴 종료
    }
}
