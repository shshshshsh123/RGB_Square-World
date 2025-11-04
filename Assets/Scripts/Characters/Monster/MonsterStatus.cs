using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonsterKnockBack))]
public class MonsterStatus : MonoBehaviour
{
    [Header("# 몬스터 스탯 정보")]
    public int maxHp = 100;
    public PoolType monsterTag;
    public float monsterDamage = 10f;
    public int monsterChargeAmount = 1; // 몬스터 때리면 차는 차지양 (킬하면 3배? 일단 그건 보류)
    private int _currentHp;

    [Header("UI 연결")]
    [Tooltip("체력을 표시할 슬라이더 UI")]
    public Slider hpSlider;
    [Tooltip("캔버스(회전용)")]
    public Canvas hpCanvas;

    void OnEnable()
    {
        _currentHp = maxHp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = _currentHp;
        }
    }

    private void LateUpdate()
    {
        // 체력 바가 항상 메인 카메라를 정면으로 바라보게
        if (Camera.main != null)
        {
            hpCanvas.transform.LookAt(hpCanvas.transform.position + Camera.main.transform.forward);
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHp -= (int)damage;
        UpdateHpBar();
        if (_currentHp <= 0)
        {
            ObjectPooler.Instance.ReturnToPool(monsterTag, gameObject);
        }
        GameManager.Instance.IncreaseChargeAttack(monsterChargeAmount);
    }

    void UpdateHpBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = _currentHp;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerTakeDamage(monsterDamage, gameObject);
            }
        }
    }
}
