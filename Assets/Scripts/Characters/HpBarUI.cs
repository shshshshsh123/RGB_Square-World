using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("체력을 표시할 슬라이더 UI")]
    public Slider hpSlider;
    [Tooltip("눈금들이 생성될 부모 RectTransform")]
    [SerializeField] private RectTransform tickContainer;

    [Header("눈금 프리팹")]
    [SerializeField] private GameObject smallTickPrefab;
    [SerializeField] private GameObject largeTickPrefab;

    [Header("눈금 설정")]
    [SerializeField] private int smallTickInterval = 10; // 작은 눈금 간격
    [SerializeField] private int largeTickInterval = 100; // 큰 눈금 간격

    // 스크립트 활성화될때 이벤트 구독
    private void OnEnable()
    {
        GameManager.OnPlayerHpChanged += UpdateHp;
        GameManager.OnPlayerMaxHpChanged += GenerateTicks;
    }

    // 스크립트 비활성화될 때 구독을 취소 (메모리 누수 방지)
    private void OnDisable()
    {
        GameManager.OnPlayerHpChanged -= UpdateHp;
        GameManager.OnPlayerMaxHpChanged -= GenerateTicks;
    }

    private void LateUpdate()
    {
        // 체력 바가 항상 메인 카메라를 정면으로 바라보게
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
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

    void GenerateTicks(float maxHp)
    {
        if (tickContainer == null || smallTickPrefab == null || largeTickPrefab == null) return;

        // 기존에 있던 모든 눈금을 삭제합니다.
        foreach (Transform child in tickContainer)
        {
            Destroy(child.gameObject);
        }

        if (maxHp <= 0) return;

        // 체력 바의 전체 너비를 가져오고
        float barWidth = tickContainer.rect.width;

        // 작은 눈금 간격마다 눈금을 생성
        for (int i = smallTickInterval; i < maxHp; i += smallTickInterval)
        {
            bool isLargeTick = (i % largeTickInterval == 0);

            GameObject prefabToInstantiate = isLargeTick ? largeTickPrefab : smallTickPrefab;
            GameObject tickInstance = Instantiate(prefabToInstantiate, tickContainer);

            float xPos = (i / maxHp) * barWidth;

            RectTransform tickRect = tickInstance.GetComponent<RectTransform>();
            tickRect.anchoredPosition = new Vector2(xPos, 0);
        }
    }
}