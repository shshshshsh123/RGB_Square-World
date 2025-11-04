using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("# 기본 정보")]
    public WeaponType weaponType; // 무기 타입 (근접, 원거리 등)
    public PoolType weaponTag; // 무기 태그 (오브젝트 풀링용)
    public Sprite weaponIcon; // UI에 표시될 아이콘
    public float lifeTime; // 무기 지속 시간 (초, -1이면 무한)
    [TextArea]
    public string weaponDescription; // 무기 설명

    [Header("# 공격 위치 프리셋")]
    public Vector3 attackPositionOffset; // 공격 위치 오프셋 (플레이어 기준)
    public Vector3 attackRotationOffset; // 공격 회전 오프셋 (플레이어 기준)

    [Header("# 레벨별 공격 속성")]
    public List<LevelData> levelDataList; // 각 레벨별 공격 속성 리스트

    [Header("# 프리팹 연결")]
    public GameObject projectilePrefab; // 발사할 투사체 또는 공격 이펙트 프리팹
}

[System.Serializable]
public class LevelData
{
    public int level;
    public float damage;
    public float attackDelay;
    public float scale; // 공격 범위 (근접 무기용)
    public int projectileCount; // 발사되는 투사체 수 (원거리 무기용, 근접은 1)
    public int penetrationCount; // 투사체가 관통할 수 있는 적 수 (근접은 한번에 때릴 수 있는 적 수)
    public float projectileSpeed; // 투사체 속도 (원거리 무기용)
}

public enum WeaponType
{
    Melee,      // 근접 무기
    Ranged,      // 원거리 무기
    Magic       // 마법 무기
}