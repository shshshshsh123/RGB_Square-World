using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    [Header("# Player Info")]
    [SerializeField] private float _playerMaxHp = 100f;
    [SerializeField] private float _playerCurrentHp;

    public static event Action<float, float> OnPlayerHpChanged;

    void Start()
    {
        
    }

    public void PlayerTakeDamage(float damage, GameObject damageSource)
    {
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
        _playerCurrentHp = Mathf.Clamp(_playerCurrentHp, 0, _playerMaxHp);
        OnPlayerHpChanged?.Invoke(_playerCurrentHp, _playerMaxHp);
    }
}
