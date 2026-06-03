using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 창고 UI — OnGUI 기반의 단순한 양방향 transfer 패널.
/// 베이스 씬에서 trigger 접촉 시 또는 'B' 키로 토글.
/// 좌: 인벤토리 / 우: 창고. 슬롯 클릭 시 반대편으로 이동.
/// </summary>
public class StashUI : MonoBehaviour
{
    public static StashUI Instance { get; private set; }

    private bool isOpen = false;
    private Vector2 invScroll;
    private Vector2 stashScroll;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 베이스 씬에서만 B 키로 토글 가능 (안전)
        if (Input.GetKeyDown(KeyCode.B) && IsInBaseScene())
        {
            Toggle();
        }
        // ESC로 닫기
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    bool IsInBaseScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return scene == "base" || scene == "Base";
    }

    public void Open()
    {
        if (!IsInBaseScene())
        {
            Debug.Log("[StashUI] 창고는 베이스에서만 열 수 있습니다");
            return;
        }
        isOpen = true;
        // 게임 일시정지 효과 (시간만 멈춤)
        Time.timeScale = 0f;
    }

    public void Close()
    {
        isOpen = false;
        Time.timeScale = 1f;
    }

    public void Toggle() { if (isOpen) Close(); else Open(); }

    void OnGUI()
    {
        if (!isOpen) return;

        // 큰 박스 배경
        float w = 700, h = 500;
        float x = (Screen.width - w) / 2f;
        float y = (Screen.height - h) / 2f;

        // 배경 어둡게
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Box(new Rect(x, y, w, h), "");
        // 제목
        var titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 22;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(x, y + 10, w, 30), "📦 창고 (Stash)  —  ESC로 닫기 / B로 토글", titleStyle);

        // 패널 폭
        float panelW = (w - 30) / 2f;
        float panelH = h - 60;

        // 인벤토리 패널 (좌)
        DrawInventoryPanel(new Rect(x + 10, y + 50, panelW, panelH));
        // 창고 패널 (우)
        DrawStashPanel(new Rect(x + 20 + panelW, y + 50, panelW, panelH));
    }

    void DrawInventoryPanel(Rect rect)
    {
        GUI.Box(rect, "");
        GUI.Label(new Rect(rect.x + 10, rect.y + 5, rect.width, 24), "인벤토리 (→ 클릭해서 창고로 이동)");

        if (Inventory.Instance == null) return;
        var items = Inventory.Instance.Items;
        var area = new Rect(rect.x + 5, rect.y + 30, rect.width - 10, rect.height - 35);

        GUILayout.BeginArea(area);
        invScroll = GUILayout.BeginScrollView(invScroll);
        InventoryItem toTransfer = null;
        foreach (var item in items)
        {
            if (GUILayout.Button(FormatItem(item), GUILayout.Height(30)))
            {
                toTransfer = item;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (toTransfer != null && Stash.Instance != null)
        {
            Stash.Instance.TransferFromInventory(toTransfer);
        }
    }

    void DrawStashPanel(Rect rect)
    {
        GUI.Box(rect, "");
        string title = Stash.Instance != null
            ? $"창고 {Stash.Instance.Items.Count}/{Stash.Instance.Capacity} (→ 클릭해서 인벤토리로)"
            : "창고 (미초기화)";
        GUI.Label(new Rect(rect.x + 10, rect.y + 5, rect.width, 24), title);

        if (Stash.Instance == null) return;
        var items = Stash.Instance.Items;
        var area = new Rect(rect.x + 5, rect.y + 30, rect.width - 10, rect.height - 35);

        GUILayout.BeginArea(area);
        stashScroll = GUILayout.BeginScrollView(stashScroll);
        InventoryItem toTransfer = null;
        foreach (var item in items)
        {
            if (GUILayout.Button(FormatItem(item), GUILayout.Height(30)))
            {
                toTransfer = item;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (toTransfer != null)
        {
            Stash.Instance.TransferToInventory(toTransfer);
        }
    }

    string FormatItem(InventoryItem item)
    {
        if (item == null) return "(빈 아이템)";
        string grade = item.Grade.ToString();
        return $"[{grade}] {item.itemName}";
    }
}
