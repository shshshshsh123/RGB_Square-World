using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("기본 정보")]
    public string weaponName; // 무기 이름
    public Sprite weaponIcon; // UI에 표시될 아이콘
    [TextArea]
    public string weaponDescription; // 무기 설명

    [Header("공격 속성")]
    public float damage; // 공격력
    public float attackDelay; // 공격 딜레이 (초)
    public float projectileSpeed; // 투사체 속도 (원거리 무기용)

    [Header("프리팹 연결")]
    public GameObject projectilePrefab; // 발사할 투사체 또는 공격 이펙트 프리팹
}