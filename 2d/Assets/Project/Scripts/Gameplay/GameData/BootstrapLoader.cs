using UnityEngine;
using UnityEngine.UI;

// 역할: 타이틀 화면 — 버튼 클릭 시 거점 씬으로 이동

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[Bootstrap] SceneController가 없습니다.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameDataManager가 없습니다.");
            return;
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnClickStart);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnClickStart()
    {
        SceneController.Instance.LoadBase();
    }

    private void OnClickQuit()
    {
        Application.Quit();
    }
}