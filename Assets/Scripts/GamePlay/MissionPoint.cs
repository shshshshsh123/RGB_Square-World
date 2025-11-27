using Unity.VisualScripting;
using UnityEngine;

public class MissionPoint : MonoBehaviour
{
    public string id;   // SO랑 맞춰야됨 똑같이!!!!!!!
    private bool _isGoaled = false;
    private bool _isSubGoaled = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            if (_isGoaled) return;
            if (other.isTrigger) return;    // 피격 판정용 콜라이더 무시하기
            if (MissionManager.Instance.currentMission.Data.missionType == MissionType.Defense)
            {
                _isGoaled = true;
                MissionManager.Instance.OnDestinationReached();
            }
        }
        else if (other.CompareTag("SubCharacter"))
        {
            if (_isSubGoaled) return;
            if (other.isTrigger) return;    // 피격 판정용 콜라이더 무시하기
            if (MissionManager.Instance.currentMission.Data.missionType == MissionType.Defense)
            {
                _isSubGoaled = true;
                MissionManager.Instance.OnSubCharacterDestinationReached();
            }
        }
    }
}
