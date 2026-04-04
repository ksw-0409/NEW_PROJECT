using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    public static SkillTreeUI Instance;

    public SkillNode selectedNode;
    public Button upgradeButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        upgradeButton.onClick.AddListener(OnClickUpgrade);
        upgradeButton.interactable = false;
    }

    public void SelectNode(SkillNode node)
    {
        // 이전 선택 해제
        if (selectedNode != null)
            selectedNode.SetSelected(false);

        // 새 선택
        selectedNode = node;
        selectedNode.SetSelected(true);

        // 버튼 활성화 여부
        upgradeButton.interactable = node.CanUnlock();
    }

    void OnClickUpgrade()
    {
        if (selectedNode == null) return;

        selectedNode.TryUnlock();

        // 선택 해제
        selectedNode.SetSelected(false);
        selectedNode = null;

        upgradeButton.interactable = false;
    }
}