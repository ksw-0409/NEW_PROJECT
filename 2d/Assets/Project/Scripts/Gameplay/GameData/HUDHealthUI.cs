using UnityEngine;
using TMPro;

// 역할: 플레이어 현재 체력 / 최대 체력 표시

public class HUDHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private PlayerStats playerStats;

    void Update()
    {
        if (playerStats == null || healthText == null) return;
        healthText.text = $"{Mathf.Max(0, (int)playerStats.currentHealth)} / {(int)playerStats.MaxHealth}";
    }
}