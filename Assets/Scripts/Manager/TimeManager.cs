using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Dictionary의 Key를 찾는 데 사용

public class TimeManager : Singleton<TimeManager>
{
    // --- 내부 상태 변수 ---
    // 현재 시간이 완전히 멈췄는지 여부 (Time.timeScale == 0)
    private bool _isStopped = false;
    public bool IsStopped => _isStopped;

    // 현재 시간이 느려졌는지 여부 (Time.timeScale이 0보다 크고 1보다 작을 때)
    private bool _isSlowed = false;
    public bool IsSlowed => _isSlowed;

    // TimeScale 요청 관리 (Key: 요청자, Value: 요청값)
    private Dictionary<object, float> _activeTimeScaleRequests = new Dictionary<object, float>();

    // 중복 코루틴 방지용 딕셔너리
    private Dictionary<object, Coroutine> _activeCoroutines = new Dictionary<object, Coroutine>();

    // Time.timeScale이 0.0f로 설정되었을 때 이 상태를 요청한 주체를 저장
    // 이 주체가 시간을 1.0f로 복원할 때까지 다른 슬로우 효과가 덮어씌워지지 않도록
    private object _stopperRequester = null;

    protected override void Awake()
    {
        base.Awake();
    }

    // --- 시간 제어 public 메서드 ---

    /// <summary>
    /// TimeManager에게 Time.timeScale을 변경해달라고 요청합니다.
    /// 가장 낮은 timeScale이 우선적으로 적용됩니다.
    /// </summary>
    /// <param name="requester">시간 변경을 요청하는 객체 (this를 전달)</param>
    /// <param name="targetTimeScale">적용하고자 하는 Time.timeScale 값 (0.0f: 정지, 0.1~0.9f: 슬로우, 1.0f: 정상)</param>
    /// <param name="duration">TimeScale을 유지할 시간 (초). 0f이면 수동으로 RestoreTimeScale을 호출해야 합니다.</param>
    public void RequestTimeScale(object requester, float targetTimeScale, float duration = 0f)
    {
        // Debug.Log($"[타임매니저] 시간정지요청: {requester.GetType().Name} 가 {Time.time}에 배율 {targetTimeScale}로 {duration}초");

        // 1. 요청 주체와 targetTimeScale 기록
        _activeTimeScaleRequests[requester] = targetTimeScale;

        // 2. _isStopped 상태 관리
        if (targetTimeScale == 0.0f)
        {
            _isStopped = true;
            _stopperRequester = requester; // 시간을 멈춘 주체를 기록
        }
        else if (requester == _stopperRequester && targetTimeScale > 0.0f)
        {
            // 시간을 멈췄던 주체가 0.0f가 아닌 다른 값을 요청하면,
            // 더 이상 시간을 멈춘 주체가 아니라고 간주하고 초기화
            _stopperRequester = null;
        }

        // 3. 현재 적용되어야 할 Time.timeScale 계산 및 적용
        ApplyHighestPriorityTimeScale();

        // 긴급투입: 기존 코루틴이 존재하면 취소하기!
        if (_activeCoroutines.TryGetValue(requester, out var oldCoroutine))
        {
            StopCoroutine(oldCoroutine);
        }

        // 4. Duration이 있으면 일정 시간 후 복원 코루틴 시작
        if (duration > 0f)
        {
            StartCoroutine(RestoreTimeScaleAfterDelay(requester, duration));
        }
    }

    /// <summary>
    /// TimeManager에게 특정 주체의 Time.timeScale 변경 요청을 취소하라고 알립니다.
    /// </summary>
    /// <param name="requester">이전에 시간 변경을 요청했던 객체</param>
    public void RestoreTimeScale(object requester)
    {
        // Debug.Log($"[타임매니저] 시간요청취소: {requester.GetType().Name} 가 {Time.time}에");

        // 요청 목록에서 제거
        if (_activeTimeScaleRequests.ContainsKey(requester))
        {
            _activeTimeScaleRequests.Remove(requester);
        }

        // 1. 실행 중인 복원 코루틴이 있다면 정지 및 제거
        if (_activeCoroutines.TryGetValue(requester, out var runningCoroutine))
        {
            if (runningCoroutine != null) StopCoroutine(runningCoroutine);
            _activeCoroutines.Remove(requester);
        }

        // Stopper 해제 체크
        if (requester == _stopperRequester)
        {
            _stopperRequester = null;
        }

        // 다시 최우선순위 TimeScale 적용
        ApplyHighestPriorityTimeScale();
    }

    // --- 내부 로직 ---

    /// <summary>
    /// 현재 활성화된 모든 TimeScale 요청 중 가장 낮은 값을 찾아 적용합니다.
    /// (0.0f 요청이 있다면 항상 최우선)
    /// </summary>
    private void ApplyHighestPriorityTimeScale()
    {
        float finalTimeScale = 1.0f; // 기본값 (아무도 요청 없으면 정상)

        // 1. 누가 시간을 0.0f로 멈춰달라고 요청했는지 확인
        if (_stopperRequester != null && _activeTimeScaleRequests.ContainsKey(_stopperRequester) && _activeTimeScaleRequests[_stopperRequester] == 0.0f)
        {
            finalTimeScale = 0.0f; // 시간을 멈춘 주체가 있다면 0.0f가 최우선
        }
        else
        {
            // 2. 0.0f 요청이 없으면, 0.0f보다 큰 요청들 중 가장 낮은 값을 찾음 (슬로우 효과)
            if (_activeTimeScaleRequests.Any())
            {
                // 0.0f를 제외한 값들 중 가장 낮은 TimeScale을 찾습니다.
                finalTimeScale = _activeTimeScaleRequests.Values
                                     .Where(scale => scale > 0.0f) // 0.0f 요청은 위에서 처리했으므로 제외
                                     .DefaultIfEmpty(1.0f)         // 요청이 없으면 1.0f 반환
                                     .Min();                        // 가장 낮은 값 선택
            }
        }

        // 3. _isStopped 및 _isSlowed 플래그 업데이트
        _isStopped = (finalTimeScale == 0.0f);
        _isSlowed = (finalTimeScale > 0.0f && finalTimeScale < 1.0f);


        // 4. 최종 Time.timeScale 적용
        Time.timeScale = finalTimeScale;
        // Debug.Log($"[타임매니저] 타임스케일변경완료: {Time.timeScale}, 정지: {_isStopped}, 슬로우: {_isSlowed}");
    }

    /// <summary>
    /// 지정된 시간(실제 시간)이 지나면 특정 주체의 TimeScale 요청을 자동으로 취소합니다.
    /// </summary>
    private IEnumerator RestoreTimeScaleAfterDelay(object requester, float delay)
    {
        // 시간을 멈췄을 때도 제대로 작동하도록 WaitForSecondsRealtime 사용
        yield return new WaitForSecondsRealtime(delay);

        // 요청자가 여전히 활성화되어 있고, 그 요청이 0.0f가 아닐 경우에만 복원 요청
        // 0.0f는 수동으로 복원되어야 함 (차지 공격처럼 명확한 시작/끝이 있는 경우)
        if (_activeTimeScaleRequests.ContainsKey(requester) && _activeTimeScaleRequests[requester] != 0.0f)
        {
            RestoreTimeScale(requester);
        }
    }
}