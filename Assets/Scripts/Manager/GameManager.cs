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
        //// ������ �ҽ��� ��ȿ���� Ȯ��
        //if (damageSource == null)
        //{
        //    Debug.Log("������ �ҽ��� �Һи��Ͽ� �������� ������� �ʾҽ��ϴ�.");
        //    return;
        //}

        //// ������ �ҽ��� ���� ������ Ȯ��
        //// ���͸� ������ �ִ� 'Enemy' �±׳� 'EnemyAI' ��ũ��Ʈ�� �ִ��� Ȯ��
        //if (!damageSource.CompareTag("Enemy"))
        //{
        //    Debug.Log($"{damageSource.name}��(��) ��ȿ�� ���� ��ü�� �ƴմϴ�.");
        //    return;
        //}

        //// ������ ��ȿ�� ����
        //// �� ���� ���ݿ� ������������ ū �������� �������� Ȯ��
        //if (damage > 1000f) // ��: �ִ� ������ �Ѱ� ����
        //{
        //    Debug.Log("���������� ������ ��ġ�� �����Ǿ����ϴ�.");
        //    return;
        //}

        // ��� ������ ������� ���� ���� ������ ����
        _playerCurrentHp -= damage;
        _playerCurrentHp = Mathf.Clamp(_playerCurrentHp, 0, _playerMaxHp);
        OnPlayerHpChanged?.Invoke(_playerCurrentHp, _playerMaxHp);
    }
}
