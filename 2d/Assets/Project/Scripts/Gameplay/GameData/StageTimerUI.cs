using UnityEngine;
using TMPro;

// 역할: 스테이지 남은 시간을 표시하는 UI
// StageManager에서 이벤트로 시간을 받아 표시

public class StageTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void OnEnable()
    {
        StageManager.OnTimerUpdated += RefreshTimer;
    }

    private void OnDisable()
    {
        StageManager.OnTimerUpdated -= RefreshTimer;
    }

    private void RefreshTimer(float remainingTime)
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}