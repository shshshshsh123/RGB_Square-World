using UnityEngine;
using System.Collections; // IEnumerator를 위해 필요
using System.Collections.Generic; // List를 위해 필요

public class MagicProjectile : MonoBehaviour
{
    private IAttackOwner _owner; // 공격 주체를 통해 HitStop 등을 통지
    private float _damage;
    private float _fallDuration; // 운석이 떨어지는 총 시간
    private float _scale;
    private PoolType _selfPoolTag; // 이 오브젝트 자신을 풀에 반납할 때 사용할 태그
    private PoolType _effectPoolTag; // 착지 이펙트 풀 태그

    private Vector3 _targetGroundPosition; // 운석이 떨어질 지면 위치
    private Vector3 _startFallPosition; // 운석이 떨어지기 시작할 공중 위치

    public AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float initialHeight = 10f; // 마우스 위치에서 얼마나 높은 곳에서 생성될지

    private bool _hasDealtDamage = false; // 한 번만 데미지 처리 (범위 공격이므로 중요)
    public float explosionRadius = 3f; // 착지 시 데미지 범위
    public LayerMask enemyLayer; // 적 레이어 마스크 (Inspector에서 설정)

    private void Awake()
    {
        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
    }

    private void OnEnable()
    {
        _hasDealtDamage = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// 마법 발사체 초기화 (RangedProjectile과 유사한 시그니처)
    /// </summary>
    public void Initialize(IAttackOwner owner, float damage, float fallDuration, float scale, PoolType selfPoolTag, PoolType effectPoolTag)
    {
        _owner = owner;
        _damage = damage;
        _fallDuration = fallDuration;
        _scale = scale;
        _selfPoolTag = selfPoolTag;
        _effectPoolTag = effectPoolTag;

        transform.localScale = Vector3.one * _scale;
        _hasDealtDamage = false;

        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
        }
    }

    /// <summary>
    /// 운석이 떨어질 지면 타겟 위치 설정 및 낙하 시작
    /// </summary>
    public void StartFall(Vector3 targetGroundPosition)
    {
        _targetGroundPosition = targetGroundPosition;
        _startFallPosition = new Vector3(targetGroundPosition.x, targetGroundPosition.y + initialHeight, targetGroundPosition.z);
        transform.position = _startFallPosition;

        StartCoroutine(FallDown());
    }

    private IEnumerator FallDown()
    {
        float timer = 0f;
        while (timer < _fallDuration)
        {
            float progress = timer / _fallDuration;
            float easedProgress = fallCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(_startFallPosition, _targetGroundPosition, easedProgress);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = _targetGroundPosition;
        OnImpact(); // 데미지
        ReturnToPool(); // 풀에 반납
    }

    /// <summary>
    /// 착지 시 처리 (데미지, 이펙트 등)
    /// </summary>
    private void OnImpact()
    {
        if (_hasDealtDamage) return;
        _hasDealtDamage = true;

        Collider[] hitColliders = Physics.OverlapSphere(_targetGroundPosition, explosionRadius, enemyLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            MonsterStatus monsterStatus = hitCollider.GetComponent<MonsterStatus>();
            if (monsterStatus != null)
            {
                monsterStatus.TakeDamage(_damage);
                _owner?.NotifyHit();
            }
        }
        // 착지 이펙트 재생
        GameObject effet = ObjectPooler.Instance.SpawnFromPool(_effectPoolTag, _targetGroundPosition, Quaternion.identity);
        effet.transform.localScale = Vector3.one * _scale;
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;

        StopAllCoroutines();
        ObjectPooler.Instance.ReturnToPool(_selfPoolTag, gameObject);
    }
}