using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptable Objects/PersistentData")]
public class PersistentData : ScriptableObject
{
    [Header("재화")]
    public int gold = 1000;

    [Header("장비")]
    public List<string> equippedItems = new List<string>();

    [Header("진행")]
    public int currentFloor = 1;

    [Header("런타임 아이템 데이터 (리롤 결과 저장)")]
    public List<RuntimeItemData> runtimeItems = new List<RuntimeItemData>();


    public void ResetAll()
    {
        gold = 1000;
        equippedItems.Clear();
        currentFloor = 1;
        runtimeItems.Clear();
    }
}