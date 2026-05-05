using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 역할: 게임 오버 패널 UI
// 사망 층, 플레이 시간, 패널티 표시, 거점 돌아가기 버튼

public class GameOverUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI floorText;       // 몇 층에서 사망
    [SerializeField] private TextMeshProUGUI playTimeText;    // 총 플레이 시간
    [SerializeField] private TextMeshProUGUI goldText;        // 골드 패널티
    [SerializeField] private TextMeshProUGUI itemText;        // 아이템 패널티
    [SerializeField] private Button returnButton;             // 거점 돌아가기 버튼
    [SerializeField] private GameObject gameOverPanel;

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

        // 게임 일시 정지
        Time.timeScale = 0f;

        // 층 표시
        if (floorText != null)
            floorText.text = $"{data.floor}층에서 사망";

        // 플레이 시간 표시
        if (playTimeText != null)
        {
            int minutes = Mathf.FloorToInt(data.playTime / 60f);
            int seconds = Mathf.FloorToInt(data.playTime % 60f);
            playTimeText.text = $"플레이 시간: {minutes:00}:{seconds:00}";
        }

        // 골드 패널티 표시
        if (goldText != null)
        {
            int lost = data.goldBefore - data.goldAfter;
            goldText.text = $"골드: {data.goldBefore}G → {data.goldAfter}G (-{lost}G)";
        }

        // 아이템 패널티 표시
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