using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isApplicationQuitting = false;  // 게임 종료 후 인스턴스 접근 방지

    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[싱글톤] {typeof(T)} 의 객체가 이미 파괴되어있습니다. Null 리턴합니다.");
                return null;
            }
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                    Debug.Log($"[싱글톤] {typeof(T)} 인스턴스가 동적으로 생성되었습니다.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        isApplicationQuitting = false;  // 게임 종료 시 인스턴스 접근 방지 해제

        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[싱글톤] {typeof(T)} 인스턴스가 초기화되었습니다.");
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[싱글톤] {typeof(T)} 중복 객체가 발견되어 파괴됩니다.");
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;  // 게임 종료 시 인스턴스 접근 방지
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            isApplicationQuitting = true;  // 게임 종료 시 인스턴스 접근 방지
        }
    }
}
