using UnityEngine;
using System.Collections;

public class Projectile_Range : MonoBehaviour
{
    [Header("발사체 설정")]

    [Tooltip("발사체 속도")]
    [SerializeField] private float _speed = 10f;

    [Tooltip("발사체 데미지")]
    [SerializeField] private float _damage = 10f;

    [Tooltip("발사체 최대 생존 시간")]
    [SerializeField] private float _lifeTime = 3f;

    [Tooltip("회전 축")]
    [SerializeField] private Vector3 _rotationAxis = new Vector3(1, 0, 0);

    [Tooltip("회전 각도(1초)")]
    [SerializeField] private float _rotationSpeed = 720f;

    [SerializeField] private PoolType projectilePoolType;

    private Rigidbody _rigidBody;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();

        if (_rigidBody != null)
        {
            _rigidBody.isKinematic = false;
            _rigidBody.useGravity = false;
        }
    }

    /// <summary>
    /// 오브젝트가 비활성화되었다가 활성화될 때마다 호출
    /// </summary>
    void OnEnable()
    {
        // 발사체 속도 설정
        _rigidBody.linearVelocity = transform.forward * _speed;

        // 발사체 회전 속도 설정
        _rigidBody.angularVelocity = transform.TransformDirection(_rotationAxis.normalized) * _rotationSpeed * Mathf.Deg2Rad;

        StartCoroutine(ReturnAfterLifetime());
    }

    /// <summary>
    /// 오브젝트가 비활성화될 때 호출
    /// </summary>
    void OnDisable()
    {
        StopAllCoroutines();

        // 발사체 속도와 회전 속도를 0으로 설정
        if (_rigidBody != null)
        {
            _rigidBody.linearVelocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// lifeTime(3초)후 ObjectPool에 반납
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(_lifeTime);
        ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌 시 데미지를 주고 풀에 반납
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerTakeDamage(_damage, gameObject);

            ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
        }
        // 그 외 충돌 시 그냥 풀에 반납
        else
        {
            ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
        }
    }
}