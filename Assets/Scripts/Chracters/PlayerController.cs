using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("�÷��̾� �̵��ӵ�")]
    public float moveSpeed = 5.0f;
    [Tooltip("�÷��̾� ȸ���ӵ�")]
    public float rotationSpeed = 15.0f;

    [Header("# Mouse Settings")]
    [Tooltip("���콺 ��ġ�� �ν��� �ٴ� ���̾�")]
    public LayerMask groundLayer;

    private Rigidbody _rigidBody;

    private Vector3 _movement;
    private Quaternion _rotation;

    private Camera _mainCamera;

    void Awake()
    {
        // ������Ʈ ��������
        _rigidBody = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
    }

    void Update()
    {
        CalculateMovement();
        CalculateRotation();
    }

    void FixedUpdate()
    {
        PlayerMove();
        PlayerRotate();
    }

    /// <summary>
    /// �����ӿ� ���õ� Input ������ �̵����� ����
    /// </summary>
    void CalculateMovement()
    {
        // �Է°���
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        // �̵����⸸ ���
        _movement = new Vector3(hAxis, 0, vAxis).normalized;
    }

    /// <summary>
    /// �÷��̾� �̵�(rigidBody�� �̿��� ó��) -> FixedUpdate���� ����Ұ�
    /// </summary>
    void PlayerMove()
    {
        // �Է��� ���� ���� �̵� �ӵ��� ���� (�̲����� ����)
        if (_movement.magnitude > 0.01f)
        {
            _rigidBody.linearVelocity = _movement * moveSpeed;
        }
        else
        {
            // �Է��� ���� ���� �ӵ��� 0���� �����Ͽ� ���� (but! y�� �������� ���� ex) ����, ��������)
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(0, currentVelocity.y, 0);
        }
    }

    /// <summary>
    /// ���콺 Ŀ�� ���������� ��ǥ ȸ�� ���� ����մϴ�.
    /// </summary>
    void CalculateRotation()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 lookDirection = hit.point - transform.position;
            lookDirection.y = 0;

            // ��ǥ ȸ�� ���� ����ؼ� ������ ���常 �صӴϴ�.
            _rotation = Quaternion.LookRotation(lookDirection).normalized;
        }
    }

    /// <summary>
    /// ���� �������� �÷��̾ ȸ����ŵ�ϴ�. (Rigidbody�� �̿�)
    /// </summary>
    void PlayerRotate()
    {
        // Slerp�� ����Ͽ� �ε巯�� ȸ��
        Quaternion newRotation = Quaternion.Slerp(_rigidBody.rotation, _rotation, rotationSpeed * Time.fixedDeltaTime).normalized;
        _rigidBody.MoveRotation(newRotation);
    }
}
