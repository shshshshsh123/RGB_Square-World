using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List 사용

[RequireComponent(typeof(Collider))] // 충돌 감지를 위해 Collider 필요
[RequireComponent(typeof(Rigidbody))] // 이동을 위해 Rigidbody 필요
public class RangedProjectile : MonoBehaviour
{
    // --- 초기화 시 PlayerAttack에서 받아올 데이터 ---
    private float _damage;
    private float _speed;
    private int _penetrationCount;
    private PoolType _poolTag;
    private float _lifeTime;

    // --- 내부 컴포넌트 참조 ---
    private Rigidbody _rb;
    private Collider _collider;
    private List<Collider> _hitEnemies; // 이미 맞은 적들을 기록 (다단 히트 방지)

    // --- 기타 설정 ---
    [SerializeField, Tooltip("충돌 이펙트 (선택 사항, 풀링 대상으로 사용)")]
    private GameObject _hitEffectPrefab; // 충돌 시 발생할 이펙트 프리팹 (ObjectPooler에 등록해야 함)
    [SerializeField, Tooltip("충돌 이펙트가 생성될 때의 콜라이더 표면으로부터의 오프셋")]
    private float _hitEffectOffset = 0.1f;

    private Coroutine _lifeTimeCoroutine; // 수명 타이머 코루틴 참조

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _hitEnemies = new List<Collider>();

        // Rigidbody 초기 설정
        if (_rb != null)
        {
            _rb.isKinematic = false; // 물리 엔진으로 이동
            _rb.useGravity = false;  // 중력 영향 받지 않음
            _rb.constraints = RigidbodyConstraints.FreezeRotation; // 회전 고정
        }

        // Collider 초기 설정
        if (_collider != null)
        {
            _collider.isTrigger = true; // Trigger 모드로 설정 (관통 및 감지용)
        }
    }

    /// <summary>
    /// PlayerAttack에서 호출하여 투사체의 초기값을 설정하고 발사를 시작합니다.
    /// </summary>
    public void Initialize(WeaponData weaponData, LevelData levelData, PoolType poolTag)
    {
        // 데이터 할당
        _damage = levelData.damage;
        // WeaponData에 projectileSpeed가 있으면 사용, 없으면 기본값 (예: 15f)
        _speed = (levelData.projectileSpeed > 0) ? levelData.projectileSpeed : 15f;
        _penetrationCount = levelData.penetrationCount;
        _poolTag = poolTag;
        _lifeTime = weaponData.lifeTime;

        if (_collider != null) _collider.enabled = true; // 콜라이더 활성화

        // 이동 시작
        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.linearVelocity = transform.forward * _speed;
        }
        else
        {
            Debug.LogWarning($"[RangedProjectile] Rigidbody가 없어 투사체가 이동하지 않을 수 있습니다. {gameObject.name}");
        }

        // 기존 코루틴이 있다면 중지하고 새로 시작
        if (_lifeTimeCoroutine != null)
        {
            StopCoroutine(_lifeTimeCoroutine);
        }
        if (_lifeTime > 0) // 수명이 0보다 크면 타이머 시작
        {
            _lifeTimeCoroutine = StartCoroutine(AutoReturnToPool(_lifeTime));
        }
    }

    // 오브젝트 풀에서 재활용되어 활성화될 때마다 호출
    private void OnEnable()
    {
        _hitEnemies.Clear(); // 맞은 적 리스트 초기화
    }


    private void OnTriggerEnter(Collider other)
    {
        // 이미 비활성화된 투사체는 추가 처리하지 않음
        if (!gameObject.activeSelf) return;

        // 플레이어 자신 또는 다른 발사체와의 충돌 무시
        if (other.CompareTag("Player") || other.CompareTag("PlayerAttack") || other.CompareTag("Ground")) return;

        // 적과 충돌 시
        if (other.CompareTag("Enemy"))
        {
            // 이미 맞은 적인지 확인 (다단 히트 방지)
            if (!_hitEnemies.Contains(other))
            {
                _hitEnemies.Add(other); // 맞은 적 리스트에 추가

                // 데미지 주기
                other.GetComponent<MonsterStatus>()?.TakeDamage(_damage);
                // Debug.Log($"{other.name}에게 {_damage} 데미지! (남은 관통: {_penetrationCount - 1})");

                _penetrationCount--; // 관통 횟수 감소

                // 충돌 이펙트 발생 (ObjectPooler 사용)
                SpawnHitEffect(other);

                // 관통 횟수를 다 썼으면 풀에 반납
                if (_penetrationCount <= 0)
                {
                    ReturnToPool();
                    return; // 반납 후 추가 로직 방지
                }
            }
        }

        else
        {
            SpawnHitEffect(other);
            ReturnToPool(); // 적이 아닌 다른 것과 충돌 시 즉시 반납
            return;
        }
    }

    /// <summary>
    /// 충돌 지점에 히트 이펙트를 생성합니다.
    /// </summary>
    private void SpawnHitEffect(Collider other)
    {
        if (_hitEffectPrefab != null)
        {
            // 충돌 지점 계산 (ClosestPoint 대신 Bounds.center와 방향 벡터 사용)
            // OnTriggerEnter에서는 정확한 ContactPoint를 얻기 어려우므로 근사치 사용
            Vector3 collisionPoint = other.ClosestPoint(transform.position); // 여전히 ClosestPoint 사용
            // 만약 ClosestPoint에서 오류가 나면 아래 코드로 대체
            // Vector3 collisionPoint = other.bounds.center; 

            Vector3 directionToOther = (other.transform.position - transform.position).normalized;
            Quaternion rot = Quaternion.LookRotation(-directionToOther); // 투사체 반대 방향 (충돌면 법선 방향에 가깝게)

            Vector3 pos = collisionPoint + (directionToOther * _hitEffectOffset); // 콜라이더 표면에서 살짝 띄워서 생성

            GameObject arrow = ObjectPooler.Instance.SpawnFromPool(PoolType.hitEffect, pos, rot);
        }
    }


    /// <summary>
    /// 지정된 시간 후 자동으로 풀에 반납하는 코루틴
    /// </summary>
    private IEnumerator AutoReturnToPool(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    /// <summary>
    /// 오브젝트를 풀에 반납합니다.
    /// </summary>
    private void ReturnToPool()
    {
        // 이미 비활성화된 상태라면 중복 처리 방지
        if (!gameObject.activeSelf) return;

        // 자동 반납 코루틴 중지
        if (_lifeTimeCoroutine != null)
        {
            StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = null;
        }

        // --- 오브젝트 상태 초기화 (멈춤) ---
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeAll; // 모든 이동/회전 고정
        }
        if (_collider != null) _collider.enabled = false; // 콜라이더 비활성화 (더 이상 충돌하지 않음)

        // 이펙트 정지 (이 투사체 자체에 파티클이 있다면)
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 오브젝트 풀러에 반납
        ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
    }
}