using UnityEngine;
using UnityEngine.UI;

// 역할: 보스 체력바 UI
// BossStageManager에서 보스 스폰 후 SetTarget()으로 연결

public class BossHealthBarUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage; // 체력바 배경
    [SerializeField] private Image fillImage;       // 채우는 이미지
    private EnemyHealth bossHealth;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    /// <summary>보스 스폰 후 BossStageManager에서 호출</summary>
    public void SetTarget(EnemyHealth health)
    {
        bossHealth = health;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (bossHealth == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(bossHealth.currentHp / bossHealth.MaxHp);
    }
}