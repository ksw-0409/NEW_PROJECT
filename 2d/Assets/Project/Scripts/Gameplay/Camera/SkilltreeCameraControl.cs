using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreeCameraControl : MonoBehaviour
{
    [SerializeField] private RectTransform contentRect;

    [Header("Drag")]
    [SerializeField] private float dragSpeed = 1f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2f;

    [Header("Bounds")]
    [SerializeField] private Vector2 minPosition = new Vector2(-1000, -1000);
    [SerializeField] private Vector2 maxPosition = new Vector2(1000, 1000);

    private Vector2 lastMousePos;
    private bool isDragging;

    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    void HandleDrag()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 버튼 클릭이면 드래그 막기
            if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
                return;

            lastMousePos = Mouse.current.position.ReadValue();
            isDragging = true;
        }

        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 currentMousePos = Mouse.current.position.ReadValue();
            Vector2 delta = currentMousePos - lastMousePos;

            contentRect.anchoredPosition += delta * dragSpeed;

            ClampPosition();

            lastMousePos = currentMousePos;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll == 0) return;

        float scaleFactor = 1 + (scroll * zoomSpeed);

        float targetScale = Mathf.Clamp(
            contentRect.localScale.x * scaleFactor,
            minZoom,
            maxZoom
        );

        contentRect.localScale = new Vector3(targetScale, targetScale, 1f);
    }

    void ClampPosition()
    {
        Vector2 pos = contentRect.anchoredPosition;

        pos.x = Mathf.Clamp(pos.x, minPosition.x, maxPosition.x);
        pos.y = Mathf.Clamp(pos.y, minPosition.y, maxPosition.y);

        contentRect.anchoredPosition = pos;
    }
}