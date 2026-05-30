using UnityEngine;
using UnityEngine.UI;

// 역할: 타이틀 화면 — 버튼 클릭 시 거점 씬으로 이동

public class BootstrapLoader : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private static readonly int[] StarterItemIds = { 1001, 1010, 1011, 1012, 1013 };

    void Start()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[Bootstrap] SceneController가 없습니다.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameDataManager가 없습니다.");
            return;
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnClickStart);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnClickStart()
    {
        GiveStarterItems();
        SceneController.Instance.LoadBase();
    }
    private void GiveStarterItems()
    {
        if (EquipmentManager.Instance == null || Inventory.Instance == null)
        {
            Debug.LogError("[Bootstrap] EquipmentManager 또는 Inventory가 없습니다.");
            return;
        }

        foreach (int id in StarterItemIds)
        {
            var equipData = EquipmentManager.Instance.CreateItem(id);
            if (equipData == null) continue;

            // 감정된 상태로 지급 (바로 장착 가능)
            var item = InventoryItem.FromEquipmentData(equipData, identified: true);
            Inventory.Instance.AddItem(item);
            Debug.Log($"[Bootstrap] 기본 장비 지급: {item.itemName}");
        }
    }

    private void OnClickQuit()
    {
        Application.Quit();
    }
}