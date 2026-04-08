using UnityEngine;

// 역할: 모루 강화 Canvas UI

public class AnvilCanvasUI : BaseCanvasUI
{
    protected override void OnOpen()
    {
        Debug.Log("[AnvilCanvasUI] 모루 UI 열림");
    }

    protected override void OnClose()
    {
        Debug.Log("[AnvilCanvasUI] 모루 UI 닫힘");
    }
}
