using UnityEngine;
using UnityEngine.UI;

public class SubCharacterStatus : MonoBehaviour
{
    private float _currentHp;
    private float _maxHp;

    [Header("# UI")]
    public GameObject hpBarCanvas;  // 카메라 바라보게 하는용
    public Image hpBarAmountImage;

    private void LateUpdate()
    {
        // 체력 바가 항상 메인 카메라를 정면으로 바라보게
        if (Camera.main != null)
        {
            hpBarCanvas.transform.LookAt(hpBarCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    /// <summary>
    /// 최대체력 설정(현재체력도 같이 초기화됩니당)
    /// </summary>
    /// <param name="maxHp"></param>
    public void Initialize(float maxHp)
    {
        _maxHp = maxHp;
        _currentHp = maxHp;
        hpBarAmountImage.fillAmount = _currentHp / _maxHp;
    }

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;
        hpBarAmountImage.fillAmount = _currentHp / _maxHp;
        if (_currentHp < 0)
        {
            MissionManager.Instance.OnSubCharacterDied();
        }
    }
}
