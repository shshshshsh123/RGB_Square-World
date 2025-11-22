using UnityEngine;
using System.Collections;
using UnityEngine.AI;

// 기본 원거리 몬스터

public class Monster_Range : BaseMonster
{
    [Header("원거리 몬스터 설정")]

    [Tooltip("발사체의 발사 위치")]
    [SerializeField] private Transform _firePos;

    [Tooltip("풀 타입(오브젝트 풀러)")]
    [SerializeField] private PoolType _projectilePoolType;

    private float _fireAnimationTime = 0.5f; // 공격 애니메이션 총 재생 시간
    private float _fireDelay = 0.3f; // 애니메이션 시작 후, 실제로 발사체가 나가는 순간
    private Rigidbody _rigidbody;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void Attack()
    {
        StartCoroutine(Fire());
    }

    /// <summary>
    /// 플레이어에게 발사체(빵) 발사
    /// </summary>
    /// <returns></returns>
    private IEnumerator Fire()
    {
        // 공격 시 미끄러지지 않도록 멈춤
        _agent.isStopped = true;

        // 공격하는 동안 플레이어가 와서 부딪혀도 영향을 받지 않도록 Rigidbody 비활성화
        if (_rigidbody != null)
            _rigidbody.isKinematic = true;

        // 0.3초 대기 (발사체가 발사 순간과 던지는 애니메이션을 일치)
        yield return new WaitForSeconds(_fireDelay);

        // 발사체 생성
        if (_firePos == null)
            Debug.LogError(gameObject.name + ": 발사 위치가 설정되지 않았습니다!");

        else
        {
            GameObject projectileObject = ObjectPooler.Instance.SpawnFromPool(_projectilePoolType, _firePos.position, _firePos.rotation);
        }

        // 나머지 시간(0.2초)를 기다려서 총 애니메이션 시간(0.5초)과 맞춤
        float remainAnimationTime = _fireAnimationTime - _fireDelay;
        if (remainAnimationTime > 0)
        {
            yield return new WaitForSeconds(remainAnimationTime);
        }

        // Rigidbody 활성화
        if (_rigidbody != null)
            _rigidbody.isKinematic = false;

        // 공격 종료(부모 클래스)
        FinishAttack();
    }
}