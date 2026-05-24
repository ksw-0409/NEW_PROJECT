using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameOverUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI monstersKilledText;
    [SerializeField] private TextMeshProUGUI totalDamageText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button returnButton;
    [SerializeField] private GameObject gameOverPanel;

    [Header("아이템 그리드")]
    [SerializeField] private Image[] itemIcons; // 28개 Image 배열
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("애니메이션")]
    [SerializeField] private Animator gameOverAnimator;
    [SerializeField] private string openAnimName = "GameOverIntro";
    [SerializeField] private GameObject[] uiElementsToShow;

    private Coroutine showUICoroutine;
    private GameOverData cachedData;

    void OnEnable() { GameOverManager.OnGameOver += ShowGameOver; }
    void OnDisable() { GameOverManager.OnGameOver -= ShowGameOver; }

    void Start()
    {
        returnButton.onClick.AddListener(OnClickReturn);
        gameOverPanel.SetActive(false);
    }

    private void ShowGameOver(GameOverData data)
    {
        cachedData = data;
        gameOverPanel.SetActive(true);
        gameOverAnimator.enabled = true;
        Time.timeScale = 0f;

        SetupTexts(data);
        SetUIVisible(false);

        if (showUICoroutine != null) StopCoroutine(showUICoroutine);
        showUICoroutine = StartCoroutine(PlayAnimAndShowUI());
    }

    private IEnumerator PlayAnimAndShowUI()
    {
        gameOverAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        yield return null;
        gameOverAnimator.Play(openAnimName, 0, 0f);

        float clipLength = GetClipLength(openAnimName);
        yield return new WaitForSecondsRealtime(clipLength);

        gameOverAnimator.enabled = false;

        // 0.5초 간격으로 순차 표시
        foreach (var obj in uiElementsToShow)
        {
            if (obj != null) obj.SetActive(true);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        BuildItemGrid(cachedData);
    }

    // ─── 아이템 그리드 ───────────────────────────────
    private void BuildItemGrid(GameOverData data)
    {
        if (itemIcons == null) return;

        // 전체 초기화 (알파 0)
        foreach (var icon in itemIcons)
            if (icon != null) icon.color = new Color(1f, 1f, 1f, 0f);

        if (data.allItems == null) return;

        for (int i = 0; i < data.allItems.Count && i < itemIcons.Length; i++)
        {
            var item = data.allItems[i];
            var icon = itemIcons[i];
            if (icon == null) continue;

            icon.sprite = item.iconSprite;
            icon.color = Color.white;

            // 사라지는 아이템은 페이드아웃
            if (data.lostItems != null && data.lostItems.Contains(item))
                StartCoroutine(FadeOut(icon, fadeDuration));
        }
    }

    private IEnumerator FadeOut(Image icon, float duration)
    {
        yield return new WaitForSecondsRealtime(0.5f); // 잠깐 보여주고 사라짐

        float elapsed = 0f;
        Color start = icon.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / duration);
            icon.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
        icon.color = new Color(start.r, start.g, start.b, 0f);
    }

    // ─── 텍스트 ──────────────────────────────────────
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

        if (monstersKilledText != null)
            monstersKilledText.text = $"처치한 몬스터: {data.monstersKilled}마리";

        if (totalDamageText != null)
            totalDamageText.text = $"총 데미지: {data.totalDamageDealt:F0}";

        if (goldText != null)
        {
            int lost = data.goldBefore - data.goldAfter;
            goldText.text = $"골드: {data.goldBefore}G → {data.goldAfter}G (-{lost}G)";
        }

    }

    private void SetUIVisible(bool visible)
    {
        foreach (var obj in uiElementsToShow)
            obj.SetActive(visible);
    }

    private float GetClipLength(string clipName)
    {
        foreach (var clip in gameOverAnimator.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        Debug.LogWarning($"클립 '{clipName}'을 찾을 수 없습니다.");
        return 1f;
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