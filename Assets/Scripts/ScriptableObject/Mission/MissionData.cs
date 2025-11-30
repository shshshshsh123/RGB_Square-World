using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Data/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("# 기본 정보")]
    public int id;  // 1000~1999: Defense, 2000~2999: Combat, 3000~3999: Exploration, 4000~4999: Survival, 5000~5999: Boss
    public string title;
    [TextArea] public string description;
    public MissionType missionType;

    [Header("# 미션 목표")]
    public int targetCount;
    public float timeLimit;
    public string destinationID;    // 목적지가 있는 미션이면 사용하는데, 이거 MissionPoint의 ID랑 똑같이 해야됨(string임)

    [Header("# 서브캐릭터 타입")]
    public SubCharacterBehavior subCharacterBehavior;
    
    [Header("# 몬스터 스폰")]
    public MonsterSpawnInfo monsterSpawnInfo;
}

public enum MissionType
{
    Defense,    // 디펜스형 (목적지 있고, 호위)
    Combat, // 전투형 (몬스터 처치)
    Exploration,    // 탐험형 (아이템 수집)
    Survival,   // 버티기 (시간제한)
    Boss    // 보스전 (보스 몬스터 처치)
}

public enum SubCharacterBehavior
{
    MoveToDestination, // 목적지로 이동
    Combat, // 전투위주
    Follow, // 플레이어 따라다님
}

[System.Serializable]
public struct MonsterWeight
{
    public PoolType monsterType;
    [Range(1, 100)]
    [Tooltip("상대적 확률 가중치 (높을수록 자주 나옴)")]
    public int weight;
}

[System.Serializable]
public class MonsterSpawnInfo
{
    [Header("Monster Mix")]
    [Tooltip("스폰될 몬스터 종류와 가중치 목록(모든 값 합 100안되도 됩니다. 대신 int값임")]
    public List<MonsterWeight> monsterWeights; // <- 여기가 핵심 변경점

    [Tooltip("스폰 간격 (초)")]
    public float spawnInterval;
    [Tooltip("최대 스폰 마리수 (0이면 무제한)")]
    public int maxSpawnCount;
}