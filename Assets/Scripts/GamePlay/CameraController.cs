using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("# Camera Target")]
    [Tooltip("카메라가 따라다닐 대상")]
    public Transform target;

    [Header("# Camera Settings")]
    [Tooltip("타겟으로부터 떨어질 기본 거리와 각도")]
    public Vector3 offset = new Vector3(0, 10, -10);
    [Tooltip("카메라의 움직임 부드러운 정도")]
    public float smoothSpeed = 0.125f;

    [Header("# Zoom Settings")]
    [Tooltip("마우스 휠 확대/축소 속도")]
    public float zoomSpeed = 10f;
    [Tooltip("가장 가까이 줌 할 수 있는 거리")]
    public float minZoomDistance = 5f;
    [Tooltip("가장 멀리 줌 할 수 있는 거리")]
    public float maxZoomDistance = 20f;

    [Header("# Obstacle Setting")]
    [Tooltip("투명화 처리할 장애물들의 레이어")]
    public LayerMask obstacleLayer;
    [Tooltip("장애물이 투명해지는 속도")]
    public float fadeSpeed = 5f;
    [Tooltip("장애물 감지하는 반경")]
    public float dectectRadius = 3f;
    [Tooltip("투명해지는 정도")]
    [Range(0.0f, 1.0f)]
    public float obstacleIntensity = 0.5f;

    private List<Renderer> _fadedRenderers = new List<Renderer>();
    void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleMovement();
        HandleObstacles();
    }

    /// <summary>
    /// 마우스 휠 입력받아서 카메라의 거리 조절
    /// </summary>
    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput == 0) return;

        // 현재 offset의 방향은 유지한채 카메라와의 거리만 조절하기
        float currentDistance = offset.magnitude;
        currentDistance -= scrollInput * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

        offset = offset.normalized * currentDistance;
    }

    /// <summary>
    /// target의 위치와 offset 값을 기준으로 카메라 이동
    /// </summary>
    void HandleMovement()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(transform.position);
    }

    /// <summary>
    /// 카메라와 타겟 사이의 장애물 감지후 투명하게 만들기
    /// </summary>
    void HandleObstacles()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        // SphereCastAll을 사용하여 감지 반경 내의 모든 장애물을 찾기
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, dectectRadius, direction, distance, obstacleLayer);

        // 현재 프레임에 감지된 렌더러들을 임시로 저장할 리스트
        List<Renderer> currentlyHitRenderers = new List<Renderer>();
        foreach (var hit in hits)
        {
            // 충돌한 오브젝트 자신의 Renderer 확인
            Renderer selfRenderer = hit.collider.GetComponent<Renderer>();

            if (selfRenderer != null)
            {
                currentlyHitRenderers.Add(selfRenderer);
            }

            // 자식 Renderer 확인
            Renderer[] childRenderers = hit.collider.GetComponentsInChildren<Renderer>();
            if (childRenderers != null && childRenderers.Length > 0)
            {
                currentlyHitRenderers.AddRange(childRenderers);
            }

            // 부모 Renderer 확인
            Transform parent = hit.collider.transform.parent;
            if (parent != null)
            {
                Renderer parentRenderer = parent.GetComponent<Renderer>();
                if (parentRenderer != null && !currentlyHitRenderers.Contains(parentRenderer))
                {
                    currentlyHitRenderers.Add(parentRenderer);
                }
            }
        }

        // 이전에 투명했지만 이제는 감지되지 않은 렌더러들을 다시 불투명하게
        for (int i = _fadedRenderers.Count - 1; i >= 0; i--)
        {
            Renderer renderer = _fadedRenderers[i];
            if (!currentlyHitRenderers.Contains(renderer))
            {
                StartCoroutine(FadeMaterial(renderer, 1.0f)); // 불투명하게
                _fadedRenderers.RemoveAt(i);
            }
        }

        // 새로 감지된 렌더러들을 투명하게
        foreach (var renderer in currentlyHitRenderers)
        {
            if (!_fadedRenderers.Contains(renderer))
            {
                StartCoroutine(FadeMaterial(renderer, obstacleIntensity)); // 반투명하게
                _fadedRenderers.Add(renderer);
            }
        }
    }

    /// <summary>
    /// 머티리얼의 Alpha 값을 조절하여 부드럽게 페이드 효과를 줍니다.
    /// </summary>
    private IEnumerator FadeMaterial(Renderer renderer, float targetAlpha)
    {
        // renderer.materials를 사용하여 모든 머티리얼에 적용
        Material[] materials = renderer.materials;

        // 각 머티리얼의 초기 색상을 저장할 리스트
        List<Color> startColors = new List<Color>();

        foreach (var mat in materials)
        {
            startColors.Add(mat.color);
        }

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * fadeSpeed;

            // 모든 머티리얼의 투명도를 조절
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;

                Color newColor = startColors[i];
                // Mathf.Lerp로 알파값 부드럽게 변경
                newColor.a = Mathf.Lerp(startColors[i].a, targetAlpha, time);
                materials[i].color = newColor;
            }
            yield return null;
        }
    }
}
