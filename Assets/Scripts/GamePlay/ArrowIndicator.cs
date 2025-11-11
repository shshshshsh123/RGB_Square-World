using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Transform _arrowTransform;

    private void LateUpdate()
    {
        if (_playerTransform == null || _targetTransform == null) return;   // 플레이어 위치와 타겟위치 둘다 있어야지만 작동합니다이야이야.

        // 일단 플레이어 따라서 움직임.
        Vector3 basePos = _playerTransform.position;
        basePos.y += 0.005f;    // 바닥에서 살짝 떨어지게

        Vector3 direction = _targetTransform.position - _playerTransform.position;
        direction.y = 0f;   // 방향에 y는 없다.

        if (direction.magnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-direction, Vector3.up);    // 화살표 방향이 그림생긴거 때문에 돌아가있어서 -붙임.
            _arrowTransform.rotation = targetRotation;  // Slerp쓰니까 플레이어 회전 떄문에 자꾸움찔거려서 그냥 이걸로함
        }
    }

    public void SetTarget(Transform targetTransform)
    {
        _targetTransform = targetTransform;
    }
}
