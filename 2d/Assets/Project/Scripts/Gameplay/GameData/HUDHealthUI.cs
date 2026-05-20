using UnityEngine;
using UnityEngine.UI;

// 역할: 플레이어 체력바 (배경 이미지 + Fill 이미지)

public class HUDHealthUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // 체력 칸 이미지
    [SerializeField] private Image fillImage;       // 채우는 이미지
    [SerializeField] private PlayerStats playerStats;

    void Update()
    {
        if (playerStats == null || fillImage == null) return;

        fillImage.fillAmount = playerStats.MaxHealth > 0
            ? Mathf.Clamp01(playerStats.currentHealth / playerStats.MaxHealth)
            : 0f;
    }
}