using UnityEngine;

public class WeaponChange : MonoBehaviour
{
    [SerializeField] GameObject _weaponPanel;
    bool _canAction = false;

    private void Update()
    {
        OpenWeaponPanel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canAction = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _canAction = false;
        }
    }

    void OpenWeaponPanel()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _weaponPanel.SetActive(true);
            Time.timeScale = 0.0f;  // UI ø≠∏Æ∏È Ω√∞£¿Ã ∏ÿ√·¥Ÿ!
        }
    }
}
