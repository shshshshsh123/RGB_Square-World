using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();
        InitalizeKillRecords();
    }

    WeaponType currentWeaponType = WeaponType.Melee;    // 선택 안하고 시작하면 근접무기로 시작하게
    [SerializeField] private int _gold = 0;
    [SerializeField] private int _fariyStone = 0;

    public WeaponType CurrentWeaponType { get => currentWeaponType; set => currentWeaponType = value; }
    public int Gold { get => _gold; set => _gold = value; }
    public int FairyStone { get => _fariyStone; set => _fariyStone = value; }

    // --- AI 데이터 수집용 ---
    private Dictionary<WeaponType, int> _killRecords = new Dictionary<WeaponType, int>();

    void InitalizeKillRecords()
    {
        // 까아아알끔하게 초기화
        _killRecords.Clear();
        foreach (WeaponType type in System.Enum.GetValues(typeof(WeaponType)))
        {
            _killRecords[type] = 0;
        }
    }

    /// <summary>
    /// 몬스터 킬했으면 기록합니다. (무기종류는 데이터 매니저에서 알아서 처리함)
    /// </summary>
    public void RecordEnemyKill()
    {
        _killRecords[currentWeaponType]++;
    }
    
    /// <summary>
    /// 특정 무기 타입의 처치수를 반환합니다
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetKillCount(WeaponType type)
    {
        if (_killRecords.ContainsKey(type))
            return _killRecords[type];
        return 0;
    }
}
