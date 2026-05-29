using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패시브 스킬 시스템.
/// 각 패시브는 Lv.1~Lv.6까지 단계적 강화되며 PlayerStats에 곱셈 보너스로 적용됩니다.
///
/// 사용 방법:
///   - 캐릭터(Player)에 PassiveSystem 컴포넌트를 붙임 (또는 PlayerStats가 자동 추가)
///   - PassiveNode가 해금 시 PassiveSystem.SetLevel(id, level) 호출
///   - PlayerStats의 property가 PassiveSystem.GetMultiplier(...)를 곱해서 반환
///
/// 영구 저장: GameDataManager의 PersistentData에 저장됨
/// </summary>
public class PassiveSystem : MonoBehaviour
{
    public static PassiveSystem Instance;

    // 패시브 ID
    public const string ID_HP        = "passive_hp";
    public const string ID_DEFENSE   = "passive_defense";
    public const string ID_PHYS_DMG  = "passive_phys_dmg";
    public const string ID_MAG_DMG   = "passive_mag_dmg";
    public const string ID_SPEED     = "passive_speed";
    public const string ID_EXP       = "passive_exp";
    public const string ID_GOLD      = "passive_gold";
    public const string ID_CRIT      = "passive_crit";
    public const string ID_SHIELD    = "passive_shield";
    // 디버프
    public const string ID_DEBT_MARK = "passive_debt_mark"; // 채무자의 낙인

