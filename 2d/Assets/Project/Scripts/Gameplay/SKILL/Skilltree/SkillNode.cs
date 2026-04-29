using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillNode : MonoBehaviour, IPointerClickHandler
{

    [Header("스킬 기본 정보")]
    public string skillName;
    public int level;

    [Header("선행 스킬")]
    public SkillNode[] prerequisites;
    public SkillConnector[] outgoingLinks;

    [Header("시각적 상태")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.yellow;
    public Image iconImage;

    [Header("강화 비용")]
    [SerializeField] private int unlockCost = 0; // 일반 재화 소모량

    // 이 부분이 정확히 있어야 SkillConnector에서 에러가 안 납니다.
    public bool IsUnlocked { get; private set; } = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (GameDataManager.Instance != null && GameDataManager.Instance.IsNodeUnlocked(skillName))
        {
            IsUnlocked = true;
        }

        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SkillTreeUI.Instance.SelectNode(this);
    }

    public void TryUnlock()
    {
        if (!CanUnlock()) return;

        if (!GameDataManager.Instance.SpendNormalCurrency(unlockCost))
        {
            Debug.Log($"[SkillNode] 일반 재화가 부족합니다. (필요: {unlockCost})");
            return;
        }

        IsUnlocked = true;

        GameDataManager.Instance.SaveUnlockedNode(skillName);

        UpdateVisual();

        foreach (var link in outgoingLinks)
        {
            if (link != null) link.RefreshColor();
        }
    }

    private void UpdateVisual()
    {
        if (iconImage == null) return;

        if (isSelected)
        {
            iconImage.color = Color.green; // 선택된 노드
        }
        else
        {
            iconImage.color = IsUnlocked ? unlockedColor : lockedColor;
        }
    }

    public bool CanUnlock()
    {
        if (IsUnlocked) return false;

        foreach (var pre in prerequisites)
        {
            if (!pre.IsUnlocked)
                return false;
        }

        return true;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }
}
