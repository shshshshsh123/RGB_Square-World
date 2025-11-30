using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [Header("# 기본 설정")]
    [Tooltip("텍스트가 위로 올라가는 속도")] public float moveSpeed = 2.0f;
    [Tooltip("텍스트가 투명해지는 속도")] public float alphaSpeed = 2.0f;
    public float lifeTime = 1.0f;
    public PoolType poolType = PoolType.DamageText;
    [Tooltip("시작 스케일 크기")] public float initalScale = 1.0f;
    [Tooltip("최종 스케일(사라질때크기)")] public float targetScale = 1.0f;
    public AnimationCurve scaleCurve;   // 크기 조절을 위한 애니메이션 커브

    private TMP_Text _text;
    private Color _originColor;
    private float _currentLifeTime;
    private Vector3 _startLocalScale;

    private void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        if ( _text != null ) _originColor = _text.color;
        _startLocalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // 초기화
        _text.color = _originColor;
        _currentLifeTime = 0.0f;
        transform.localScale = _startLocalScale * initalScale;

        // Invoke로 자동으로 풀 반납하기
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void Update()
    {
        _currentLifeTime += Time.deltaTime;
        float progress = _currentLifeTime / lifeTime;   // 0에서 1사이값이 나오겟지??

        // 데미지 이펙트 효과 (메이플 화산효과느낌)
        // 1. 위로 올라가
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 2. 스케일 조정(애니메이션커브)
        float currentScale = 1.0f;
        if (scaleCurve != null && scaleCurve.length > 0)    // 오류 방지용 if문
        {
            currentScale = Mathf.Lerp(initalScale, targetScale, scaleCurve.Evaluate(progress));
        }
        transform.localScale = _startLocalScale * currentScale;

        // 3. 점점 투명화
        Color color = _text.color;
        color.a = Mathf.Lerp(_originColor.a, 0, progress);
        _text.color = color;

        // 4. 항상 카메라 바라보게
        transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>
    /// 외부에서 데미지 수치와 속성을 설정하는 함수
    /// </summary>
    /// <param name="Damage">데미지 얼마?</param>
    /// <param name="isCritical">크리티컬 여부</param>
    public void SetDamage(float damage, bool isCritical = false)
    {
        _text.text = Mathf.RoundToInt(damage).ToString("N0");
        if (isCritical)
        {
            _text.fontSize = 2f;
            _text.color = Color.red;
            _text.outlineColor = Color.black;
        }
        else
        {
            _text.fontSize = 1.5f;
            _text.color = Color.yellow;
            _text.outlineColor = new Color(1f, 0.4f, 0f);
        }
    }

    void ReturnToPool()
    {
        ObjectPooler.Instance.ReturnToPool(poolType, gameObject);
    }
}
