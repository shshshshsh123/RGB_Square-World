using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [Header("# 메인 로비")]
    [SerializeField] GameObject _mainLobbyContent;
    [SerializeField] TMP_Text _chapterText;
    [SerializeField] TMP_Text _chapterDescText;
    [SerializeField] Button _gameStartButton;
    [SerializeField] TMP_Text _gameStartText;
    [SerializeField] Button _combatStyleButton;
    [SerializeField] TMP_Text _combatStyleText;
    [SerializeField] Button _costumeButton;
    [SerializeField] TMP_Text _costumeText;
    [SerializeField] Button _gatchaButton;
    [SerializeField] TMP_Text _gatchaText;
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
    [SerializeField] TMP_Text _upgradeGoldText;
    [SerializeField] TMP_Text _upgradeFairyStoneText;
    [SerializeField] Button _backButton;
    [SerializeField] Button _startButton;   // 여기에도 전투시작 버튼 있음
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
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
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
        Outline outline = _upgradeButton.GetComponent<Outline>();
        Outline startOutline = _startButton.GetComponent<Outline>();

        // TODO: 여기 레벨이랑 필요골드 등등 나중에 데이터에서 가져오도록 바꾸기
        switch (combatStyle)
        {
            case CombatStyleType.Melee:
                _currentCombatStyleImage.sprite = _combatStyleImages[0];
                _combatStyleLevelText.text = "근접 전투";
                _levelUpText.text = "LV.1 -> LV.2";
                _upgradeGoldText.text = string.Format("{0:#,###}", 5000);
                _upgradeFairyStoneText.text = string.Format("{0:#,###}", 200);
                outline.effectColor = new Color32(200, 40, 40, 100);
                startOutline.effectColor = new Color32(200, 40, 40, 100);
                break;
            case CombatStyleType.Range:
                _currentCombatStyleImage.sprite = _combatStyleImages[1];
                _combatStyleLevelText.text = "원거리 전투";
                _levelUpText.text = "LV.3 -> LV.4";
                _upgradeGoldText.text = string.Format("{0:#,###}", 20000);
                _upgradeFairyStoneText.text = string.Format("{0:#,###}", 800);
                outline.effectColor = new Color32(40, 40, 200, 100);
                startOutline.effectColor = new Color32(40, 40, 200, 100);
                break;
            case CombatStyleType.Magic:
                _currentCombatStyleImage.sprite = _combatStyleImages[2];
                _combatStyleLevelText.text = "마법 전투";
                _levelUpText.text = "LV.2 -> LV.3";
                _upgradeGoldText.text = string.Format("{0:#,###}", 10000);
                _upgradeFairyStoneText.text = string.Format("{0:#,###}", 500);
                outline.effectColor = new Color32(200, 40, 200, 100);
                startOutline.effectColor = new Color32(200, 40, 200, 100);
                break;
            case CombatStyleType.Mix:
                _currentCombatStyleImage.sprite = _combatStyleImages[Random.Range(0, _combatStyleImages.Length)];
                _combatStyleLevelText.text = "혼합 전투";
                _levelUpText.text = "LV.5 -> LV.6";
                _upgradeGoldText.text = string.Format("{0:#,###}", 50000);
                _upgradeFairyStoneText.text = string.Format("{0:#,###}", 2000);
                outline.effectColor = new Color32(0, 0, 0, 100);
                startOutline.effectColor = new Color32(0, 0, 0, 100);
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
