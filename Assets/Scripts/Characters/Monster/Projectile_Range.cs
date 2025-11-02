using UnityEngine;
using System.Collections;

public class Projectile_Range : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private PoolType projectilePoolType;

    private Rigidbody _rigidbody;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // lifetime 후에 풀에 반납하는 코루틴
        StartCoroutine(ReturnAfterLifetime());
    }

    void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    /// <summary>
    /// 오브젝트가 풀로 돌아갈 때 호출
    /// </summary>
    void OnDisable()
    {
        // 코루틴이 중복 실행되지 않도록 정지
        StopAllCoroutines();
    }

    /// <summary>
    /// 3초 뒤에 풀로 반납하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(_lifetime);
        ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.PlayerTakeDamage(_damage, gameObject);

            ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
        }
        else
        {
            ObjectPooler.Instance.ReturnToPool(projectilePoolType, gameObject);
        }
    }
}