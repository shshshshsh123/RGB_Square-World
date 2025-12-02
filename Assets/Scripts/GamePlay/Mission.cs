using UnityEngine;

[System.Serializable]
public class Mission
{
    public MissionData Data { get; private set; }

    // 현재 상황
    public int currentProgress;
    public float currentTime;
    public bool isCompleted;
    public bool isFailed;

    // 생성자
    public Mission(MissionData data)
    {
        this.Data = data;
        this.currentProgress = 0;
        this.currentTime = 0;
        this.isCompleted = false;
        this.isFailed = false;
    }

    // 진행도 업데이트 (외부호출)
    public void AddProgress(int amount)
    {
        currentProgress += amount;
        CheckCompletion();
    }

    // UI업데이트용
    public string GetProgressText()
    {
        if (Data.missionType == MissionType.Survival)
        {
            float timeLeft = Data.timeLimit - currentTime;
            return $"남은 시간: {timeLeft:F1}s / {Data.timeLimit}s";
        }
        else if (Data.missionType == MissionType.Defense)
        {
            if (currentProgress == -2)
                return "도착!";
            else
                return "이동중...";
        }
        else
        {
            return $"처치 수: {currentProgress} / {Data.targetCount}";
        }
    }

    // 시간업데이트
    public void UpdateTime(float deltaTime)
    {
        if (isCompleted || isFailed) return;

        // 서바이벌(시간제한) 미션일 경우에만 체크
        if (Data.missionType != MissionType.Survival) return;

        currentTime += deltaTime;
        if (currentTime >= Data.timeLimit) Complete();
    }

    private void CheckCompletion()
    {
        if (Data.missionType == MissionType.Combat || Data.missionType == MissionType.Exploration)
        {
            if (currentProgress >= Data.targetCount)
            {
                Complete();
            }
        }
        if (Data.missionType == MissionType.Defense)
        {
            if (currentProgress == -2)
            {
                Complete();
            }
        }
    }

    private void Complete()
    {
        isCompleted = true;
        // TODO: 미션완료 하면뭐함??
        StageManager.Instance.OnMissionCompleted();
    }

    public void Fail(string text)
    {
        isFailed = true;
        GameUIManager.Instance.ShowGameOverUI(text);
    }
}
