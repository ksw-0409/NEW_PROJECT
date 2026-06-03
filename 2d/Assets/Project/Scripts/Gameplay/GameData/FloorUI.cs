using UnityEngine;
using TMPro;

// 역할: 던전 현재 층수를 TMP로 표시 (골드UI 옆)

public class FloorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI floorText;

    void OnEnable()
    {
        GameDataManager.OnFloorChanged += RefreshFloor;

        if (GameDataManager.Instance != null)
            RefreshFloor(GameDataManager.Instance.CurrentFloor);
    }

    void OnDisable()
    {
        GameDataManager.OnFloorChanged -= RefreshFloor;
    }

    private void RefreshFloor(int floor)
    {
        if (floorText != null)
            floorText.text = $"{floor}F";
    }
}
