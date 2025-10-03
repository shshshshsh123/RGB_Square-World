using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("# Player Setting")]
    [Tooltip("�÷��̾� �̵��ӵ�")]
    public float moveSpeed = 7.5f;
    [Tooltip("�÷��̾� ȸ���ӵ�")]
    public float rotationSpeed = 15.0f;

    [Header("# Mouse Settings")]
    [Tooltip("���콺 ��ġ�� �ν��� �ٴ� ���̾�")]
    public LayerMask groundLayer;

    private Rigidbody _rigidBody;
    private Animator _animator;

    private Vector3 _movement;
    private Quaternion _rotation;
    public Quaternion Rotation => _rotation;

    private Camera _mainCamera;

    void Awake()
    {
        // ������Ʈ ��������
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _mainCamera = Camera.main;

        _rigidBody.freezeRotation = true; // ���������� ���� ȸ�� ����
    }

    void Update()
    {
        CalculateMovement();
        CalculateRotation();

        // Test
        TestFunction();
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
            // �ִϸ��̼� ����
            _animator.SetBool("isRunning", true);
        }
        else
        {
            // �Է��� ���� ���� �ӵ��� 0���� �����Ͽ� ���� (but! y�� �������� ���� ex) ����, ��������)
            Vector3 currentVelocity = _rigidBody.linearVelocity;
            _rigidBody.linearVelocity = new Vector3(0, currentVelocity.y, 0);

            // �ִϸ��̼� ����
            _animator.SetBool("isRunning", false);
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

            // ��ǥ ȸ�� ���� ����ؼ� ������ ���� (���콺�� �÷��̾� ��ġ�� ���� ���ٸ� ȸ������ ����)
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                _rotation = Quaternion.LookRotation(lookDirection);
            }
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

    void TestFunction()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.IncreaseMaxHp(Random.Range(80f, 550f));
        }
        else if (Input.GetMouseButtonDown(2))
        {
            ObjectPooler.Instance.SpawnFromPool("Dummy", transform.position + transform.forward * 2, Quaternion.identity);
        }
    }
}
