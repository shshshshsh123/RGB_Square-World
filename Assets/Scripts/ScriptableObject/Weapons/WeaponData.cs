using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string weaponName; // ���� �̸�
    public Sprite weaponIcon; // UI�� ǥ�õ� ������
    [TextArea]
    public string weaponDescription; // ���� ����

    [Header("���� �Ӽ�")]
    public float damage; // ���ݷ�
    public float attackDelay; // ���� ������ (��)
    public float projectileSpeed; // ����ü �ӵ� (���Ÿ� �����)

    [Header("������ ����")]
    public GameObject projectilePrefab; // �߻��� ����ü �Ǵ� ���� ����Ʈ ������
}