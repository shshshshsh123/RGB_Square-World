using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    [Tooltip("ü���� ǥ���� �����̴� UI")]
    public Slider hpSlider;

    // ��ũ��Ʈ Ȱ��ȭ�ɶ� �̺�Ʈ ����
    private void OnEnable()
    {
        GameManager.OnPlayerHpChanged += UpdateHp;
    }

    // ��ũ��Ʈ ��Ȱ��ȭ�� �� ������ ��� (�޸� ���� ����)
    private void OnDisable()
    {
        GameManager.OnPlayerHpChanged -= UpdateHp;
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

    private void LateUpdate()
    {
        // ü�� �ٰ� �׻� ���� ī�޶� �������� �ٶ󺸰�
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
