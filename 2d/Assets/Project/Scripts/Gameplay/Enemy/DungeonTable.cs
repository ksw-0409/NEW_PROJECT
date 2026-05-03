using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// [1] 2차원 배열의 '열' (ID와 확률 한 쌍)
[System.Serializable]
public class MonsterSpawnRate
{
    public int monsterID; // ID
    public float chance;     // 확률(가중치)
}

// [2] 2차원 배열의 '행' (한 층의 정보)
[System.Serializable]
public class FloorData
{
    public int floorName; // "1층", "2층" 등 이름표
    public List<MonsterSpawnRate> spawnList; // 층별 몬스터 리스트 (동적 배열)
}

[CreateAssetMenu(fileName = "DungeonTable", menuName = "Scriptable Objects/DungeonTable")]
public class DungeonTable : ScriptableObject
{
    public List<FloorData> floors; // 1층부터 10층까지 담길 리스트
}
