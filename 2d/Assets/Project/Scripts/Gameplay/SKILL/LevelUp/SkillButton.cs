using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

public class SkillButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image iconImage;

    private SkillData assignedData;
    private LevelUpManager manager;

    public void Setup(SkillData data, LevelUpManager mgr)
    {
        assignedData = data;
        manager = mgr;

        nameText.text = data.skillName;
        descText.text = data.description;
     //   iconImage.sprite = data.icon;
    }


    // 버튼의 OnClick 이벤트에 연결할 함수
    public void OnClick()
    {
        Debug.Log("Click");
        manager.OnSkillSelected(assignedData);
    }
}