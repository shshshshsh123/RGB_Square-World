using MoreMountains.Feedbacks;
using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("# Player Info")]
    [SerializeField] private float _playerMaxHp = 100f;
    [SerializeField] private float _playerCurrentHp;
    [SerializeField] private int _currentChargeAttack = 0;
    [SerializeField] private int _maxChargeAttack = 100;
    [SerializeField] private float _invincibilityDuration = 0.3f;
    public bool CanChargeAttack => _currentChargeAttack == 100;
    bool isInvincibility = false;

    [Header("# MMF Player")]
    public MMF_Player mmfPlayer_Damaged;

    public static event Action<float, float> OnPlayerHpChanged;
    public static event Action<float> OnPlayerMaxHpChanged; // 새로운 최대 체력 변경 이벤트
    public static event Action<int, int> OnPlayerChargeChanged;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        initPlayerInfo();
        _playerCurrentHp = _playerMaxHp;
    }

    void initPlayerInfo()
    {
        _playerMaxHp = 100f;
        _playerCurrentHp = _playerMaxHp;
        _maxChargeAttack = 100;
        _currentChargeAttack = 20;
        UpdateAllPlayerUIs();
    }

    void UpdateAllPlayerUIs()
    {
        OnPlayerMaxHpChanged?.Invoke(_playerMaxHp);
        OnPlayerHpChanged?.Invoke(_playerCurrentHp, _playerMaxHp);
        OnPlayerChargeChanged?.Invoke(_currentChargeAttack, _maxChargeAttack);
    }

    public void PlayerTakeDamage(float damage, GameObject damageSource)
    {
        // 무적 체크
        if (isInvincibility) return;

        //// 데미지 소스가 유효한지 확인
        //if (damageSource == null)
        //{
        //    Debug.Log("데미지 소스가 불분명하여 데미지가 적용되지 않았습니다.");
        //    return;
        //}

        //// 데미지 소스가 정말 적인지 확인
        //// 몬스터만 가지고 있는 'Enemy' 태그나 'EnemyAI' 스크립트가 있는지 확인
        //if (!damageSource.CompareTag("Enemy"))
        //{
        //    Debug.Log($"{damageSource.name}은(는) 유효한 공격 주체가 아닙니다.");
        //    return;
        //}

        //// 데미지 유효성 검증
        //// 한 번의 공격에 비정상적으로 큰 데미지가 들어오는지 확인
        //if (damage > 1000f) // 예: 최대 데미지 한계 설정
        //{
        //    Debug.Log("비정상적인 데미지 수치가 감지되었습니다.");
        //    return;
        //}

        // 모든 검증을 통과했을 때만 실제 데미지 적용
        _playerCurrentHp -= damage;
        _playerCurrentHp = Mathf.Clamp(_playerCurrentHp, 0, _playerMaxHp);  // 오류방지
        OnPlayerHpChanged?.Invoke(_playerCurrentHp, _playerMaxHp);
        if (_playerCurrentHp <= 0f)
        {
            PlayerDeath();
        }
        // 피격무적
        StartCoroutine(InvincibilityCoroutine(_invincibilityDuration));

        // 피격 피드백 재생
        mmfPlayer_Damaged?.PlayFeedbacks();
    }

    IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincibility = true;
        yield return new WaitForSeconds(duration);
        isInvincibility = false;
    }

    void PlayerDeath()
    {
        GameUIManager.Instance.ShowGameOverUI("플레이어 쓰러짐");
    }

    public void IncreaseMaxHp(float amount)
    {
        _playerMaxHp = amount;
        // 최대 체력이 변경되었음을 모든 구독자에게 알리기
        OnPlayerMaxHpChanged?.Invoke(_playerMaxHp);
        OnPlayerHpChanged?.Invoke(_playerCurrentHp, _playerMaxHp);
    }

    public void IncreaseChargeAttack(int amount)
    {
        _currentChargeAttack += amount;
        if (_currentChargeAttack > _maxChargeAttack) _currentChargeAttack = _maxChargeAttack;

        OnPlayerChargeChanged?.Invoke(_currentChargeAttack, _maxChargeAttack);
    }
}
