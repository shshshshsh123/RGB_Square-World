using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int initalSize;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;    // ���� ������Ʈ���� ��� ��ųʸ�

    private void Awake()
    {
        // �̱��濡 DontDestroyOnLoad�� ������� ����
        Instance = this;
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.initalSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    /// <summary>
    /// Ǯ���� ������Ʈ�� �����ɴϴ�. Ǯ�� ��������� ���� �����մϴ�.
    /// </summary>
    /// <param name="tag">Pool�� �޸� �±��̸� (string�̹Ƿ� ��Ȯ�ϰ�)</param>
    /// <param name="position">������Ʈ�� ��Ÿ�� ��ġ</param>
    /// <param name="rotation">������Ʈ�� ȸ����</param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[������ƮǮ��] �±� {tag} �� ã�� �� �����ϴ�.");
            return null;
        }

        GameObject objectToSpawn;

        // ���� ť�� ��Ȱ��ȭ�� ������Ʈ�� �����ִٸ� �װ��� ���
        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        // ť�� ����ִٸ�, ���ο� ������Ʈ�� ����
        else
        {
            // pools ����Ʈ���� �ش� �±��� �������� ã��
            GameObject prefabToInstantiate = null;
            foreach (Pool pool in pools)
            {
                if (pool.tag == tag)
                {
                    prefabToInstantiate = pool.prefab;
                    break;
                }
            }

            if (prefabToInstantiate != null)
            {
                objectToSpawn = Instantiate(prefabToInstantiate, transform);
                Debug.Log($"[������ƮǮ��] �±� {tag} �� ���� ���ο� ������Ʈ�� �����մϴ�.");
            }
            else
            {
                Debug.LogWarning($"[������ƮǮ��] �±� {tag} �� ���� �������� ã�� �� �����ϴ�.");
                return null;
            }
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    /// <summary>
    /// ����� ���� ������Ʈ�� Ǯ�� �ݳ��մϴ�.
    /// </summary>
    /// <param name="tag">Pool�� �޸� �±��̸� (string�̹Ƿ� ��Ȯ�ϰ�)</param>
    /// <param name="objectToReturn">������ ������Ʈ</param>
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[������ƮǮ��] �±� {tag} �� ã�� �� �����ϴ�.");
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
