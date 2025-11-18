using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("# 이번 스테이지에 있을 미션 목록")]
    public List<MissionData> stageMission;

    private int _currentMissionIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 스테이지 시작하자마자 미션시작할거면 넣기 (지금 임시로 이렇게 테스트함)
        if (stageMission.Count > 0) StartMissionByIndex(0);
    }

    public void StartMissionByIndex(int missionIndex)
    {
        // 유효한지부터 검사해
        if (missionIndex < 0 || missionIndex >= stageMission.Count) return;

        _currentMissionIndex = missionIndex;
        MissionManager.Instance.StartMission(stageMission[missionIndex]);
    }

    public void OnMissionCompleted()
    {
        _currentMissionIndex++;
        if (_currentMissionIndex < stageMission.Count)
        {
            StartMissionByIndex(_currentMissionIndex);
        }
        else
        {
            // 스테이지 클리어
            Debug.Log("다깼는데 이제뭐함");
        }
    }
}
