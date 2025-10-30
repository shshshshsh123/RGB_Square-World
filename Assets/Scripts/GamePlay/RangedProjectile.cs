using UnityEngine;
using System.Collections; // Invoke 사용을 위해 필요
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
    private PoolType _hitEffectTag = PoolType.ArrowHitEffect;
    private float _hitEffectOffset = 0.1f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _hitEnemies = new List<Collider>();

        // Rigidbody 초기 설정
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Collider 초기 설정
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
    }

    /// <summary>
    /// 투사체의 초기값을 설정하고 발사를 시작합니다.
    /// </summary>
    public void Initialize(float damage, float speed, int penetrationCount, PoolType poolTag, float lifeTime, PoolType effectPoolType = PoolType.ArrowHitEffect)
    {
        // 데이터 할당
        _damage = damage;
        _speed = speed;
        _penetrationCount = penetrationCount;
        _poolTag = poolTag;
        _lifeTime = lifeTime;
        _hitEffectTag = effectPoolType;

        if (_collider != null) _collider.enabled = true; // 콜라이더 활성화

        // 이동 시작
        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotation; // 이동 전에 Constraint 재설정
            _rb.linearVelocity = transform.forward * _speed; // velocity 사용
        }
        else
        {
            Debug.LogWarning($"[RangedProjectile] Rigidbody가 없어 투사체가 이동하지 않을 수 있습니다. {gameObject.name}");
        }

        // --- Invoke로 수명 관리 ---
        CancelInvoke(nameof(ReturnToPool)); // 이전 Invoke 취소
        if (_lifeTime > 0) // 수명이 0보다 크면 타이머 시작
        {
            Invoke(nameof(ReturnToPool), _lifeTime);
        }
    }

    // 오브젝트 풀에서 재활용되어 활성화될 때마다 호출
    private void OnEnable()
    {
        _hitEnemies.Clear(); // 맞은 적 리스트 초기화

        // Rigidbody 상태 초기화 (혹시 모를 FreezeAll 상태 해제)
        if (_rb != null)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        // Collider 활성화 (Initialize에서도 하지만 안전하게)
        if (_collider != null) _collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeSelf) return; // 비활성화 중이면 무시
        if (other.CompareTag("Player") || other.CompareTag("PlayerAttack") || other.CompareTag("Ground")) return; // 충돌 무시 대상

        // 적과 충돌 시
        if (other.CompareTag("Enemy"))
        {
            if (!_hitEnemies.Contains(other)) // 다단 히트 방지
            {
                _hitEnemies.Add(other);
                other.GetComponent<MonsterStatus>()?.TakeDamage(_damage);
                SpawnHitEffect(other); // 충돌 이펙트 생성

                // 무한 관통인 경우
                if (_penetrationCount == -100)
                {
                    return;
                }
                else
                {
                    _penetrationCount--;
                }

                if (_penetrationCount <= 0) // 관통 횟수 소진
                {
                    ReturnToPool();
                    return;
                }
            }
        }
        // 적이 아닌 다른 콜라이더(벽 등)와 충돌 시
        else if (!other.isTrigger) // 트리거가 아닌 콜라이더와 충돌 시
        {
            if (_penetrationCount == 100) return; // 무한 관통이면 무시
            SpawnHitEffect(other); // 충돌 이펙트 생성
            ReturnToPool(); // 즉시 반납
            return;
        }
    }

    /// <summary>
    /// 충돌 지점에 히트 이펙트를 생성
    /// </summary>
    private void SpawnHitEffect(Collider other)
    {
        Vector3 collisionPoint = other.ClosestPoint(transform.position);
        Vector3 directionToOther = (other.transform.position - transform.position).normalized;
        Quaternion rot = Quaternion.LookRotation(-directionToOther);
        Vector3 pos = collisionPoint + (directionToOther * _hitEffectOffset);

        GameObject hitEffet = ObjectPooler.Instance.SpawnFromPool(_hitEffectTag, pos, rot);
        StartCoroutine(ReturnHitEffect(hitEffet));
    }

    IEnumerator ReturnHitEffect(GameObject hitEffect)
    {
        yield return new WaitForSeconds(0.5f);
        if (hitEffect != null)
        {
            ObjectPooler.Instance.ReturnToPool(_hitEffectTag, hitEffect);
        }
    }

    /// <summary>
    /// 오브젝트를 풀에 반납합니다.
    /// </summary>
    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return; // 중복 반납 방지

        CancelInvoke(nameof(ReturnToPool)); // 예약된 자동 반납 취소

        // 오브젝트 상태 초기화
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeAll; // 이동/회전 모두 고정
        }
        if (_collider != null) _collider.enabled = false; // 콜라이더 비활성화

        // 자체 파티클 시스템 정지 (있을 경우)
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 오브젝트 풀러에 반납
        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnToPool(_poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 오브젝트 풀러 인스턴스가 없어 풀에 반납할 수 없습니다. 대신 비활성화합니다.");
            gameObject.SetActive(false); // 풀러 없으면 그냥 비활성화
        }
    }
}