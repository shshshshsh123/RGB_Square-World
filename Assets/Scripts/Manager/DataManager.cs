using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    WeaponType currentWeaponType = WeaponType.Melee;    // 선택 안하고 시작하면 근접무기로 시작하게
    [SerializeField] private int _gold = 0;
    [SerializeField] private int _fariyStone = 0;

    public WeaponType CurrentWeaponType { get => currentWeaponType; set => currentWeaponType = value; }


}
