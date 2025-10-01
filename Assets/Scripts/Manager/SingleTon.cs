using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isApplicationQuitting = false;  // ���� ���� �� �ν��Ͻ� ���� ����

    public static T Instance
    {
        get
        {
            if (isApplicationQuitting)
            {
                Debug.LogWarning($"[�̱���] {typeof(T)} �� ��ü�� �̹� �ı��Ǿ��ֽ��ϴ�. Null �����մϴ�.");
                return null;
            }
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance = obj.AddComponent<T>();
                    Debug.Log($"[�̱���] {typeof(T)} �ν��Ͻ��� �������� �����Ǿ����ϴ�.");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        isApplicationQuitting = false;  // ���� ���� �� �ν��Ͻ� ���� ���� ����

        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[�̱���] {typeof(T)} �ν��Ͻ��� �ʱ�ȭ�Ǿ����ϴ�.");
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[�̱���] {typeof(T)} �ߺ� ��ü�� �߰ߵǾ� �ı��˴ϴ�.");
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;  // ���� ���� �� �ν��Ͻ� ���� ����
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            isApplicationQuitting = true;  // ���� ���� �� �ν��Ͻ� ���� ����
        }
    }
}
