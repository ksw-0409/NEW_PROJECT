using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillNode : MonoBehaviour, IPointerClickHandler
{
    public enum NodeType { StatBoost, Specialty }

    [Header("노드 기본 정보")]
    public string nodeName;
    [TextArea(2, 4)] public string nodeDescription;

    [Header("스킬 연결 정보")]
    public SkillData targetSkill;
    public NodeType nodeType;

    [Header("스탯 보너스")]
    public float damageMultiplier = 1.0f;
    public float rangeMultiplier = 1.0f;
    public float cooldownMultiplier = 1.0f;
    public int countBonus = 0;

    [Header("CC / 지속 보너스")]
    public float slowPercentMultiplier = 1f;
    public float durationMultiplier = 1f;

    [Header("특수 효과 (Specialty 노드)")]
    public string specialtyTag;

    [Header("노드 설정")]
    public string skillNodeID;
    public int unlockCost = 1;

    [Header("선행 조건")]
    public SkillNode[] prerequisites;
    public SkillConnector[] outgoingLinks;

    [Header("✨ 배타 제약 (3갈래 분기)")]
    [Tooltip("이 노드와 서로 배타적인 노드들. 이 노드가 해금되면 exclusiveWith의 모든 노드는 영원히 잠김됩니다.\n例: 공격/유틸/변칙 중 하나를 고르면 나머지 둘은 선택 불가.")]
    public SkillNode[] exclusiveWith;

    [Header("색상")]
    public Color lockedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color unlockedColor = new Color(1f, 0.85f, 0f, 1f);
    public Color selectedColor = new Color(0.2f, 1f, 0.2f, 1f);
    public Color availableColor = new Color(0.55f, 0.8f, 1f, 1f);

    [Header("UI")]
    public Image iconImage;

    [Header("✨ 아이콘 상태별 스프라이트")]
    [Tooltip("잠김 상태일 때 표시할 스프라이트 (회색). 비워두면 iconImage의 기본 sprite가 그대로 사용됩니다.")]
    public Sprite iconLocked;
    [Tooltip("해금되었거나 해금 가능한 상태일 때 표시할 스프라이트 (파란/컴러).")]
    public Sprite iconUnlocked;

    public Image frameImage;
    public TextMeshProUGUI costText;

    // ─── 상태 ───────────────────────────────
    public bool IsUnlocked { get; private set; } = false;
    private bool isSelected = false;

    // ─── 초기화 ─────────────────────────────
    void Awake()
    {
        if (GameDataManager.Instance != null
            && !string.IsNullOrEmpty(skillNodeID)
            && GameDataManager.Instance.IsNodeUnlocked(skillNodeID))
        {
            IsUnlocked = true;
        }

        UpdateVisual();
    }

    /// <summary>
    /// PlayerStats에 이 노드의 효과를 적용 (스킬 보너스 혹은 스폐셜티 태그 등록).
    /// Awake/Start 시점에서 재적용 — 이 메서드를 DoUnlock과 Start에서 모두 사용.
    /// </summary>
    private void ApplyUnlockEffectToStats()
    {
        if (PlayerStats.Instance == null) return;

        if (nodeType == NodeType.StatBoost)
        {
            if (targetSkill != null)
                PlayerStats.Instance.UpdateSkillBonus(targetSkill,
                    damageMultiplier, rangeMultiplier, cooldownMultiplier,
                    countBonus, slowPercentMultiplier, durationMultiplier);
        }
        else if (nodeType == NodeType.Specialty)
        {
            PlayerStats.Instance.UnlockSpecialty(specialtyTag, targetSkill,
                damageMultiplier, rangeMultiplier, cooldownMultiplier,
                countBonus, slowPercentMultiplier, durationMultiplier);
        }
    }

    void Start()
    {
        UpdateVisual();
        // ✨ 던전에 진입했을 때 PlayerStats에 재적용 (해금된 경우에만)
        if (IsUnlocked) ApplyUnlockEffectToStats();
    }

    // ─── 클릭 ───────────────────────────────
    public void OnPointerClick(PointerEventData e)
    {
        if (SkillTreeUI.Instance != null)
            SkillTreeUI.Instance.OnNodeSelected(this);
    }

    public void SetSelected(bool v) { isSelected = v; UpdateVisual(); }

    // ─── 해금 ───────────────────────────────
    public void TryUnlock()
    {
        if (!CanUnlock()) return;

        // ★ 테스트 모드: 재화 차감 없이 바로 해금
#if UNITY_EDITOR
        DoUnlock();
        return;
#endif
        if (GameDataManager.Instance != null
            && !GameDataManager.Instance.SpendGold(unlockCost))
        {
            Debug.Log("[SkillNode] 재화 부족");
            return;
        }
        DoUnlock();
    }

    private void DoUnlock()
    {
        IsUnlocked = true;

        if (!string.IsNullOrEmpty(skillNodeID) && GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SaveUnlockedNode(skillNodeID);

            var effect = new UnlockedNodeEffect
            {
                skillNodeID = skillNodeID,
                targetSkillAssetName = targetSkill != null ? targetSkill.name : "",
                nodeType = (int)nodeType,
                specialtyTag = specialtyTag,
                damageMultiplier = damageMultiplier,
                rangeMultiplier = rangeMultiplier,
                cooldownMultiplier = cooldownMultiplier,
                countBonus = countBonus,
                slowPercentMultiplier = slowPercentMultiplier,
                durationMultiplier = durationMultiplier
            };
            GameDataManager.Instance.SaveUnlockedEffect(effect);
        }

        ApplyUnlockEffectToStats();

        UpdateVisual();

        if (outgoingLinks != null)
            foreach (var link in outgoingLinks)
                if (link != null) link.RefreshColor();

        foreach (var node in FindObjectsOfType<SkillNode>())
            if (node.prerequisites != null)
                foreach (var pre in node.prerequisites)
                    if (pre == this) { node.UpdateVisual(); break; }

        if (exclusiveWith != null)
        {
            foreach (var other in exclusiveWith)
            {
                if (other == null) continue;
                other.UpdateVisual();
                foreach (var sub in FindObjectsOfType<SkillNode>())
                    if (sub.prerequisites != null)
                        foreach (var pre in sub.prerequisites)
                            if (pre == other) { sub.UpdateVisual(); break; }
            }
        }

        if (SkillTreeUI.Instance != null) SkillTreeUI.Instance.UpdateButtonState();
    }

    // ─── 조건 ───────────────────────────────
    public bool CanUnlock()
    {
        if (IsUnlocked) return false;
        if (IsBlockedByExclusive()) return false;
        if (prerequisites != null)
            foreach (var pre in prerequisites)
                if (pre != null && !pre.IsUnlocked) return false;
        return true;
    }

    public bool IsBlockedByExclusive()
    {
        if (exclusiveWith == null) return false;
        foreach (var other in exclusiveWith)
        {
            if (other != null && other.IsUnlocked) return true;
        }
        return false;
    }

    // ─── 시각 ───────────────────────────────
    public void UpdateVisual()
    {
        bool blocked = IsBlockedByExclusive();

        if (iconImage != null && (iconLocked != null || iconUnlocked != null))
        {
            Sprite chosen = IsUnlocked ? iconUnlocked : iconLocked;
            if (chosen != null) iconImage.sprite = chosen;

            if (isSelected)
                iconImage.color = new UnityEngine.Color(1.3f, 1.3f, 1.0f, 1f);
            else if (IsUnlocked)
                iconImage.color = UnityEngine.Color.white;
            else if (blocked)
                iconImage.color = new UnityEngine.Color(0.22f, 0.22f, 0.26f, 1f); // 영원 잠김 (거의 검정)
            else if (CanUnlock())
                iconImage.color = new UnityEngine.Color(0.62f, 0.62f, 0.62f, 1f); // 해금 가능 (은은한 회색, 미선택 시 불 꺼짐)1f);
            else
                iconImage.color = new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1f);
        }

        if (frameImage != null)
        {
            if (isSelected) frameImage.color = selectedColor;
            else if (IsUnlocked) frameImage.color = unlockedColor;
            else if (blocked) frameImage.color = new UnityEngine.Color(0.15f, 0.15f, 0.18f, 1f);
            else if (CanUnlock()) frameImage.color = availableColor;
            else frameImage.color = lockedColor;
        }
        else if (iconImage != null && iconLocked == null && iconUnlocked == null)
        {
            if (isSelected) iconImage.color = selectedColor;
            else if (IsUnlocked) iconImage.color = unlockedColor;
            else if (blocked) iconImage.color = new UnityEngine.Color(0.15f, 0.15f, 0.18f, 1f);
            else if (CanUnlock()) iconImage.color = availableColor;
            else iconImage.color = lockedColor;
        }

        if (costText != null)
            costText.text = IsUnlocked ? "✔" : (blocked ? "✕" : unlockCost.ToString());
    }
}