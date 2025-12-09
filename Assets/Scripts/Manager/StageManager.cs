using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("# 이번 스테이지에 있을 미션 목록")]
    public List<MissionData> stageMission;

    [Header("# UI")]
    [Tooltip("미션 클리어시 나타날 패널")] public GameObject missionClearPanel;
    [Tooltip("다음 미션 시작 전 나타날 패널")] public GameObject nextMissionPanel;
    [Tooltip("다음 미션 제목 텍스트")] public TMP_Text nextMissionTitleText;
    [Tooltip("각 UI가 떠있는 시간(초)")] public float uiDisplayDuration = 2.0f;

    private int _currentMissionIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // UI 초기화 (시작하자마자 꺼둠)
        if (missionClearPanel != null) missionClearPanel.SetActive(false);
        if (nextMissionPanel != null) nextMissionPanel.SetActive(false);

        // 스테이지 시작 시 첫 번째 미션 시퀀스 시작
        if (stageMission.Count > 0)
        {
            StartCoroutine(StartFirstMissionRoutine());
        }
    }

    private IEnumerator StartFirstMissionRoutine()
    {
        // 0. 첫 미션 데이터 가져오기
        MissionData firstData = stageMission[0];

        // 1. 시작 UI 표시 및 대기
        if (nextMissionPanel != null)
        {
            nextMissionPanel.SetActive(true);
            if (nextMissionTitleText != null)
            {
                nextMissionTitleText.text = firstData.title; // 첫 미션 제목 설정
            }
        }
        yield return new WaitForSeconds(uiDisplayDuration); // 대기

        if (nextMissionPanel != null)
        {
            nextMissionPanel.SetActive(false);
        }

        // 2. 실제 첫 미션 시작
        StartMissionByIndex(0);
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
        StartCoroutine(ProcessMissionTransition());
    }

    // 미션 완료 -> 다음 미션 시작 사이의 연출을 담당하는 코루틴
    private IEnumerator ProcessMissionTransition()
    {
        // 1. 미션 클리어 UI 표시 및 대기
        if (missionClearPanel != null)
        {
            missionClearPanel.SetActive(true);
        }
        yield return new WaitForSeconds(uiDisplayDuration); // 지정된 시간만큼 대기

        if (missionClearPanel != null)
        {
            missionClearPanel.SetActive(false);
        }

        // 2. 다음 인덱스 계산
        _currentMissionIndex++;

        // 3. 다음 미션이 있는지 확인
        if (_currentMissionIndex < stageMission.Count)
        {
            // 3-A. 다음 미션이 있다면: 시작 UI 표시 및 대기
            if (nextMissionPanel != null)
            {
                nextMissionPanel.SetActive(true);
                MissionData nextData = stageMission[_currentMissionIndex];
                nextMissionTitleText.text = nextData.title;
            }
            yield return new WaitForSeconds(uiDisplayDuration); // 지정된 시간만큼 대기

            if (nextMissionPanel != null)
            {
                nextMissionPanel.SetActive(false);
            }

            // 3-B. 실제 다음 미션 시작
            StartMissionByIndex(_currentMissionIndex);
        }
        else
        {
            // 4. 모든 미션 완료 (스테이지 클리어)
        }
    }
}
