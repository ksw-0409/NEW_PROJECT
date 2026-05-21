using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 패시브 스킬 노드 — 단일 노드에서 Lv.0~Lv.6까지 누적 강화.
/// 노드 아이콘 옆에 "Lv.N/6" 같은 레벨 카운터를 표시.
///
/// SkillTreeUI에서 클릭하면 InfoPanel에 정보 표시 → [강화] 버튼으로 다음 레벨 활성화.
/// PassiveSystem.LevelUp(passiveID) 호출.
/// </summary>
[RequireComponent(typeof(Image))]
public class PassiveSkillNode : MonoBehaviour
{
    [Header("패시브 정보")]
    [Tooltip("PassiveSystem.ID_XXX 와 일치해야 함")]
    public string passiveID;

    public string nodeName;
    [TextArea] public string nodeDescription;

    [Header("비주얼")]
    public Sprite iconSprite;
    public Image iconImage;

    [Tooltip("자동 생성됨 — 비워두면 노드 우상단에 'Lv.N/6' 카운터 자동 추가")]
    public TMP_Text levelText;

    [Header("강화 비용")]
    public int unlockCostPerLevel = 1;

    /// <summary>현재 레벨 (0 = 미해금).</summary>
    public int CurrentLevel => PassiveSystem.Instance != null ? PassiveSystem.Instance.GetLevel(passiveID) : 0;

    /// <summary>최대 레벨 도달 여부.</summary>
    public bool IsMaxLevel => CurrentLevel >= 6;

    void Start()
    {
        EnsureLevelText();
        UpdateVisual();
    }

    void OnEnable()
    {
        EnsureLevelText();
        UpdateVisual();
    }

    /// <summary>강화 가능 여부 (재화 충분 + 최대 레벨 미만).</summary>
    public bool CanLevelUp()
    {
        if (IsMaxLevel) return false;
        if (GameDataManager.Instance == null) return false;
        return GameDataManager.Instance.NormalCurrency >= unlockCostPerLevel;
    }

    /// <summary>강화 실행 (재화 차감 + 레벨업).</summary>
    public bool TryLevelUp()
    {
        if (IsMaxLevel)
        {
            Debug.Log($"<color=yellow>[PassiveNode]</color> {passiveID} 이미 최대 레벨");
            return false;
        }
        if (PassiveSystem.Instance == null)
        {
            Debug.LogError("[PassiveNode] PassiveSystem.Instance == null. Player에 컴포넌트가 붙어있는지 확인.");
            return false;
        }

#if UNITY_EDITOR
        // 에디터: 재화 체크 우회하여 자유롭게 테스트 가능
        if (GameDataManager.Instance != null && GameDataManager.Instance.NormalCurrency >= unlockCostPerLevel)
            GameDataManager.Instance.SpendNormalCurrency(unlockCostPerLevel);
#else
        if (GameDataManager.Instance == null) return false;
        if (!GameDataManager.Instance.SpendNormalCurrency(unlockCostPerLevel))
        {
            Debug.LogWarning($"[PassiveNode] {passiveID} 강화 실패: 재화 부족");
            return false;
        }
#endif

        PassiveSystem.Instance.LevelUp(passiveID);
        UpdateVisual();
        Debug.Log($"<color=lime>[PassiveNode]</color> {passiveID} → Lv.{CurrentLevel}");
        return true;
    }

    /// <summary>현재 레벨의 효과 문자열.</summary>
    public string GetCurrentLevelEffectText()
    {
        int lv = CurrentLevel;
        if (lv <= 0) return "현재: 미해금";

        float bonus = PassiveSystem.Instance.GetBonus(passiveID);
        return $"현재 Lv.{lv}: +{(bonus * 100f):F0}%";
    }

    /// <summary>다음 레벨의 효과 문자열.</summary>
    public string GetNextLevelEffectText()
    {
        if (IsMaxLevel) return "[최대 레벨]";
        int nextLv = CurrentLevel + 1;
        if (!PassiveSystem.LevelBonusTable.TryGetValue(passiveID, out var table)) return "";
        int idx = Mathf.Clamp(nextLv - 1, 0, table.Length - 1);
        float nextBonus = table[idx];
        return $"다음 Lv.{nextLv}: +{(nextBonus * 100f):F0}%";
    }

    /// <summary>레벨 텍스트가 없으면 자동으로 노드 우상단에 만들어줌.</summary>
    void EnsureLevelText()
    {
        if (levelText != null) return;

        // 기존 자식 중 LevelBadge가 있는지
        var existing = transform.Find("LevelBadge");
        if (existing != null)
        {
            levelText = existing.GetComponent<TMP_Text>();
            if (levelText != null) return;
        }

        // 새로 생성 — 노드 우상단 모서리에 작은 배지로
        var badgeGo = new GameObject("LevelBadge");
        var rt = badgeGo.AddComponent<RectTransform>();
        rt.SetParent(transform, false);
        rt.anchorMin = new Vector2(1f, 1f);     // 우상단 기준
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(8f, 8f); // 살짝 노드 밖으로 튀어나옴
        rt.sizeDelta = new Vector2(40f, 22f);

        // 어두운 배경 (작은 패널)
        var bgImg = badgeGo.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);
        bgImg.raycastTarget = false;

        // 황금 테두리 효과는 Outline 컴포넌트로
        var outline = badgeGo.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.65f, 0.52f, 0.18f, 1f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        // 텍스트 자식
        var textGo = new GameObject("Text");
        var trt = textGo.AddComponent<RectTransform>();
        trt.SetParent(badgeGo.transform, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "Lv.0/6";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 11;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.85f, 0.3f, 1f);
        tmp.raycastTarget = false;
        levelText = tmp;
    }

    public void UpdateVisual()
    {
        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.color = CurrentLevel > 0 ? Color.white : new Color(0.55f, 0.55f, 0.55f, 1f);
        }
        if (levelText != null)
        {
            int lv = CurrentLevel;
            levelText.text = $"Lv.{lv}/6";
            levelText.color = IsMaxLevel
                ? new Color(1f, 0.85f, 0.2f)        // 최대: 황금
                : (lv > 0 ? Color.white              // 강화 중: 흰색
                          : new Color(0.6f, 0.6f, 0.65f)); // 미해금: 회색
        }
    }
}
