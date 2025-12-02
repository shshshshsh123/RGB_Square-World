using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public PoolType type; // enum으로 관리
        public GameObject prefab;
        public int initalSize;
    }

    public List<Pool> pools;
    public Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    private Dictionary<PoolType, GameObject> prefabDictionary; // 프리팹을 빠르게 찾기 위한 딕셔너리

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        prefabDictionary = new Dictionary<PoolType, GameObject>(); // 초기화

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.initalSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.type, objectPool);
            prefabDictionary.Add(pool.type, pool.prefab); // 프리팹 참조 딕셔너리 저장
        }
    }

    public GameObject SpawnFromPool(PoolType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"[오브젝트풀러] 타입 {type} 을 찾을 수 없습니다.");
            return null;
        }

        GameObject objectToSpawn;

        if (poolDictionary[type].Count > 0)
        {
            objectToSpawn = poolDictionary[type].Dequeue();
        }
        else
        {
            // foreach 대신 딕셔너리에서 바로 프리팹을 찾아 생성 (성능 개선)
            GameObject prefabToInstantiate = prefabDictionary[type];
            objectToSpawn = Instantiate(prefabToInstantiate, position, rotation, transform);
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    public void ReturnToPool(PoolType type, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"[오브젝트풀러] 타입 {type} 을 찾을 수 없습니다.");
            Destroy(objectToReturn); // 풀에 없으면 그냥 파괴
            return;
        }

        objectToReturn.transform.position = Vector3.zero;
        objectToReturn.transform.rotation = Quaternion.identity;
        objectToReturn.SetActive(false);
        poolDictionary[type].Enqueue(objectToReturn);
    }
}

public enum PoolType
{
    Dummy,
    MeleeMonster,
    RangeMonster,
    MonsterProjectile,
    BasicSlash,
    BasicSlashHitEffect,
    ChargeSlash,
    BasicArrow,
    ArrowHitEffect,
    ChargeArrow,
    ChargeArrrowHitEffect,
    MagicProjectile,
    MagicHitEffect,
    MagicChargeProjectile,
    MagicChargeHitEffect,
    AttackRange,
    DamageText,
    SubCharacter,
    RangedMonster_Defensive,
    MidBoss,
}