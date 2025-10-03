using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("# �⺻ ����")]
    public WeaponType weaponType; // ���� Ÿ�� (����, ���Ÿ� ��)
    public string weaponTag; // ���� �±� (������Ʈ Ǯ����)
    public Sprite weaponIcon; // UI�� ǥ�õ� ������
    [TextArea]
    public string weaponDescription; // ���� ����

    [Header("# ���� ��ġ ������")]
    public Vector3 attackPositionOffset; // ���� ��ġ ������ (�÷��̾� ����)

    [Header("# ���� �Ӽ�")]
    public float damage; // ���ݷ�
    public float attackDelay; // ���� ������ (��)
    public float lifeTime; // ���� ���� �ð� (��)
    public float projectileSpeed; // ����ü �ӵ� (���Ÿ� �����)

    [Header("# ������ ����")]
    public GameObject projectilePrefab; // �߻��� ����ü �Ǵ� ���� ����Ʈ ������
}

public enum WeaponType
{
    Melee,      // ���� ����
    Ranged      // ���Ÿ� ����
}