using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    [Tooltip("체력을 표시할 슬라이더 UI")]
    public Slider hpSlider;

    // 스크립트 활성화될때 이벤트 구독
    private void OnEnable()
    {
        GameManager.OnPlayerHpChanged += UpdateHp;
    }

    // 스크립트 비활성화될 때 구독을 취소 (메모리 누수 방지)
    private void OnDisable()
    {
        GameManager.OnPlayerHpChanged -= UpdateHp;
    }

    /// <summary>
    /// 체력 바의 값을 업데이트합니다.
    /// </summary>
    /// <param name="currentHealth">현재 체력</param>
    /// <param name="maxHealth">최대 체력</param>
    public void UpdateHp(float currentHealth, float maxHealth)
    {
        if (hpSlider != null)
        {
            // 슬라이더의 값을 0과 1 사이로 정규화하여 설정
            hpSlider.value = currentHealth / maxHealth;
        }
    }

    private void LateUpdate()
    {
        // 체력 바가 항상 메인 카메라를 정면으로 바라보게
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
