using UnityEngine;
using UnityEngine.UI;

// 역할: 플레이어 경험치를 Image Fill Amount로 표시


public class ExpBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private PlayerStats playerStats;

    private const float expToLevelUp = 50f; // 임시 하드코딩

    void Update()
    {
        if (fillImage == null || playerStats == null) return;

        fillImage.fillAmount = Mathf.Clamp01(playerStats.currentExp / expToLevelUp);
    }
}
