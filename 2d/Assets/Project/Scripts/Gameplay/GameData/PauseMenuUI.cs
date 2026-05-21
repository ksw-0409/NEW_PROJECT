using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Animator pauseAnimator;
    [SerializeField] private Button[] buttonsToShowAfterAnim;
    [SerializeField] private string openAnimName = "PanelIntro";

    private bool isPaused = false;
    private Coroutine showButtonsCoroutine;

    void Start()
    {
        foreach (var clip in pauseAnimator.runtimeAnimatorController.animationClips)
            Debug.Log($"클립 이름: [{clip.name}]");

        continueButton.onClick.AddListener(OnClickContinue);
        quitButton.onClick.AddListener(OnClickQuit);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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

        if (isPaused)
        {
            SetButtonsVisible(false);
            pauseAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

            if (showButtonsCoroutine != null) StopCoroutine(showButtonsCoroutine);
            showButtonsCoroutine = StartCoroutine(PlayAnimAndShowButtons());
        }
    }
    private IEnumerator PlayAnimAndShowButtons()
    {
        yield return null; // 한 프레임 대기 (Animator 초기화 완료 후 Play)
        pauseAnimator.Play(openAnimName, 0, 0f);

        float clipLength = GetClipLength(openAnimName);
        yield return new WaitForSecondsRealtime(clipLength);
        SetButtonsVisible(true);
    }


    private void SetButtonsVisible(bool visible)
    {
        foreach (var btn in buttonsToShowAfterAnim)
            btn.gameObject.SetActive(visible);
    }

    private float GetClipLength(string clipName)
    {
        foreach (var clip in pauseAnimator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;

        Debug.LogWarning($"클립 '{clipName}'을 찾을 수 없습니다.");
        return 1f;
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
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        BaseInteractable.IsUIOpen = false;

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneController.SceneName.Dungeon)
        {
            Application.Quit();
            return;
        }

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.TriggerGameOver();
    }

    public void OnClickPauseButton()
    {
        TogglePause();
    }
}