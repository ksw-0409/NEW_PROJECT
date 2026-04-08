using UnityEngine;

// 역할: 층별 스테이지 설정 데이터

[CreateAssetMenu(fileName = "StageData", menuName = "Scriptable Objects/StageData")]
public class StageData : ScriptableObject
{
    [System.Serializable]
    public class FloorData
    {
        public int floor;                  // 층 번호
        public float stageDuration = 180f; // 제한 시간 (초)
    }

    public FloorData[] floors;

    public FloorData GetFloorData(int floor)
    {
        foreach (var data in floors)
        {
            if (data.floor == floor) return data;
        }
        return floors[floors.Length - 1];
    }
}