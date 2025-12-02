using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("좌측하단 - 플레이어 정보")]
    [SerializeField] Image playerHpAmount;
    [SerializeField] Image playerChargeAttackAmount;
    [SerializeField] Color chargeAttackReadyColor;
    [SerializeField] Color chargeAttackNotReadyColor;

    [Header("플레이어 상단 - HP 바")]
    [SerializeField] Canvas playerHUDCanvas;
    [SerializeField] Image playerHpAmountHUD;


    // UI Manager에서는 직접 연결보다는 다른곳 함수발생->구독하고 있다가 실행 형식으로 사용할듯??
    #region 구독구독
    private void OnEnable()
    {
        GameManager.OnPlayerHpChanged += UpdatePlayerHp;
        GameManager.OnPlayerChargeChanged += UpdateChargeAttack;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerHpChanged -= UpdatePlayerHp;
        GameManager.OnPlayerChargeChanged -= UpdateChargeAttack;
    }
    #endregion

    private void LateUpdate()
    {
        // 체력 바가 항상 메인 카메라를 정면으로 바라보게
        if (Camera.main != null)
        {
            playerHUDCanvas.transform.LookAt(playerHUDCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    /// <summary>
    /// 좌측하단 플레이어정보 업데이트 + 플레이어 HUD도
    /// </summary>
    /// <param name="currentHp">현재 체력</param>
    /// <param name="maxHp">최대 체력</param>
    void UpdatePlayerHp(float currentHp, float maxHp)
    {
        playerHpAmount.fillAmount =currentHp / maxHp;
        playerHpAmountHUD.fillAmount = currentHp / maxHp;
    }

    void UpdateChargeAttack(int chrageAmount, int maxChargeAmount)
    {
        playerChargeAttackAmount.fillAmount = (float)chrageAmount / (float)maxChargeAmount;
        if (chrageAmount >= maxChargeAmount)
        {
            playerChargeAttackAmount.color = chargeAttackReadyColor;
        }
        else
        {
            playerChargeAttackAmount.color = chargeAttackNotReadyColor;
        }
    }
}
