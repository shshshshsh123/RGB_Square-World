using UnityEngine;

public class DummyDamage : MonoBehaviour
{
    int hitTimes = 0;

    void Update()
    {
        if (hitTimes >= 5)
        {
            Debug.Log($"{gameObject.name}가 죽었습니다!");
            ObjectPooler.Instance.ReturnToPool("Dummy", gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        hitTimes++;
        Debug.Log($"끼에에에엑! {gameObject.name}가 {damage}만큼의 데미지를 입었습니다! 맞은횟수: {hitTimes}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.PlayerTakeDamage(10f, gameObject);
            }
        }
    }
}
