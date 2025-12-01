using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("# 현재 미션")]
    public Mission currentMission;
    public TMP_Text missionTitleText;
    //public TMP_Text missionDescText;
    public TMP_Text missionProgressText;

    [Header("# 플레이어 네비게이션")]
    public GameObject playerNav;

    [Header("# 서브캐릭터 호위 미션")]
    public bool isPlayerReachDestination = false;
    public bool isSubCharacterReachDestination = false;
    SubCharacterAI _subCharacterAI;

    // 미션 포인트(목적지) 저장용 딕셔너리
    private Dictionary<string, Transform> _missionPoints = new Dictionary<string, Transform>();

    private MonsterSpawner _monsterSpawner;

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

        // MonsterSpawner 참조 가져오기
        _monsterSpawner = GetComponent<MonsterSpawner>();
    }

    public void StartMission(MissionData missionData)
    {
        // 1. 미션 생성
        currentMission = new Mission(missionData);

        // 2. 이전 미션의 몬스터 스폰 초기화 및 스폰 시작
        _monsterSpawner.StopSpawning();
        _monsterSpawner.StartSpawning(missionData.monsterSpawnInfo);

        // 3. 이동미션이면 설정해주기
        if (missionData.missionType == MissionType.Defense)
        {
            playerNav.SetActive(true);
            // 딕셔너리에서 ID로 실제 Transform을 찾음
            if (_missionPoints.TryGetValue(missionData.destinationID, out Transform targetTransform))
            {
                playerNav.GetComponent<ArrowIndicator>().SetTarget(targetTransform);
                Debug.Log($"목표 지점 설정 완료: {missionData.destinationID}");

                // 서브 캐릭터 소환
                GameObject subCharObj = ObjectPooler.Instance.SpawnFromPool(PoolType.SubCharacter, playerNav.transform.position, Quaternion.identity);
                _subCharacterAI = subCharObj.GetComponent<SubCharacterAI>();
                _subCharacterAI.SetDestination(targetTransform.position);
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

        // 4. UI 업데이트
        missionTitleText.text = missionData.title;
        //missionDescText.text = missionData.description;
        missionProgressText.text = currentMission.GetProgressText();
    }

    // --- 외부에서 수신받는 함수들 ---

    // 몬스터 처치시
    public void OnEnemyKilled()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Combat)
        {
            currentMission.AddProgress(1);
        }
        missionProgressText.text = currentMission.GetProgressText();
    }

    // 퀘스트 아이템 습득시
    public void OnItemCollected()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Exploration)
        {
            currentMission.AddProgress(1);
        }
        missionProgressText.text = currentMission.GetProgressText();
    }

    // 목적지 도착 시 호출
    public void OnDestinationReached()
    {
        // 서브 캐릭터가 도착했으면 발동하기!!
        if (currentMission != null && currentMission.Data.missionType == MissionType.Defense && isSubCharacterReachDestination && !isPlayerReachDestination)
        {
            currentMission.AddProgress(-1); // 목표치를 -2로 설정할거임
            isPlayerReachDestination = true;
        }
        missionProgressText.text = currentMission.GetProgressText();
    }

    // 서브캐릭터가 목적지 도착시 호출
    public void OnSubCharacterDestinationReached()
    {
        if (currentMission != null && currentMission.Data.missionType == MissionType.Defense && !isSubCharacterReachDestination)
        {
            currentMission.AddProgress(-1); // 목표치를 -2로 설정할거임
            isSubCharacterReachDestination = true;
        }
        missionProgressText.text = currentMission.GetProgressText();
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
