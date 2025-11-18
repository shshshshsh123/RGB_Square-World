using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "Data/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("# 기본 정보")]
    public int id;  // 1000~1999: Defense, 2000~2999: Combat, 3000~3999: Exploration, 4000~4999: Survival
    public string title;
    [TextArea] public string description;
    public MissionType missionType;

    [Header("# 미션 목표")]
    public int targetCount;
    public float timeLimit;
    public string destinationID;    // 목적지가 있는 미션이면 사용하는데, 이거 MissionPoint의 ID랑 똑같이 해야됨(string임)

    [Header("# 서브캐릭터 성격")]
    public SubCharacterBehavior subCharacterBehavior;
}

public enum MissionType
{
    Defense,    // 디펜스형 (목적지 있고, 호위)
    Combat, // 전투형 (몬스터 처치)
    Exploration,    // 탐험형 (아이템 수집)
    Survival,   // 버티기 (시간제한)
}

public enum SubCharacterBehavior
{
    Autonomous, // 자율형 (아마도 목적지로 쭉 이동하는애?)
    Combat, // 전투위주
    Follow, // 플레이어 따라다님
}