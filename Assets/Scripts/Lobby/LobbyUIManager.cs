using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("# 메인 로비")]
    [SerializeField] GameObject _mainLobbyContent;
    [SerializeField] TMP_Text _chapterText;
    [SerializeField] TMP_Text _chapterIconText;
    [SerializeField] Button _gameStartButton;
    [SerializeField] Button _combatStyleButton;
    [SerializeField] Button _costumeButton;
    [SerializeField] Button _gatchaButton;
    [SerializeField] Button _historyButton;

    [Header("# 팝업")]
    [SerializeField] Button _popupContent;
    [SerializeField] GameObject _combatStylePopup;
    [SerializeField] GameObject _costumePopup;
    [SerializeField] GameObject _gatchaPopup;
    [SerializeField] GameObject _historyPopup;
    GameObject _currentPopup;

    [Header("# 전투스타일 버튼")]
    [SerializeField] Button _meleeButton;
    [SerializeField] Button _rangeButton;
    [SerializeField] Button _magicButton;
    [SerializeField] Button _mixButton;

    [Header("# 전투스타일 팝업")]
    [SerializeField] GameObject _combatStyleContent;
    [SerializeField] Sprite[] _combatStyleImages;
    [SerializeField] Image _currentCombatStyleImage;
    [SerializeField] TMP_Text _combatStyleLevelText;
    [SerializeField] TMP_Text _goldAmountText;
    [SerializeField] TMP_Text _fairyStoneAmountText;
    [SerializeField] TMP_Text _levelUpText;
    [SerializeField] Button _upgradeButton;
    [SerializeField] Button _backButton;
    enum CombatStyleType { Melee, Range, Magic, Mix };

    void Start()
    {
        _currentPopup = _combatStylePopup;
        // TODO: 챕터 텍스트 설정

        // 버튼 클릭 이벤트 설정
        _gameStartButton.onClick.AddListener(() => StartGame());
        _combatStyleButton.onClick.AddListener(() => PopUpUI(_combatStylePopup, true));
        _costumeButton.onClick.AddListener(() => PopUpUI(_costumePopup, true));
        _gatchaButton.onClick.AddListener(() => PopUpUI(_gatchaPopup, true));
        _historyButton.onClick.AddListener(() => PopUpUI(_historyPopup, true));
        // 팝업 닫기 버튼 이벤트 설정
        _popupContent.onClick.AddListener(() => PopUpUI(_currentPopup, false));
        // 전투스타일 버튼 이벤트 설정
        _meleeButton.onClick.AddListener(() => CombatStyle(CombatStyleType.Melee));
        _rangeButton.onClick.AddListener(() => CombatStyle(CombatStyleType.Range));
        _magicButton.onClick.AddListener(() => CombatStyle(CombatStyleType.Magic));
        _mixButton.onClick.AddListener(() => CombatStyle(CombatStyleType.Mix));
        // 전투스타일 팝업 버튼 이벤트 설정
        _upgradeButton.onClick.AddListener(() => Upgrade());
        _backButton.onClick.AddListener(() => BackButtonCombatStyle());
    }

    void StartGame()
    {
        // TODO: 씬로더 만들고나서 구현
    }

    void PopUpUI(GameObject popup, bool doActive)
    {
        // 팝업이 활성화되면 메인 로비 UI 비활성화
        if (doActive)
        {
            _mainLobbyContent.SetActive(false);
            _popupContent.gameObject.SetActive(true);
        }
        else
        {
            _mainLobbyContent.SetActive(true);
            _popupContent.gameObject.SetActive(false);
        }

        popup.gameObject.SetActive(doActive);
        _currentPopup = popup;
    }

    void CombatStyle(CombatStyleType combatStyle)
    {
        _combatStyleContent.SetActive(true);
        _popupContent.gameObject.SetActive(false);

        switch (combatStyle)
        {
            case CombatStyleType.Melee:
                _currentCombatStyleImage.sprite = _combatStyleImages[0];
                _combatStyleLevelText.text = "근접 Lv.1";
                _levelUpText.text = "LV.1 -> LV.2";
                break;
            case CombatStyleType.Range:
                _currentCombatStyleImage.sprite = _combatStyleImages[1];
                _combatStyleLevelText.text = "원거리 Lv.3";
                _levelUpText.text = "LV.3 -> LV.4";
                break;
            case CombatStyleType.Magic:
                _currentCombatStyleImage.sprite = _combatStyleImages[2];
                _combatStyleLevelText.text = "마법 Lv.2";
                _levelUpText.text = "LV.2 -> LV.3";
                break;
            case CombatStyleType.Mix:
                _currentCombatStyleImage.sprite = _combatStyleImages[Random.Range(0, _combatStyleImages.Length)];
                _combatStyleLevelText.text = "혼합 Lv.5";
                _levelUpText.text = "LV.5 -> LV.6";
                break;
        }

        // TODO: 임시로 해두었는데, 어딘가에 저장해두고 가져오는거로 바꾸기
        _goldAmountText.text = string.Format("{0:#,###}", 100000);
        _fairyStoneAmountText.text = string.Format("{0:#,###}", 5000);
    }

    void Upgrade()
    {
        // TODO: 전투스타일 업그레이드 구현 (골드, 요정석 차감 및 레벨업)
    }

    void BackButtonCombatStyle()
    {
        _combatStyleContent.SetActive(false);
        _popupContent.gameObject.SetActive(true);
    }
}
