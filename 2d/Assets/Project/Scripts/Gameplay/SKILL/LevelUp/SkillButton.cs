using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; // DOTween 사용

public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image iconImage;
    public TextMeshProUGUI levelText;

    private SkillData data;              // 스킬 카드일 때
    private PassiveCardData passiveData; // 패시브 카드일 때
    private LevelUpManager manager;

    public Image[] Gems;
    public Image MaxGems;
    public Image Card;

    private Vector3 originalScale;
    public float hoverScale = 1.1f; // 마우스 올렸을 때 커질 배율

    void Start()
    {
        originalScale = transform.localScale;
    }

    // ===================== 스킬 카드 =====================
    public void Setup(SkillData data, LevelUpManager mgr)
    {
        if (data == null) { Debug.LogError("SkillButton: data가 null임"); return; }
        if (mgr == null) { Debug.LogError("SkillButton: manager가 null임"); return; }

        this.data = data;
        this.passiveData = null; // 스킬 카드이므로 패시브 비움
        this.manager = mgr;

        if (nameText != null) nameText.text = data.skillName;
        else Debug.LogError("nameText 연결 안됨");

        if (descText != null) descText.text = data.description;
        else Debug.LogError("descText 연결 안됨");

        if (iconImage != null) iconImage.sprite = data.icon;
        if (Card != null && data.card != null) Card.sprite = data.card;

        if (data.gem != null)
        {
            for (int i = 0; i < Gems.Length; i++) Gems[i].sprite = data.gem;
        }
        if (MaxGems != null && data.maxGem != null) MaxGems.sprite = data.maxGem;

        PlayerSkillController skillController = FindObjectOfType<PlayerSkillController>();
        if (skillController == null) { Debug.LogError("PlayerSkillController 못찾음"); return; }
        if (levelText == null) { Debug.LogError("levelText 연결 안됨"); return; }

        if (skillController.HasSkill(data))
        {
            int level = skillController.GetSkillLevel(data);
            if (skillController.IsMaxLevel(data))
            {
                levelText.text = "MAX";
                MaxGems.enabled = true;
            }
            else
            {
                levelText.text = "Lv." + level;
                MaxGems.enabled = false;
                for (int i = 0; i <= level && i < Gems.Length; i++) Gems[i].enabled = true;
                for (int i = level; i < Gems.Length; i++) Gems[i].enabled = false;
            }
        }
        else
        {
            levelText.text = "NEW";
            for (int i = 0; i < Gems.Length; i++)
            {
                Gems[i].enabled = false;
                MaxGems.enabled = false;
            }
        }
    }

    // ===================== 패시브 카드 =====================
    public void Setup(PassiveCardData passive, LevelUpManager mgr)
    {
        if (passive == null) { Debug.LogError("SkillButton: passive가 null임"); return; }
        if (mgr == null) { Debug.LogError("SkillButton: manager가 null임"); return; }

        this.passiveData = passive;
        this.data = null; // 패시브 카드이므로 스킬 비움
        this.manager = mgr;

        if (nameText != null) nameText.text = passive.cardName;
        if (descText != null) descText.text = passive.description;
        if (iconImage != null) iconImage.sprite = passive.icon;
        if (Card != null && passive.card != null) Card.sprite = passive.card;

        if (passive.gem != null)
        {
            for (int i = 0; i < Gems.Length; i++) Gems[i].sprite = passive.gem;
        }
        if (MaxGems != null && passive.maxGem != null) MaxGems.sprite = passive.maxGem;

        if (levelText == null) { Debug.LogError("levelText 연결 안됨"); return; }

        int level = passive.CurrentLevel; // 0~6
        if (passive.IsMaxLevel)
        {
            levelText.text = "MAX";
            if (MaxGems != null) MaxGems.enabled = true;
            for (int i = 0; i < Gems.Length; i++) Gems[i].enabled = false;
        }
        else if (level <= 0)
        {
            levelText.text = "NEW";
            for (int i = 0; i < Gems.Length; i++) Gems[i].enabled = false;
            if (MaxGems != null) MaxGems.enabled = false;
        }
        else
        {
            levelText.text = "Lv." + level;
            if (MaxGems != null) MaxGems.enabled = false;
            for (int i = 0; i < level && i < Gems.Length; i++) Gems[i].enabled = true;
            for (int i = level; i < Gems.Length; i++) Gems[i].enabled = false;
        }
    }

    public void OnClick()
    {
        if (manager == null) { Debug.LogError("버튼 클릭 실패: manager null"); return; }
        if (data == null && passiveData == null) { Debug.LogError("버튼 클릭 실패: data/passive 모두 null"); return; }

        transform.DOScale(originalScale * 1.5f, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (passiveData != null)
                    manager.OnPassiveSelected(passiveData);
                else
                    manager.OnSkillSelected(data);
            });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(originalScale * hoverScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
    }
}
