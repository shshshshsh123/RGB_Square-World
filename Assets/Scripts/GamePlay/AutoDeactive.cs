using UnityEngine;

public class AutoDeactive : MonoBehaviour
{
    [Tooltip("자동으로 비활성화될 때까지의 시간 (초)")]
    public float lifeTime = 1.0f;
    [Tooltip("오브젝트 풀러에 등록된 이 오브젝트의 태그")]
    public string poolTag;

    private void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 lifeTime 후에 ReturnToPool 메서드를 호출하도록 예약합니다.
        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void ReturnToPool()
    {
        // 오브젝트 풀러가 존재하고, 태그가 설정되어 있을 때만 풀에 반납합니다.
        if (ObjectPooler.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}의 태그가 이상해요!!!");
        }
    }
}
