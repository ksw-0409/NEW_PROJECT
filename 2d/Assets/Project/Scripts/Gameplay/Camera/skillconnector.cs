using UnityEngine;
using UnityEngine.UI.Extensions;

[ExecuteInEditMode] // 이 줄이 있어야 에디터에서도 선이 보입니다.
[RequireComponent(typeof(UILineRenderer))]
public class SkillConnector : MonoBehaviour
{
    public SkillNode fromNode;
    public SkillNode toNode;

    private UILineRenderer lineRenderer;

    private void OnEnable()
    {
        lineRenderer = GetComponent<UILineRenderer>();
        // 선이 클릭을 방해하지 않도록 설정
        lineRenderer.raycastTarget = false;
    }

    // Update 대신 UpdatePositions를 명확히 호출
    private void LateUpdate()
    {
        UpdatePositions();
    }

    public void UpdatePositions()
    {
        if (fromNode == null || toNode == null || lineRenderer == null) return;

        // 노드의 RectTransform 위치 가져오기
        Vector2 startPos = fromNode.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = toNode.GetComponent<RectTransform>().anchoredPosition;

        // Points 배열 업데이트 (Size를 2로 만듦)
        lineRenderer.Points = new Vector2[] { startPos, endPos };

        // 그래픽 갱신 강제 호출
        lineRenderer.SetAllDirty();
    }

    // 노드가 해제되었을 때 호출할 함수
    public void RefreshColor()
    {
        if (lineRenderer == null || fromNode == null) return;

        // 시작 노드가 해제되었다면 선 색상을 바꿈 (예: 주황색)
        lineRenderer.color = fromNode.IsUnlocked ? new Color(1f, 0.6f, 0f) : Color.gray;
        lineRenderer.SetAllDirty();
    }
}
