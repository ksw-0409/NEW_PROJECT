using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 추가

public class GameOverUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private Button returnButton;
    [SerializeField] private GameObject gameOverPanel;

    // ↓ 추가
    [Header("애니메이션")]
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private string openAnimName = "GameOverIntro";
    [SerializeField] private GameObject[] uiElementsToShow; // 텍스트 + 버튼을 배열로 연결

    private Coroutine showUICoroutine;

    void OnEnable()
    {
        GameOverManager.OnGameOver += ShowGameOver;
    }

    void OnDisable()
    {
        GameOverManager.OnGameOver -= ShowGameOver;
    }

    void Start()
    {
        returnButton.onClick.AddListener(OnClickReturn);
        gameOverPanel.SetActive(false);
    }

    private void ShowGameOver(GameOverData data)
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        // 텍스트 내용은 미리 세팅, 오브젝트는 숨김
        SetupTexts(data);
        SetUIVisible(false); // ← 추가

        // 애니메이션 재생 후 UI 표시
        if (showUICoroutine != null) StopCoroutine(showUICoroutine);
        showUICoroutine = StartCoroutine(PlayAnimAndShowUI()); // ← 추가
    }

    // ↓ 추가
    private IEnumerator PlayAnimAndShowUI()
    {
        gameOverAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        yield return null; // Animator 초기화 대기
        gameOverAnimator.Play(openAnimName, 0, 0f);

        float clipLength = GetClipLength(openAnimName);
        yield return new WaitForSecondsRealtime(clipLength);

        SetUIVisible(true);
    }

    // ↓ 추가
    private void SetUIVisible(bool visible)
    {
        foreach (var obj in uiElementsToShow)
            obj.SetActive(visible);
    }

    // ↓ 추가
    private float GetClipLength(string clipName)
    {
        foreach (var clip in gameOverAnimator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;

        Debug.LogWarning($"클립 '{clipName}'을 찾을 수 없습니다.");
        return 1f;
    }

    // 텍스트 세팅만 분리
    private void SetupTexts(GameOverData data)
    {
        if (floorText != null)
            floorText.text = $"{data.floor}층에서 쓰러짐";

        if (playTimeText != null)
        {
            int minutes = Mathf.FloorToInt(data.playTime / 60f);
            int seconds = Mathf.FloorToInt(data.playTime % 60f);
            playTimeText.text = $"플레이 시간: {minutes:00}:{seconds:00}";
        }

        if (goldText != null)
        {
            int lost = data.goldBefore - data.goldAfter;
            goldText.text = $"골드: {data.goldBefore}G → {data.goldAfter}G (-{lost}G)";
        }

        if (itemText != null)
        {
            int lost = data.itemsBefore - data.itemsAfter;
            itemText.text = $"아이템: {data.itemsBefore}개 → {data.itemsAfter}개 (-{lost}개)";
        }
    }

    private void OnClickReturn()
    {
        Time.timeScale = 1f;
        BaseInteractable.IsUIOpen = false;
        var playerInput = FindFirstObjectByType<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) playerInput.ActivateInput();
        gameOverPanel.SetActive(false);
        if (SceneController.Instance != null)
            SceneController.Instance.LoadBase();
    }
}