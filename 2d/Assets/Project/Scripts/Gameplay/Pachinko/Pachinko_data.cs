using UnityEngine;

[System.Serializable]
public class SlotItem 
{
    public int itemValue;   // 슬롯에 표시될 숫자나 아이템 ID
    public int probability; // 당첨 확률 (1~100 사이의 가중치)
}

[CreateAssetMenu(fileName = "Pachinko_data", menuName = "Scriptable Objects/Pachinko_data")]
public class Pachinko_data : ScriptableObject
{
    public SlotItem[] items;
    public int GetTotalProbability()
    {
        int total = 0;
        foreach (var item in items) total += item.probability;
        return total;
    }
}
