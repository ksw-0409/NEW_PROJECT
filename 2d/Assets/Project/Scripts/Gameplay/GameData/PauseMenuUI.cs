using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// 역할: ESC 키로 열고 닫는 일시정지 메뉴

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private bool isPaused = false;

    void Start()
    {
        continueButton.onClick.AddListener(OnClickContinue);
        quitButton.onClick.AddListener(OnClickQuit);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 다른 UI가 열려있으면 ESC는 해당 UI가 처리
            if (BaseInteractable.IsUIOpen && !isPaused) return;

            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        BaseInteractable.IsUIOpen = isPaused;
    }

    private void OnClickContinue()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        BaseInteractable.IsUIOpen = false;
    }

    private void OnClickQuit()
    {
        // 일시정지 해제
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        BaseInteractable.IsUIOpen = false;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneController.SceneName.Dungeon)
        {
            Application.Quit();
            return;
        }

        // 패널티 적용 + 게임오버 UI 트리거
        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();
    }

    public void OnClickPauseButton()
    {
        TogglePause();
    }
}