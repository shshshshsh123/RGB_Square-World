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

    bool CheckCritical(float criticalChance)
    {
        float chance = UnityEngine.Random.Range(0.0f, 1.0f);
        return chance <= criticalChance;
    }

    public void TakeDamage(float damage, float critcalChance, float criticalDamage)
    {
        if (_isDead) return;    // 2번죽는거 방지

        bool isCritical = CheckCritical(critcalChance);
        if (isCritical) damage *= criticalDamage;

        _currentHp -= (int)damage;
        UpdateHpBar();

        // 데미지 표시
        ShowDamageText(damage, isCritical);

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

    private void ShowDamageText(float damage, bool isCritical)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 1.2f; // 몬스터 위에 위치
        // 랜덤성을 주어 텍스트가 겹치지 않게 (살짝 옆으로 퍼지게)
        spawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;

        // 소환
        GameObject textObj = ObjectPooler.Instance.SpawnFromPool(PoolType.DamageText, spawnPosition, Quaternion.identity);

        // 값 설정
        DamageText dmgTextScript = textObj.GetComponent<DamageText>();
        dmgTextScript.SetDamage(damage, isCritical);
    }
}
