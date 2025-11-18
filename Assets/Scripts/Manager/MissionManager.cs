using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("# 현재 미션")]
    public Mission currentMission;

    [Header("# 플레이어 네비게이션")]
    public GameObject playerNav;

    //[Header("# 서브캐릭터")]
    //public SubCharacterAI subCharacter;

    // 미션 포인트(목적지) 저장용 딕셔너리
    private Dictionary<string, Transform> _missionPoints = new Dictionary<string, Transform>();

    private void Awake()
    {
        Instance = this;

        // 이번 스테이지의 MissionPoint 찾기
        MissionPoint[] points = FindObjectsByType<MissionPoint>(FindObjectsSortMode.None);
        foreach (var point in points)
        {
            if (!_missionPoints.ContainsKey(point.id))
            {
                _missionPoints.Add(point.id, point.transform);
            }
            else
            {
                Debug.LogWarning($"중복된 MissionPoint ID가 있습니다: {point.id}");
            }
        }
    }

    public void StartMission(MissionData missionData)
    {
        // 1. 미션 생성
        currentMission = new Mission(missionData);

        // 2. UI표시?? 일단 미정

        // 3. 서브 캐릭터 AI 구현

        // 4. 이동미션이면 설정해주기
        if (missionData.missionType == MissionType.Defense)
        {
            playerNav.SetActive(true);
            // 딕셔너리에서 ID로 실제 Transform을 찾음
            if (_missionPoints.TryGetValue(missionData.destinationID, out Transform targetTransform))
            {
                playerNav.GetComponent<ArrowIndicator>().SetTarget(targetTransform);
                Debug.Log($"목표 지점 설정 완료: {missionData.destinationID}");
            }
            else
            {
                Debug.LogError($"해당 ID의 MissionPoint를 찾을 수 없습니다: {missionData.destinationID}");
            }
        }
        else
        {
            playerNav.SetActive(false);
        }
    }

    // --- 외부에서 수신받는 함수들 ---

    // 몬스터 처치시
    public void OnEnemyKilled()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Combat)
        {
            currentMission.AddProgress(1);
        }
    }

    // 퀘스트 아이템 습득시
    public void OnItemCollected()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Exploration)
        {
            currentMission.AddProgress(1);
        }
    }

    // 목적지 도착 시 호출
    public void OnDestinationReached()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Defense)
        {
            currentMission.AddProgress(-1); // 목표치를 -1으로 설정할거임
        }
    }

    // 서브 캐릭터 사망 시 호출
    public void OnSubCharacterDied()
    {
        if (currentMission != null)
        {
            currentMission.Fail();
        }
    }
}
