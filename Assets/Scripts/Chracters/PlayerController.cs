using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("�÷��̾� �̵��ӵ�")]
    public float moveSpeed = 5.0f;

    private Rigidbody _rigidBody;
    private Vector3 _movement;

    void Awake()
    {
        // ������Ʈ ��������
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CalculateMovement();
    }

    void FixedUpdate()
    {
        PlayerMove();
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
        _rigidBody.linearVelocity = _movement * moveSpeed;
    }
}
