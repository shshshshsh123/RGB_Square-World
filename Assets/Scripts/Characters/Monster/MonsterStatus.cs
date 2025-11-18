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
    private bool _isDead = false;

    [Header("UI 연결")]
    [Tooltip("체력을 표시할 슬라이더 UI")]
    public Slider hpSlider;
    [Tooltip("캔버스(회전용)")]
    public Canvas hpCanvas;

    private Monster_Melee_Coward _meleeCoward;

    private void Awake()
    {
        _meleeCoward = GetComponent<Monster_Melee_Coward>();
    }

    void OnEnable()
    {
        _isDead = false;
        _currentHp = maxHp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = _currentHp;
        }

        if (_meleeCoward != null)
        {
            _meleeCoward.ResetRunAwayStatus();
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
        if (_isDead) return;    // 2번죽는거 방지
        _currentHp -= (int)damage;
        UpdateHpBar();

        if (_meleeCoward != null)
        {
            _meleeCoward.RunAwayStatus((float)_currentHp, (float)maxHp);
        }

        if (_currentHp <= 0)
        {
            _isDead = true;

            if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
            {
                if (MissionManager.Instance.currentMission.Data.missionType == MissionType.Combat)
                    MissionManager.Instance.currentMission.AddProgress(1);
            }

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
        if (_isDead) return;
        if (other != null)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerTakeDamage(monsterDamage, gameObject);
            }
        }
    }
}
