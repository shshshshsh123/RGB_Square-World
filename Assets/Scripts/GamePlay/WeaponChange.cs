using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] GameObject _weaponPanel;
    [SerializeField] GameObject _keyCanvas;
    [SerializeField] Vector3 _keyCanvasOffset;
    bool _canAction = false;

    [Header("# UI")]
    [SerializeField] TMP_Text[] _texts; // 0: Melee, 1: Range, 2: Magic

    private PlayerAttack _playerAttack; // 무기 정보 확인용

    private void Update()
    {
        OpenWeaponPanel();
    }

    private void LateUpdate()
    {
        _keyCanvas.transform.position = transform.position + _keyCanvasOffset;
        _keyCanvas.transform.LookAt(_keyCanvas.transform.position + Camera.main.transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canAction = true;
            _keyCanvas.SetActive(true);
            _playerAttack = other.GetComponent<PlayerAttack>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canAction = false;
            _keyCanvas.SetActive(false);
        }
    }

    void OpenWeaponPanel()
    {
        if (Input.GetKeyDown(KeyCode.F) && _canAction)
        {
            Time.timeScale = 0.0f;  // UI 열리면 시간이 멈춘다!
            _weaponPanel.SetActive(true);

            if (_playerAttack.equippedWeapons == null) _playerAttack.equippedWeapons = new List<PlayerAttack.EquippedWeapon>();

            for (int i = 0; i < 3; i++)
            {
                _texts[i].text = "새로 획득!";
            }
            if (_playerAttack.equippedWeapons.Count > 0)
            {
                int type = (int)_playerAttack.equippedWeapons[0].weaponData.weaponType;
                if (_playerAttack.equippedWeapons[0].currentLevel == 5) _texts[type].text = "최고레벨!";
                else _texts[type].text = "Lv." + _playerAttack.equippedWeapons[0].currentLevel.ToString() + " -> Lv." + (_playerAttack.equippedWeapons[0].currentLevel + 1).ToString();
            }
        }
    }

    public void ChangeWeapon(int type)  // 0: Melee, 1: Range, 2: Magic
    {
        if (_playerAttack.equippedWeapons == null) _playerAttack.equippedWeapons = new List<PlayerAttack.EquippedWeapon>();

        if (_playerAttack.equippedWeapons.Count > 0)
        {
            // 무기가 같은걸 눌럿으면 레벨업 합니다.
            if (type == (int)_playerAttack.equippedWeapons[0].weaponData.weaponType)
            {
                _playerAttack.AddOrUpgradeWeapon(_playerAttack.weaponDatas[type]); // 일단은 임시로 이렇게 처리함;; 좀 더럽긴한데
            }
            // 다른 무기 골랏으면 무기 다 지우고 다시 새거 획득합니다다다다.
            else
            {
                _playerAttack.equippedWeapons.Clear();
                _playerAttack.AddOrUpgradeWeapon(_playerAttack.weaponDatas[type]);
            }
        }
        else
        {
            // 이건 무기 처음 획득할때
            _playerAttack.AddOrUpgradeWeapon(_playerAttack.weaponDatas[type]);
        }

        // 고르는게 끝나며는?
        _weaponPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
