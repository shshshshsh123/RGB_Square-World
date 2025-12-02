using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;

    [Header("# 게임오버 UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverDescText;
    public Button retryButton;
    public Button lobbyButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // 버튼 클릭 이벤트 등록
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        lobbyButton.onClick.AddListener(OnLobbyButtonClicked);
    }

    private void OnRetryButtonClicked()
    {
        // 게임 재시작시 그냥 게임 씬 다시 로드하기
        SceneLoader.LoadScene(SceneLoader.Scene.GameScene);
    }

    private void OnLobbyButtonClicked()
    {
        // 로비로 이동하는 로직 구현
        SceneLoader.LoadScene(SceneLoader.Scene.LobbyScene);
    }

    /// <summary>
    /// 게임오버시 UI 보여주기
    /// </summary>
    public void ShowGameOverUI(string text)
    {
        TimeManager.Instance.RequestTimeScale(this, 0f);    // 시간 멈춰 주세요~
        gameOverPanel.SetActive(true);
        gameOverDescText.text = text;
    }
}
