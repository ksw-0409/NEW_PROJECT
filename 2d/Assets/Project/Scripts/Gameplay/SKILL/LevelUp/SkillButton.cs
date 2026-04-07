using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image iconImage;
    public TextMeshProUGUI levelText;

    private SkillData data;
    private LevelUpManager manager;

    public void Setup(SkillData data, LevelUpManager mgr)
    {
        // ⭐ null 방어 (이거 중요)
        if (data == null)
        {
            Debug.LogError("SkillButton: data가 null임");
            return;
        }

        if (mgr == null)
        {
            Debug.LogError("SkillButton: manager가 null임");
            return;
        }

        this.data = data;
        this.manager = mgr;

        // ⭐ 텍스트 null 방어
        if (nameText != null)
            nameText.text = data.skillName;
        else
            Debug.LogError("nameText 연결 안됨");

        if (descText != null)
            descText.text = data.description;
        else
            Debug.LogError("descText 연결 안됨");

        if (iconImage != null)
            iconImage.sprite = data.icon;

        // ⭐ SkillController 찾기
        PlayerSkillController skillController = FindObjectOfType<PlayerSkillController>();

        if (skillController == null)
        {
            Debug.LogError("PlayerSkillController 못찾음");
            return;
        }

        // ⭐ 레벨 표시
        if (levelText == null)
        {
            Debug.LogError("levelText 연결 안됨");
            return;
        }

        if (skillController.HasSkill(data))
        {
            int level = skillController.GetSkillLevel(data);

            if (skillController.IsMaxLevel(data))
                levelText.text = "MAX";
            else
                levelText.text = "Lv." + level;
        }
        else
        {
            levelText.text = "NEW";
        }
    }

    public void OnClick()
    {
        if (manager == null || data == null)
        {
            Debug.LogError("버튼 클릭 실패: manager 또는 data null");
            return;
        }

        manager.OnSkillSelected(data);
    }
}