    // 6레벨 보너스 테이블 (퍼센트, 1.0 = +100%)
    // index 0 = Lv.1, ... index 5 = Lv.6
    public static readonly Dictionary<string, float[]> LevelBonusTable = new Dictionary<string, float[]>{
        // 생명력: +10/20/30/40/50/70 %
        { ID_HP,       new[] { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 0.70f } },
        // 방어력 감소율: +5/10/15/20/25/40 %
        { ID_DEFENSE,  new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.40f } },
        // 물리 데미지: +10/20/30/40/50/100 %
        { ID_PHYS_DMG, new[] { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 1.00f } },
        // 마법 데미지: +10/20/30/40/50/100 %
        { ID_MAG_DMG,  new[] { 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 1.00f } },
        // 이동속도: +10/15/20/25/30/50 %
        { ID_SPEED,    new[] { 0.10f, 0.15f, 0.20f, 0.25f, 0.30f, 0.50f } },
        // 경험치: +10/25/40/55/70/100 %
        { ID_EXP,      new[] { 0.10f, 0.25f, 0.40f, 0.55f, 0.70f, 1.00f } },
        // 골드: +10/25/40/60/80/100 %
        { ID_GOLD,     new[] { 0.10f, 0.25f, 0.40f, 0.60f, 0.80f, 1.00f } },
        // 치명타: +5/10/15/20/25/30 %
        { ID_CRIT,     new[] { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.30f } },
        // 쉴드 쿨다운(초): 90/85/80/70/60/50  — 값이 작아질수록 좋음
        { ID_SHIELD,   new[] { 90f, 85f, 80f, 70f, 60f, 50f } },
        // 채무자 낙인: 강도 (페널티 비율, 활성 시 사용)
        { ID_DEBT_MARK, new[] { 0.30f, 0.30f, 0.30f, 0.30f, 0.30f, 0.30f } }, // 단일 효과, 레벨 1만 사용
    };

    // 현재 각 패시브의 레벨 (0 = 미해금, 1~6 = 해금됨)
    private Dictionary<string, int> levels = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        // ⭐ 플레이어 사망 시 패시브 전부 초기화 (런 한정 — 스킬카드 방식)
        PlayerStats.OnPlayerDied += ResetAll;
    }

    void OnDestroy()
    {
        PlayerStats.OnPlayerDied -= ResetAll;
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        // ⭐ 패시브는 '런(run) 한정' — 죽으면 사라지는 스킬카드 방식이므로
        // 영구 저장값을 복원하지 않는다 (항상 0에서 시작).
    }

    /// <summary>패시브의 현재 레벨 (0 = 미해금).</summary>
    public int GetLevel(string passiveID)
    {
        return levels.TryGetValue(passiveID, out int lv) ? lv : 0;
    }

    /// <summary>패시브 레벨 설정 (해금/레벨업 시 호출).</summary>
    public void SetLevel(string passiveID, int level)
    {
        level = Mathf.Clamp(level, 0, 6);
        levels[passiveID] = level;
        // 영구 저장 안 함 (런 한정 패시브)

        Debug.Log($"<color=lime>[Passive]</color> {passiveID} → Lv.{level}");
    }

    /// <summary>다음 레벨로 강화 (현재 0이면 1, 5면 6, 6이면 이미 최대).</summary>
    public bool LevelUp(string passiveID)
    {
        int cur = GetLevel(passiveID);
        if (cur >= 6) return false;
        SetLevel(passiveID, cur + 1);
        return true;
    }

    /// <summary>현재 레벨에 해당하는 보너스 값을 반환 (0레벨이면 0).</summary>
    public float GetBonus(string passiveID)
    {
        int lv = GetLevel(passiveID);
        if (lv <= 0) return 0f;
        if (!LevelBonusTable.TryGetValue(passiveID, out var table)) return 0f;
        int idx = Mathf.Clamp(lv - 1, 0, table.Length - 1);
        return table[idx];
    }

    /// <summary>PlayerStats의 곱셈 계수로 쓰는 헬퍼 (1 + bonus). 0레벨이면 1.0 반환.</summary>
    public float GetMultiplier(string passiveID)
    {
        return 1f + GetBonus(passiveID);
    }

    /// <summary>채무자 낙인 활성 여부 (일반 재화가 - 일 때).</summary>
    public bool IsDebtMarkActive
    {
        get
        {
            // 채무자 낙인은 일반 재화가 음수일 때 항상 자동 발동
            // (노드 해금 조건 제거 — 사용자 의도에 따라)
            if (GameDataManager.Instance == null) return false;
            return GameDataManager.Instance.NormalCurrency < 0;
        }
    }

    /// <summary>채무자 낙인의 데미지 페널티 곱셈 계수 (활성 시 0.7).</summary>
    public float DebtDamageMultiplier => IsDebtMarkActive ? 0.7f : 1.0f;

    /// <summary>채무자 낙인의 이동속도 페널티 곱셈 계수 (활성 시 0.85).</summary>
    public float DebtSpeedMultiplier => IsDebtMarkActive ? 0.85f : 1.0f;

    // ========= 저장/복원 =========
    private void SaveToPersistent()
    {
        if (GameDataManager.Instance == null) return;
        GameDataManager.Instance.SavePassiveLevels(levels);
    }

    /// <summary>모든 패시브 레벨 초기화 (캐릭터 사망 시 호출). 영구저장본도 비움.</summary>
    public void ResetAll()
    {
        levels.Clear();
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SavePassiveLevels(new Dictionary<string, int>());
        Debug.Log("<color=orange>[PassiveSystem]</color> 모든 패시브 초기화 (사망)");
        // ⭐ 모든 스킬트리 UI 노드 갱신 (데이터만 비우면 화면이 안 바뀜)
        var nodes = UnityEngine.Object.FindObjectsByType<PassiveSkillNode>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None);
        foreach (var n in nodes)
        {
            if (n != null) n.UpdateVisual();
        }
        Debug.Log($"<color=orange>[PassiveSystem]</color> 노드 UI {nodes.Length}개 갱신");
    }

    private void RestoreFromSave()
    {
        if (GameDataManager.Instance == null) return;
        var saved = GameDataManager.Instance.GetPassiveLevels();
        if (saved == null) return;
        foreach (var kv in saved) levels[kv.Key] = kv.Value;
        Debug.Log($"<color=cyan>[PassiveSystem]</color> restored {levels.Count} passive levels");
    }
}