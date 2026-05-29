using UnityEngine;

/// <summary>
/// 쉴드 자동 재생 + 쉴드 보유 시 HP 회복 (신성한 영역 — 사용자 요청으로 통합).
/// </summary>
public class PlayerShieldRegen : MonoBehaviour
{
    [Header("쉴드 재생")]
    [Tooltip("쉴드가 빠진 후 새 쉴드가 생기기까지의 간격(초)")]
    public float regenInterval = 15f;

    [Tooltip("스킬트리 방어 패시브가 찍혀있을 때 패시브의 간격값으로 덮어씌울지")]
    public bool useShieldPassiveIfAvailable = true;

    [Header("신성한 영역 — 쉴드 보유 시 HP 회복")]
    [Tooltip("쉴드를 가지고 있는 동안 1초마다 최대체력의 몇 %를 회복할지")]
    [Range(0f, 0.2f)]
    public float sanctuaryHealRatio = 0.03f;  // 초당 최대 HP의 3%

    private float regenTimer = 0f;
    private float sanctuaryTimer = 0f;
    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null) return;

        // ===== 1) 쉴드 자동 재생 =====
        if (stats.currentShield < stats.maxShield)
        {
            float interval = regenInterval;
            if (useShieldPassiveIfAvailable && PassiveSystem.Instance != null)
            {
                int shieldLv = PassiveSystem.Instance.GetLevel(PassiveSystem.ID_SHIELD);
                if (shieldLv > 0)
                {
                    float passiveInterval = PassiveSystem.Instance.GetBonus(PassiveSystem.ID_SHIELD);
                    if (passiveInterval > 0f) interval = passiveInterval;
                }
            }
            regenTimer += Time.deltaTime;
            if (regenTimer >= interval)
            {
                regenTimer = 0f;
                stats.AddShield();
            }
        }
        else
        {
            regenTimer = 0f;
        }

        // ===== 2) 신성한 영역: 쉴드 가지고 있으면 초당 HP 회복 =====
        if (stats.currentShield > 0)
        {
            sanctuaryTimer += Time.deltaTime;
            if (sanctuaryTimer >= 1f)
            {
                sanctuaryTimer = 0f;
                float maxHp = stats.MaxHealth;
                float heal = maxHp * sanctuaryHealRatio;
                if (heal > 0f && stats.currentHealth < maxHp)
                {
                    stats.currentHealth = Mathf.Min(stats.currentHealth + heal, maxHp);
                    Debug.Log($"<color=#88ccff>[신성한 영역]</color> 쉴드 보유 중 → +{heal:F1} HP ({stats.currentHealth:F0}/{maxHp:F0})");
                }
            }
        }
        else
        {
            sanctuaryTimer = 0f;
        }
    }
}
