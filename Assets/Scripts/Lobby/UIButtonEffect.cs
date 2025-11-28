using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("타겟 설정")]
    [SerializeField] private Image _targetImage;
    [SerializeField] private TMP_Text _targetText;

    [Header("효과 설정 (Alpha)")]
    [Range(0f, 1f)][SerializeField] private float _hoverAlpha = 1.0f;    // 마우스 올렸을 때 알파값
    [Range(0f, 1f)][SerializeField] private float _normalAlpha = 0.7f;   // 평소 알파값

    [Header("효과 설정 (Scale)")]
    [SerializeField] private float _hoverScale = 1.1f;    // 마우스 올렸을 때 텍스트 스케일 배율
    [SerializeField] private float _normalScale = 1.0f;   // 평소 텍스트 스케일 배율

    [Header("애니메이션 설정")]
    [SerializeField] private float _duration = 0.2f;      // 변화하는 데 걸리는 시간 (초)

    private Vector3 _initialTextScale; // 텍스트의 초기 스케일 저장용
    private Coroutine _currentCoroutine; // 현재 실행 중인 코루틴 관리용

    private void Start()
    {
        if (_targetImage == null) _targetImage = GetComponent<Image>();
        if (_targetText == null) _targetText = GetComponentInChildren<TMP_Text>();

        if (_targetText != null)
        {
            _initialTextScale = _targetText.transform.localScale;
        }

        // 시작 시 즉시 평소 상태로 초기화
        SetStateImmediate(false);
    }

    // 마우스 들어옴 -> 호버 상태로 부드럽게 전환
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartTransition(true);
    }

    // 마우스 나감 -> 평소 상태로 부드럽게 전환
    public void OnPointerExit(PointerEventData eventData)
    {
        StartTransition(false);
    }

    // 오브젝트가 비활성화될 때 (예: 팝업 닫힘) -> 즉시 평소 상태로 리셋하고 코루틴 정지
    private void OnDisable()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
        SetStateImmediate(false);
    }

    // --- 내부 로직 ---

    // 전환 코루틴 시작 관리자
    private void StartTransition(bool isHover)
    {
        // 이미 실행 중인 전환 코루틴이 있다면 중지 (중복 실행 방지)
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }
        // 새로운 전환 코루틴 시작
        _currentCoroutine = StartCoroutine(TransitionRoutine(isHover));
    }

    // 실제 부드러운 전환을 담당하는 코루틴
    private IEnumerator TransitionRoutine(bool isHover)
    {
        // 1. 목표 값 설정
        float targetAlpha = isHover ? _hoverAlpha : _normalAlpha;
        Vector3 targetScaleVector = isHover ? _initialTextScale * _hoverScale : _initialTextScale * _normalScale;

        // 2. 시작 값 설정 (현재 상태에서 시작해야 부드러움)
        float startAlpha = _targetImage != null ? _targetImage.color.a : targetAlpha;
        Vector3 startScaleVector = _targetText != null ? _targetText.transform.localScale : targetScaleVector;

        float elapsed = 0f;

        // 지정된 시간(_duration) 동안 루프 실행
        while (elapsed < _duration)
        {
            // UI는 게임 일시정지(TimeScale=0) 상태에서도 동작해야 하므로 unscaledDeltaTime 사용
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _duration); // 0~1 사이 진행률 계산

            // 알파값 부드럽게 변경 (Lerp)
            if (_targetImage != null)
            {
                Color c = _targetImage.color;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                _targetImage.color = c;
            }

            // 스케일 부드럽게 변경 (Lerp)
            if (_targetText != null)
            {
                _targetText.transform.localScale = Vector3.Lerp(startScaleVector, targetScaleVector, t);
            }

            yield return null; // 다음 프레임까지 대기
        }

        // 루프 종료 후 최종 목표 값으로 확실하게 설정 (미세한 오차 방지)
        SetStateImmediate(isHover);
        _currentCoroutine = null;
    }

    // 상태를 즉시 설정하는 헬퍼 함수 (Start나 루프 종료 후 사용)
    private void SetStateImmediate(bool isHover)
    {
        if (_targetImage != null)
        {
            Color c = _targetImage.color;
            c.a = isHover ? _hoverAlpha : _normalAlpha;
            _targetImage.color = c;
        }

        if (_targetText != null)
        {
            _targetText.transform.localScale = isHover ? _initialTextScale * _hoverScale : _initialTextScale * _normalScale;
        }
    }
}