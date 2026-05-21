using UnityEngine;

/// <summary>
/// 방어 패시브 — 일정 간격마다 PlayerStats에 쉴드 1개를 자동으로 추가.
/// 간격은 PassiveSystem의 ID_SHIELD 레벨에 따라 결정 (90→85→80→70→60→50초).
///
/// Player에 한 번 붙여놓으면, 패시브 레벨이 0보다 큰 동안 자동 작동.
/// </summary>
public class PlayerShieldRegen : MonoBehaviour
{
    private float timer = 0f;
    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (stats == null) return;
        if (PassiveSystem.Instance == null) return;

        int shieldLv = PassiveSystem.Instance.GetLevel(PassiveSystem.ID_SHIELD);
        if (shieldLv <= 0) return; // 패시브 미해금이면 작동 안 함

        // 이미 쉴드가 꽉 차 있으면 카운트 안 함
        if (stats.currentShield >= stats.maxShield)
        {
            timer = 0f;
            return;
        }

        // 쉴드 재생 간격 (레벨에 따라)
        float interval = PassiveSystem.Instance.GetBonus(PassiveSystem.ID_SHIELD); // 90/85/.../50초

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            stats.AddShield();
        }
    }
}
