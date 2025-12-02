using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    static string nextSceneName;
    [SerializeField] private Image loadingBarProgress;
    [SerializeField] private TMP_Text loadingPerText;

    static int tipNum = -1;
    [SerializeField] private TMP_Text tipText;

    const float fakeLoadSpeed = 0.5f;   // 가짜로딩 속도
    const float targetLoadProgress = 0.9f;  // 실제 로딩으로 로딩바 어디까지

    public enum Scene
    {
        LobbyScene,
        GameScene
    }

    public static void LoadScene(Scene scene)
    {
        nextSceneName = scene.ToString();
        Debug.Log($"[씬 로더] {nextSceneName} 씬 로딩 시작");
        TimeManager.Instance.ResetAllTimeScales();  // 씬 전환 시 타임스케일 초기화
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        tipNum = tipNum == 4 ? 0 : tipNum + 1;
        switch (tipNum)
        {
            case 0:
                tipText.text = "Tip! 가나다라.";
                break;
            case 1:
                tipText.text = "Tip! 안녕하세요.";
                break;
            case 2:
                tipText.text = "Tip! 하이하이.";
                break;
            case 3:
                tipText.text = "Tip! ㅁㄴㅇㄹ.";
                break;
        }
        StartCoroutine(LoadSceneProgress());
    }

    IEnumerator LoadSceneProgress()
    {
        // 로딩바 초기화
        loadingBarProgress.fillAmount = 0f;

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false; // 로딩 일부러 멈춰서 최소시간 확보

        while (!operation.isDone)
        {
            yield return null;

            loadingPerText.text = $"{Mathf.RoundToInt(loadingBarProgress.fillAmount * 100)}%";

            if (operation.progress < targetLoadProgress)
            {   // targetLoadProgress까지는 실제 로딩
                loadingBarProgress.fillAmount = Mathf.Clamp01(operation.progress / 0.9f);
            }
            else
            {   // targetLoadProgress 이후부터는 가짜 로딩
                loadingBarProgress.fillAmount += fakeLoadSpeed * Time.unscaledDeltaTime;

                if (loadingBarProgress.fillAmount >= 0.999f)
                {
                    Debug.Log($"[씬 로더] {nextSceneName} 씬 로딩 완료");
                    loadingBarProgress.fillAmount = 1f;
                    operation.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}