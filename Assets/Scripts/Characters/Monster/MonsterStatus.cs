using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MonsterKnockBack))]
public class MonsterStatus : MonoBehaviour
{
    [Header("# ���� ���� ����")]
    public int maxHp = 100;
    public PoolType monsterTag;
    public float monsterDamage = 10f;
    public int monsterChargeAmount = 1; // ���� ������ ���� ������ (ų�ϸ� 3��? �ϴ� �װ� ����)
    private int _currentHp;
    private bool _isDead = false;

    [Header("UI ����")]
    [Tooltip("ü���� ǥ���� �����̴� UI")]
    public Slider hpSlider;
    [Tooltip("ĵ����(ȸ����)")]
    public Canvas hpCanvas;

    void OnEnable()
    {
        _isDead = false;
        _currentHp = maxHp;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = _currentHp;
        }
    }

    private void LateUpdate()
    {
        // ü�� �ٰ� �׻� ���� ī�޶� �������� �ٶ󺸰�
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
        if (_isDead) return;    // 2���״°� ����

        bool isCritical = CheckCritical(critcalChance);
        if (isCritical) damage *= criticalDamage;

        _currentHp -= (int)damage;
        UpdateHpBar();

        // ������ ǥ��
        ShowDamageText(damage, isCritical);

        if (_currentHp <= 0)
        {
            _isDead = true;

            if (MissionManager.Instance != null && MissionManager.Instance.currentMission != null)
            {
                if (MissionManager.Instance.currentMission.Data.missionType == MissionType.Combat)
                    MissionManager.Instance.OnEnemyKilled();
            }

            ObjectPooler.Instance.ReturnToPool(monsterTag, gameObject);
            DataManager.Instance.RecordEnemyKill();
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
            else if (other.CompareTag("SubCharacter"))
            {
                other.GetComponent<SubCharacterStatus>()?.TakeDamage(monsterDamage);
            }
        }
    }

    private void ShowDamageText(float damage, bool isCritical)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * 1.2f; // ���� ���� ��ġ
        // �������� �־� �ؽ�Ʈ�� ��ġ�� �ʰ� (��¦ ������ ������)
        spawnPosition += UnityEngine.Random.insideUnitSphere * 0.5f;

        // ��ȯ
        GameObject textObj = ObjectPooler.Instance.SpawnFromPool(PoolType.DamageText, spawnPosition, Quaternion.identity);

        // �� ����
        DamageText dmgTextScript = textObj.GetComponent<DamageText>();
        dmgTextScript.SetDamage(damage, isCritical);
    }
}
