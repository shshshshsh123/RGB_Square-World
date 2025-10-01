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
    public Dictionary<string, Queue<GameObject>> poolDictionary;    // 실제 오브젝트들이 담길 딕셔너리

    private void Awake()
    {
        // 싱글톤에 DontDestroyOnLoad는 사용하지 않음
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
    /// 풀에서 오브젝트를 가져옵니다. 풀이 비어있으면 새로 생성합니다.
    /// </summary>
    /// <param name="tag">Pool에 달린 태그이름 (string이므로 정확하게)</param>
    /// <param name="position">오브젝트가 나타날 위치</param>
    /// <param name="rotation">오브젝트가 회전값</param>
    /// <returns></returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[오브젝트풀러] 태그 {tag} 를 찾을 수 없습니다.");
            return null;
        }

        GameObject objectToSpawn;

        // 만약 큐에 비활성화된 오브젝트가 남아있다면 그것을 사용
        if (poolDictionary[tag].Count > 0)
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        // 큐가 비어있다면, 새로운 오브젝트를 생성
        else
        {
            // pools 리스트에서 해당 태그의 프리팹을 찾기
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
                Debug.Log($"[오브젝트풀러] 태그 {tag} 에 대해 새로운 오브젝트를 생성합니다.");
            }
            else
            {
                Debug.LogWarning($"[오브젝트풀러] 태그 {tag} 에 대한 프리팹을 찾을 수 없습니다.");
                return null;
            }
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀에 반납합니다.
    /// </summary>
    /// <param name="tag">Pool에 달린 태그이름 (string이므로 정확하게)</param>
    /// <param name="objectToReturn">리턴할 오브젝트</param>
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[오브젝트풀러] 태그 {tag} 를 찾을 수 없습니다.");
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
