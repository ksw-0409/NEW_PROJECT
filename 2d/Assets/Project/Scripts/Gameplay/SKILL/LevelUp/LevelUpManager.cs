using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelUpManager : MonoBehaviour
{
    public List<SkillData> allSkills;        // 전체 스킬 풀
    [Header("패시브 카드 풀 (화살 제외: hp/defense/phys/mag/speed/exp/gold/crit)")]
    public List<PassiveCardData> allPassives; // 전체 패시브 카드 풀
    public SkillButton[] uiButtons;          // UI 버튼 3개
    public GameObject levelUpUI;             // UI 패널
    public PlayerStats playerStats;
    public PlayerSkillController skillController;

    private bool startUIShown = false;

    void Start()
    {
        var bsm = FindFirstObjectByType<BossStageManager>();
        bool isBossMode = bsm != null && bsm.gameObject.activeInHierarchy;
        int floor = GameDataManager.Instance.CurrentFloor;
        Debug.Log($"[LevelUpManager] Start - CurrentFloor={floor} startUIShown={startUIShown} isBossMode={isBossMode}");

        bool shouldShow = !startUIShown && floor == 1 && !isBossMode;
        if (shouldShow)
        {
            startUIShown = true;
            ShowStartSkillUI();
        }
        else if (startUIShown)
        {
            Debug.Log("[LevelUpManager] Skipping start UI - already shown this session");
        }
    }

    private void OnEnable()
    {
        playerStats.OnLevelUp += ShowLevelUpUI;
    }

    private void OnDisable()
    {
        playerStats.OnLevelUp -= ShowLevelUpUI;
    }

    // 🟢 시작 스킬 선택 (시작창에서는 스킬만 — 무기를 먼저 골라야 하므로)
    public void ShowStartSkillUI()
    {
        Debug.Log("[LevelUpManager] ShowStartSkillUI invoked");
        levelUpUI.SetActive(true);
        Time.timeScale = 0f;
        ShowRandomCards(includePassives: false);
    }

    // 🟢 레벨업 시 호출 (스킬 + 패시브 섞어서 등장)
    public void ShowLevelUpUI()
    {
        Debug.Log("[LevelUpManager] ShowLevelUpUI invoked (from level up event)");
        levelUpUI.SetActive(true);
        Time.timeScale = 0f;
        ShowRandomCards(includePassives: true);
    }

    // 🎯 핵심 랜덤 로직 (스킬 + 패시브 통합 풀)
    private void ShowRandomCards(bool includePassives)
    {
        // ---- 스킬 후보 ----
        bool hasBowSkill = allSkills.OfType<BowSkillData>().Any(bowData => skillController.HasSkill(bowData));

        var availableSkills = allSkills
            .Where(skill =>
            {
                if (skill == null) return false;
                if (skill is ArrowPassiveData || skill is ArrowRainData)
                {
                    if (!hasBowSkill) return false; // 활 없으면 화살계열 제외
                }
                if (!skillController.HasSkill(skill)) return true;
                return !skillController.IsMaxLevel(skill);
            })
            .Cast<object>()
            .ToList();

        // ---- 패시브 후보 ----
        var passivePool = new List<object>();
        if (includePassives && allPassives != null)
        {
            passivePool = allPassives
                .Where(p => p != null && !p.IsMaxLevel)  // 최대(6) 도달한 패시브는 제외
                .Cast<object>()
                .ToList();
        }

        if (availableSkills.Count == 0 && passivePool.Count == 0)
        {
            Debug.LogWarning("선택 가능한 카드 없음");
            CloseUI();
            return;
        }

        // 🎯 패시브 등장 확률을 높이는 가중치 뽑기
        //   - PASSIVE_GUARANTEE_CHANCE 확률로 첫 장을 패시브로 보장
        //   - 나머지는 스킬+패시브 혼합 풀에서 뽑되, 패시브를 PASSIVE_WEIGHT배 중복 투입해 가중치 부여
        const float PASSIVE_GUARANTEE_CHANCE = 0.45f; // 레벨업 시 약 45% 확률로 패시브 1장 보장
        const int PASSIVE_WEIGHT = 3;                 // 패시브를 풀에 3배 넣어 뽑힐 확률 상승

        var chosen = new List<object>();

        // 1) 패시브 1장 보장 (확률적으로)
        if (passivePool.Count > 0 && Random.value < PASSIVE_GUARANTEE_CHANCE)
        {
            var guaranteed = passivePool[Random.Range(0, passivePool.Count)];
            chosen.Add(guaranteed);
        }

        // 2) 가중치 풀 구성 (이미 뽑힌 건 제외)
        var weightedPool = new List<object>();
        foreach (var s in availableSkills)
            if (!chosen.Contains(s)) weightedPool.Add(s);
        foreach (var p in passivePool)
            if (!chosen.Contains(p))
                for (int w = 0; w < PASSIVE_WEIGHT; w++) weightedPool.Add(p); // 패시브 가중치

        // 3) 남은 자리 채우기 (중복 없이)
        weightedPool = weightedPool.OrderBy(x => Random.value).ToList();
        foreach (var item in weightedPool)
        {
            if (chosen.Count >= 3) break;
            if (!chosen.Contains(item)) chosen.Add(item);
        }

        // 최종 순서도 섞기 (보장 패시브가 항상 첫 칸에 오지 않도록)
        chosen = chosen.OrderBy(x => Random.value).Take(3).ToList();

        for (int i = 0; i < uiButtons.Length; i++)
        {
            if (i < chosen.Count)
            {
                uiButtons[i].gameObject.SetActive(true);
                if (chosen[i] is PassiveCardData passive)
                    uiButtons[i].Setup(passive, this);
                else if (chosen[i] is SkillData skill)
                    uiButtons[i].Setup(skill, this);
            }
            else
            {
                uiButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // 🟢 스킬 선택 시 호출
    public void OnSkillSelected(SkillData selected)
    {
        if (!skillController.HasSkill(selected))
            skillController.AddNewSkill(selected);
        else
            skillController.LevelUpSkill(selected);

        CloseUI();
    }

    // 🟢 패시브 선택 시 호출 — PassiveSystem 레벨 +1
    public void OnPassiveSelected(PassiveCardData selected)
    {
        if (selected == null) { CloseUI(); return; }
        selected.Apply(); // 내부에서 PassiveSystem.Instance.LevelUp(passiveID)
        CloseUI();
    }

    private void CloseUI()
    {
        levelUpUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnPlayerDie()
    {
        skillController.ResetSkills();
        // ⭐ 패시브(골드/이속/HP 등)도 런 한정 → 사망 시 초기화
        if (PassiveSystem.Instance != null)
            PassiveSystem.Instance.ResetAll();
        Debug.Log("인게임 레벨 초기화. 스킬 트리 능력치는 보존됩니다.");
    }
}