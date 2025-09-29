using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [Header("# Camera Target")]
    [Tooltip("ī�޶� ����ٴ� ���")]
    public Transform target;

    [Header("# Camera Settings")]
    [Tooltip("Ÿ�����κ��� ������ �⺻ �Ÿ��� ����")]
    public Vector3 offset = new Vector3(0, 10, -10);
    [Tooltip("ī�޶��� ������ �ε巯�� ����")]
    public float smoothSpeed = 0.125f;

    [Header("# Zoom Settings")]
    [Tooltip("���콺 �� Ȯ��/��� �ӵ�")]
    public float zoomSpeed = 10f;
    [Tooltip("���� ������ �� �� �� �ִ� �Ÿ�")]
    public float minZoomDistance = 5f;
    [Tooltip("���� �ָ� �� �� �� �ִ� �Ÿ�")]
    public float maxZoomDistance = 20f;

    [Header("# Obstacle Setting")]
    [Tooltip("����ȭ ó���� ��ֹ����� ���̾�")]
    public LayerMask obstacleLayer;
    [Tooltip("��ֹ��� ���������� �ӵ�")]
    public float fadeSpeed = 5f;
    [Tooltip("��ֹ� �����ϴ� �ݰ�")]
    public float dectectRadius = 3f;
    [Tooltip("���������� ����")]
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
    /// ���콺 �� �Է¹޾Ƽ� ī�޶��� �Ÿ� ����
    /// </summary>
    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput == 0) return;

        // ���� offset�� ������ ������ä ī�޶���� �Ÿ��� �����ϱ�
        float currentDistance = offset.magnitude;
        currentDistance -= scrollInput * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);

        offset = offset.normalized * currentDistance;
    }

    /// <summary>
    /// target�� ��ġ�� offset ���� �������� ī�޶� �̵�
    /// </summary>
    void HandleMovement()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(transform.position);
    }

    /// <summary>
    /// ī�޶�� Ÿ�� ������ ��ֹ� ������ �����ϰ� �����
    /// </summary>
    void HandleObstacles()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        // SphereCastAll�� ����Ͽ� ���� �ݰ� ���� ��� ��ֹ��� ã��
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, dectectRadius, direction, distance, obstacleLayer);

        // ���� �����ӿ� ������ ���������� �ӽ÷� ������ ����Ʈ
        List<Renderer> currentlyHitRenderers = new List<Renderer>();
        foreach (var hit in hits)
        {
            currentlyHitRenderers.Add(hit.collider.GetComponent<Renderer>());
        }

        // ������ ���������� ������ �������� ���� ���������� �ٽ� �������ϰ�
        for (int i = _fadedRenderers.Count - 1; i >= 0; i--)
        {
            Renderer renderer = _fadedRenderers[i];
            if (!currentlyHitRenderers.Contains(renderer))
            {
                StartCoroutine(FadeMaterial(renderer, 1.0f)); // �������ϰ�
                _fadedRenderers.RemoveAt(i);
            }
        }

        // ���� ������ ���������� �����ϰ�
        foreach (var renderer in currentlyHitRenderers)
        {
            if (!_fadedRenderers.Contains(renderer))
            {
                StartCoroutine(FadeMaterial(renderer, 0.3f)); // �������ϰ�
                _fadedRenderers.Add(renderer);
            }
        }
    }

    /// <summary>
    /// ��Ƽ������ Alpha ���� �����Ͽ� �ε巴�� ���̵� ȿ���� �ݴϴ�.
    /// </summary>
    private IEnumerator FadeMaterial(Renderer renderer, float targetAlpha)
    {
        Material material = renderer.material;

        if (targetAlpha < 1.0f) material.SetFloat("_Surface", 1);   // ���̴��� SurfaceŸ���� Transparent�� (���İ� ��������)
        else material.SetFloat("_Surface", 0);  // ���̴��� SurfaceŸ���� �ٽ� Opaque�� ����

        Color startColor = material.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float time = 0f;
        while (time < 1f)
        {
            // Lerp�� ����Ͽ� �ε巴�� ���� ����
            material.color = Color.Lerp(startColor, endColor, time);
            time += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        material.color = endColor; // ���� ���� ����
    }
}
