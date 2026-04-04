using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptable Objects/PersistentData")]
public class PersistentData : ScriptableObject
{
    [Header("재화")]
    public int gold = 0;

    [Header("장비")]
    public List<string> equippedItems = new List<string>();

    public void ResetAll()
    {
        gold = 0;
        equippedItems.Clear();
    }
}