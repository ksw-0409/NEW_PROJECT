using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptable Objects/PersistentData")]
public class PersistentData : ScriptableObject
{
    [Header("재화")]
    public int gold = 1000;
    public int normalCurrency = 10;
    public int specialCurrency = 10;
    public int bossTokens = 0;       // 보스 처치 증표 (골드처럼 카운트 저장, 추후 조정 가능)

    [Header("장비")]
    public List<string> equippedItems = new List<string>();

    [Header("층")]
    public int currentFloor = 1;

    [Header("리타임 아이템 데이터 (리롤 결과 보존)")]
    public List<RuntimeItemData> runtimeItems = new List<RuntimeItemData>();

    [Header("활성화된 스킬 노드 (skillNodeID 저장)")]
    public List<string> unlockedSkillNodes = new List<string>();

    [Header("저장된 스킬 (skillName 저장)")]
    public List<string> savedSkills = new List<string>();

    [Header("✨ 해금된 노드의 효과 데이터 (PlayerStats 재구성용 - 씬 전환 시 효과 복원)")]
    public List<UnlockedNodeEffect> unlockedEffects = new List<UnlockedNodeEffect>();

    [Header("✨ 패시브 스킬 레벨 (id -> level)")]
    public List<PassiveLevelEntry> passiveLevels = new List<PassiveLevelEntry>();
    public List<SkillLevelEntry> savedSkillLevels = new List<SkillLevelEntry>(); // ⭐ 스킬 레벨 영구 저장 (씬 전환 시 유지)

    // 전체 초기화 (스킬 트리 포함)
    public void ResetAll()
    {
        gold = 1000;
        normalCurrency = 10;
        specialCurrency = 10;
        equippedItems.Clear();
        currentFloor = 1;
        runtimeItems.Clear();
        unlockedSkillNodes.Clear();
        savedSkills.Clear();
        unlockedEffects.Clear();
        passiveLevels.Clear();
    }

    // ✨ 인게임 데이터만 초기화 (스킬 트리 해금/패시브는 유지)
    public void ResetRuntimeData()
    {
        gold = 1000;
        normalCurrency = 10;
        specialCurrency = 10;
        equippedItems.Clear();
        currentFloor = 1;
        runtimeItems.Clear();
        savedSkills.Clear();
    }
}

[System.Serializable]
public class PassiveLevelEntry
{
    public string passiveID;
    public int level;
}

[System.Serializable]
public class SkillLevelEntry
{
    public string skillName;
    public int level;
}

[System.Serializable]
public class UnlockedNodeEffect
{
    public string skillNodeID;
    public string targetSkillAssetName;
    public int nodeType;
    public string specialtyTag;
    public float damageMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public int countBonus = 0;
    public float slowPercentMultiplier = 1f;
    public float durationMultiplier = 1f;
}