using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("ü���� ǥ���� �����̴� UI")]
    public Slider hpSlider;
    [Tooltip("���ݵ��� ������ �θ� RectTransform")]
    [SerializeField] private RectTransform tickContainer;

    [Header("���� ������")]
    [SerializeField] private GameObject smallTickPrefab;
    [SerializeField] private GameObject largeTickPrefab;

    [Header("���� ����")]
    [SerializeField] private int smallTickInterval = 10; // ���� ���� ����
    [SerializeField] private int largeTickInterval = 100; // ū ���� ����

    // ��ũ��Ʈ Ȱ��ȭ�ɶ� �̺�Ʈ ����
    private void OnEnable()
    {
        GameManager.OnPlayerHpChanged += UpdateHp;
        GameManager.OnPlayerMaxHpChanged += GenerateTicks;
    }

    // ��ũ��Ʈ ��Ȱ��ȭ�� �� ������ ��� (�޸� ���� ����)
    private void OnDisable()
    {
        GameManager.OnPlayerHpChanged -= UpdateHp;
        GameManager.OnPlayerMaxHpChanged -= GenerateTicks;
    }

    private void LateUpdate()
    {
        // ü�� �ٰ� �׻� ���� ī�޶� �������� �ٶ󺸰�
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }

    /// <summary>
    /// ü�� ���� ���� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentHealth">���� ü��</param>
    /// <param name="maxHealth">�ִ� ü��</param>
    public void UpdateHp(float currentHealth, float maxHealth)
    {
        if (hpSlider != null)
        {
            // �����̴��� ���� 0�� 1 ���̷� ����ȭ�Ͽ� ����
            hpSlider.value = currentHealth / maxHealth;
        }
    }

    void GenerateTicks(float maxHp)
    {
        if (tickContainer == null || smallTickPrefab == null || largeTickPrefab == null) return;

        // ������ �ִ� ��� ������ �����մϴ�.
        foreach (Transform child in tickContainer)
        {
            Destroy(child.gameObject);
        }

        if (maxHp <= 0) return;

        // ü�� ���� ��ü �ʺ� ��������
        float barWidth = tickContainer.rect.width;

        // ���� ���� ���ݸ��� ������ ����
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