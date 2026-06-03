using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 창고 UI — 인벤토리는 화면 왼쪽 끝, 창고는 오른쪽 끝, 중앙에 큰 ➤ 화살표.
/// 베이스 씬에서 B 키로 토글. ESC로 닫기.
/// </summary>
public class StashUI : MonoBehaviour
{
    public static StashUI Instance { get; private set; }
    public bool IsOpen { get; private set; } = false;

    private GameObject stashPanel;
    private StashSlot[] stashSlots;
    private GameObject inventoryPanelRef;
    private GameObject arrowGo;

    // 원래 인벤토리 anchor/pivot/pos 저장 (Close 시 복원)
    private Vector2 origAnchorMin, origAnchorMax, origPivot, origAnchoredPos;
    private bool savedOrig = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // ⭐ B 키 토글 제거 — StashInteractable trigger + F 키로 Open() 호출됨
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    bool IsInBaseScene()
    {
        var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        return s == "base" || s == "Base";
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        if (!IsInBaseScene()) return;

        var iuType = System.Type.GetType("InventoryUI, Assembly-CSharp");
        var iu = UnityEngine.Object.FindFirstObjectByType(iuType, FindObjectsInactive.Include) as MonoBehaviour;
        if (iu != null)
        {
            var hack = new UnityEditor_StashUIHack();
            inventoryPanelRef = hack.GetField(iu, "inventoryPanel") as GameObject;
            if (inventoryPanelRef != null)
            {
                inventoryPanelRef.SetActive(true);
                var equip = hack.GetField(iu, "equipmentPanel") as GameObject;
                if (equip != null) equip.SetActive(false);
                iu.GetType().GetMethod("ForceRefresh")?.Invoke(iu, null);
            }
        }
        if (inventoryPanelRef == null) return;

        // 원래 anchor/pivot/pos 저장 (최초 1회)
        var invRt = inventoryPanelRef.GetComponent<RectTransform>();
        if (!savedOrig)
        {
            origAnchorMin = invRt.anchorMin;
            origAnchorMax = invRt.anchorMax;
            origPivot = invRt.pivot;
            origAnchoredPos = invRt.anchoredPosition;
            savedOrig = true;
        }

        // ⭐ 좌측 끝에 배치: anchor (0, 0.5), pivot (0, 0.5), anchoredPos = (20, 0)
        invRt.anchorMin = new Vector2(0f, 0.5f);
        invRt.anchorMax = new Vector2(0f, 0.5f);
        invRt.pivot = new Vector2(0f, 0.5f);
        invRt.anchoredPosition = new Vector2(20f, 0f); // 화면 왼쪽 가장자리 + 약간 여백

        // 창고 패널 생성 (최초 1회)
        if (stashPanel == null) CreateStashPanel();

        // ⭐ 우측 끝에 배치: anchor (1, 0.5), pivot (1, 0.5), anchoredPos = (-20, 0)
        var stRt = stashPanel.GetComponent<RectTransform>();
        stRt.anchorMin = new Vector2(1f, 0.5f);
        stRt.anchorMax = new Vector2(1f, 0.5f);
        stRt.pivot = new Vector2(1f, 0.5f);
        stRt.anchoredPosition = new Vector2(-20f, 0f); // 화면 오른쪽 가장자리

        stashPanel.SetActive(true);
        RefreshStashSlots();

        // 중앙 화살표
        if (arrowGo == null) CreateArrow();
        arrowGo.SetActive(true);
        var arRt = arrowGo.GetComponent<RectTransform>();
        arRt.anchorMin = new Vector2(0.5f, 0.5f);
        arRt.anchorMax = new Vector2(0.5f, 0.5f);
        arRt.pivot = new Vector2(0.5f, 0.5f);
        arRt.anchoredPosition = Vector2.zero;

        BaseInteractable.IsUIOpen = true;
        IsOpen = true;
        Stash.OnStashChanged += RefreshStashSlots;
    }

    public void Close()
    {
        if (stashPanel != null) stashPanel.SetActive(false);
        if (arrowGo != null) arrowGo.SetActive(false);

        // 인벤토리 패널 원래 위치 복원 + 비활성
        if (inventoryPanelRef != null)
        {
            if (savedOrig)
            {
                var rt = inventoryPanelRef.GetComponent<RectTransform>();
                rt.anchorMin = origAnchorMin;
                rt.anchorMax = origAnchorMax;
                rt.pivot = origPivot;
                rt.anchoredPosition = origAnchoredPos;
            }
            inventoryPanelRef.SetActive(false);
        }

        Stash.OnStashChanged -= RefreshStashSlots;
        InventoryTooltip.Instance?.Hide();
        BaseInteractable.IsUIOpen = false;
        IsOpen = false;
    }

    void CreateStashPanel()
    {
        stashPanel = UnityEngine.Object.Instantiate(inventoryPanelRef, inventoryPanelRef.transform.parent);
        stashPanel.name = "StashPanel";
        var rt = stashPanel.GetComponent<RectTransform>();
        var src = inventoryPanelRef.GetComponent<RectTransform>();
        if (rt != null && src != null) rt.sizeDelta = src.sizeDelta;

        var oldSlots = stashPanel.GetComponentsInChildren<InventorySlot>(true);
        var newSlots = new System.Collections.Generic.List<StashSlot>();
        foreach (var s in oldSlots)
        {
            var go = s.gameObject;
            UnityEngine.Object.DestroyImmediate(s);
            var stashSlot = go.AddComponent<StashSlot>();
            newSlots.Add(stashSlot);
        }
        stashSlots = newSlots.ToArray();
    }

    void CreateArrow()
    {
        arrowGo = new GameObject("StashArrow");
        arrowGo.transform.SetParent(inventoryPanelRef.transform.parent, false);
        var rt = arrowGo.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400f, 400f);
        var tmp = arrowGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "→";
        tmp.fontSize = 300f;
        tmp.color = new Color(1f, 0.85f, 0.3f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }

    void RefreshStashSlots()
    {
        if (stashSlots == null || Stash.Instance == null) return;
        var items = Stash.Instance.Items;
        for (int i = 0; i < stashSlots.Length; i++)
        {
            InventoryItem it = (i < items.Count) ? items[i] : null;
            stashSlots[i].Setup(it);
        }
    }
}

internal class UnityEditor_StashUIHack
{
    public object GetField(object target, string fieldName)
    {
        if (target == null) return null;
        var fi = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return fi?.GetValue(target);
    }
}
