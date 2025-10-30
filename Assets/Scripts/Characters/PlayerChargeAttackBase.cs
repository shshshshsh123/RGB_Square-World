using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public abstract class PlayerChargeAttackBase : MonoBehaviour
{
    [Header("# 차지공격 공통 설정")]
    public float chargeAttackTimeRequire = 1.0f; // 몇초 눌러야지 발동??
    public int chargeAttackTarget = 3; // 몇명 때리나요? (강화하면 올라감)
    public float chargeAttackRange = 10.0f; // 범위
    public Image chargeAttackKeyDownImage; // 차지공격 키 누르고 있는 동안 채워지는 이미지 (UI)

    protected bool _isCharging = false;
    protected float _chargeTimer = 0.0f;

    protected virtual void Start()
    {
        if (chargeAttackKeyDownImage != null)
        {
            chargeAttackKeyDownImage.fillAmount = 0f;
        }
    }

    protected virtual void Update() // virtual로 변경
    {
        // 차지공격 입력 처리
        HandleChargeAttackInput();

        // 테스트용 - 차지공격 게이지 채우기
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            GameManager.Instance.IncreaseChargeAttack(50);
        }
    }

    /// <summary>
    /// 차지공격 입력을 감지하고 타이머를 관리합니다.
    /// </summary>
    protected virtual void HandleChargeAttackInput() // virtual로 변경
    {
        // 게이지 다찼나?
        if (!GameManager.Instance.CanChargeAttack) return;

        // 차지 공격 키 처음 누르면 타이머 시작
        if (Input.GetMouseButtonDown(1))
        {
            _isCharging = true;
            _chargeTimer = 0.0f;
        }

        // 차지 공격 키를 누르고 있는 동안
        if (Input.GetMouseButton(1) && _isCharging)
        {
            _chargeTimer += Time.unscaledDeltaTime;
            if (chargeAttackKeyDownImage != null)
            {
                chargeAttackKeyDownImage.fillAmount = Mathf.Clamp01(_chargeTimer / chargeAttackTimeRequire);
            }

            if (_isCharging && _chargeTimer >= chargeAttackTimeRequire)
            {
                // 차지 성공! 실제 공격 실행 (자식 클래스에서 구현됨)
                StartCoroutine(PerformChargeAttack());
                _isCharging = false;
                _chargeTimer = 0f;
                // 게이지 초기화는 PerformChargeAttack 끝에서 처리
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            // 차지 실패 또는 취소
            _isCharging = false;
            _chargeTimer = 0f;
            if (chargeAttackKeyDownImage != null)
            {
                chargeAttackKeyDownImage.fillAmount = 0f;
            }
        }
    }

    /// <summary>
    /// 실제 차지 공격 로직을 구현하는 추상 코루틴 (자식 클래스에서 반드시 구현해야 함)
    /// </summary>
    protected abstract IEnumerator PerformChargeAttack();

    /// <summary>
    /// 범위 내의 적을 찾아 리스트로 반환하는 헬퍼 함수
    /// </summary>
    protected List<Transform> FindTargets()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, chargeAttackRange, LayerMask.GetMask("Enemy"));
        return enemiesInRange
            .OrderBy(enemy => Random.value) // 랜덤 정렬
            .Take(chargeAttackTarget)
            .Select(enemy => enemy.transform)
            .ToList();
    }

    /// <summary>
    /// 최종 공격 대상 리스트를 생성하는 헬퍼 함수
    /// </summary>
    protected List<Transform> GetFinalTargets(List<Transform> foundTargets)
    {
        if (foundTargets == null || foundTargets.Count == 0)
        {
            return new List<Transform>(); // 빈 리스트 반환
        }

        List<Transform> finalTargets = new List<Transform>();
        for (int i = 0; i < chargeAttackTarget; i++)
        {
            finalTargets.Add(foundTargets[i % foundTargets.Count]);
        }
        return finalTargets;
    }
}