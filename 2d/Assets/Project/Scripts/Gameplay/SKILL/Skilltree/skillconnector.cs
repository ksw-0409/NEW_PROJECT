using UnityEngine;
using UnityEngine.UI.Extensions;

[ExecuteAlways]
[RequireComponent(typeof(UILineRenderer))]
public class SkillConnector : MonoBehaviour
{
    public SkillNode fromNode;
    public SkillNode toNode;

    private UILineRenderer line;

    void OnEnable()
    {
        line = GetComponent<UILineRenderer>();
        if (line != null) line.raycastTarget = false;
        RefreshColor();
        UpdateLine();
    }

    void LateUpdate() => UpdateLine();

    public void UpdateLine()
    {
        if (line == null) line = GetComponent<UILineRenderer>();
        if (line == null || fromNode == null || toNode == null) return;

        var fromR = fromNode.GetComponent<RectTransform>();
        var toR   = toNode  .GetComponent<RectTransform>();
        if (fromR == null || toR == null) return;

        line.Points = new Vector2[] { fromR.anchoredPosition, toR.anchoredPosition };
        line.SetAllDirty();
    }

    public void RefreshColor()
    {
        if (line == null) line = GetComponent<UILineRenderer>();
        if (line == null || fromNode == null) return;

        if (fromNode.IsUnlocked && toNode != null && toNode.IsUnlocked)
            line.color = new Color(1f,  0.82f, 0.1f, 1f);   // 둘 다 해금: 황금
        else if (fromNode.IsUnlocked)
            line.color = new Color(0.4f, 0.8f,  1f,  0.9f); // from만 해금: 하늘
        else
            line.color = new Color(0.3f, 0.3f,  0.35f, 0.6f); // 잠금: 회색

        line.SetAllDirty();
    }
}