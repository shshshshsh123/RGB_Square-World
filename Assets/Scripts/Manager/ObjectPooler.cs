using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public PoolType type; // enum���� ����
        public GameObject prefab;
        public int initalSize;
    }

    public List<Pool> pools;
    public Dictionary<PoolType, Queue<GameObject>> poolDictionary;
    private Dictionary<PoolType, GameObject> prefabDictionary; // �������� ������ ã�� ���� ��ųʸ�

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        poolDictionary = new Dictionary<PoolType, Queue<GameObject>>();
        prefabDictionary = new Dictionary<PoolType, GameObject>(); // �ʱ�ȭ

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
            prefabDictionary.Add(pool.type, pool.prefab); // ������ ������ ��ųʸ��� ����
        }
    }

    public GameObject SpawnFromPool(PoolType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"[������ƮǮ��] Ÿ�� {type} �� ã�� �� �����ϴ�.");
            return null;
        }

        GameObject objectToSpawn;

        if (poolDictionary[type].Count > 0)
        {
            objectToSpawn = poolDictionary[type].Dequeue();
        }
        else
        {
            // foreach ���� ��� ��ųʸ����� ��� �������� ã�� ���� ����
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
            Debug.LogWarning($"[������ƮǮ��] Ÿ�� {type} �� ã�� �� �����ϴ�.");
            Destroy(objectToReturn); // Ǯ�� ������ �׳� �ı�
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