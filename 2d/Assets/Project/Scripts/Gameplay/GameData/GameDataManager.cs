using System;
using System.Collections.Generic;
using UnityEngine;

// 역할: 씬 전환에도 살아남는 유일한 데이터 관리자
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public static event Action<int> OnGoldChanged;
    public static event Action<int> OnNormalCurrencyChanged;
    public static event Action<int> OnSpecialCurrencyChanged;
    public static event Action<List<string>> OnItemsChanged;
    public static event Action<int> OnFloorChanged;
    public static event Action<int> OnBossTokensChanged;

    [SerializeField] private PersistentData persistentData;

    public int Gold => persistentData.gold;
    public int NormalCurrency => persistentData.normalCurrency;
    public int SpecialCurrency => persistentData.specialCurrency;
    public IReadOnlyList<string> Items => persistentData.equippedItems;
    public IReadOnlyList<string> EquippedSkills => persistentData.savedSkills;
    public int CurrentFloor => persistentData.currentFloor;
    public int BossTokens => persistentData.bossTokens;

    public bool isPachinkoActive=false;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ✨ 에셋 원본을 건드리지 않도록 런타임 복사본 생성
        // 해금 데이터(스킬 트리/패시브)는 복사본에 유지되고,
        // 골드/층수 등 인게임 데이터는 매 실행마다 초기화됨
        persistentData = Instantiate(persistentData);
        persistentData.ResetRuntimeData();

        Debug.Log($"[GameDataManager] 로드 완료 — unlocked 노드: {persistentData.unlockedSkillNodes.Count}, 골드: {persistentData.gold}");
    }

    // 명시적 리셋 (메뉴에서 호출)
    public void HardResetAll()
    {
        persistentData.ResetAll();
        OnGoldChanged?.Invoke(persistentData.gold);
        OnNormalCurrencyChanged?.Invoke(persistentData.normalCurrency);
        OnSpecialCurrencyChanged?.Invoke(persistentData.specialCurrency);
        OnItemsChanged?.Invoke(persistentData.equippedItems);
        OnFloorChanged?.Invoke(persistentData.currentFloor);
        Debug.Log("[GameDataManager] HardResetAll 호출 — 모든 데이터 리셋");
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        persistentData.gold += amount;
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    public void PachinkoAddGold(int amount)
    {
        persistentData.gold += amount;
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    public bool SpendGold(int amount)
    {
        if (persistentData.gold < amount) return false;
        persistentData.gold -= amount;
        OnGoldChanged?.Invoke(persistentData.gold);
        return true;
    }

    public void AddNormalCurrency(int amount)
    {
        if (amount <= 0) return;
        persistentData.normalCurrency += amount;
        OnNormalCurrencyChanged?.Invoke(persistentData.normalCurrency);
    }

    public bool SpendNormalCurrency(int amount)
    {
        if (persistentData.normalCurrency < amount) return false;
        persistentData.normalCurrency -= amount;
        OnNormalCurrencyChanged?.Invoke(persistentData.normalCurrency);
        return true;
    }

    public void SaveUnlockedNode(string skillName)
    {
        if (!persistentData.unlockedSkillNodes.Contains(skillName))
            persistentData.unlockedSkillNodes.Add(skillName);
    }

    public bool IsNodeUnlocked(string skillName)
    {
        return persistentData.unlockedSkillNodes.Contains(skillName);
    }

    public void SaveUnlockedEffect(UnlockedNodeEffect effect)
    {
        if (effect == null || string.IsNullOrEmpty(effect.skillNodeID)) return;
        persistentData.unlockedEffects.RemoveAll(e => e.skillNodeID == effect.skillNodeID);
        persistentData.unlockedEffects.Add(effect);
    }

    public List<UnlockedNodeEffect> GetUnlockedEffects()
    {
        return persistentData.unlockedEffects;
    }

    public void SavePassiveLevels(System.Collections.Generic.Dictionary<string, int> levels)
    {
        persistentData.passiveLevels.Clear();
        foreach (var kv in levels)
            persistentData.passiveLevels.Add(new PassiveLevelEntry { passiveID = kv.Key, level = kv.Value });
    }

    public System.Collections.Generic.Dictionary<string, int> GetPassiveLevels()
    {
        var d = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var e in persistentData.passiveLevels)
            if (!string.IsNullOrEmpty(e.passiveID)) d[e.passiveID] = e.level;
        return d;
    }

    public void AddSpecialCurrency(int amount)
    {
        if (amount <= 0) return;
        persistentData.specialCurrency += amount;
        OnSpecialCurrencyChanged?.Invoke(persistentData.specialCurrency);
    }

        // 보스 처치 증표 (골드처럼 카운트로 저장 — 추후 인벤토리 아이템 등으로 조정 가능)
    public void AddBossToken(int amount)
    {
        if (amount <= 0) return;
        persistentData.bossTokens += amount;
        OnBossTokensChanged?.Invoke(persistentData.bossTokens);
        Debug.Log($"<color=yellow>[BossToken]</color> +{amount} → 총 {persistentData.bossTokens}개");
    }

    public bool SpendBossToken(int amount)
    {
        if (persistentData.bossTokens < amount) return false;
        persistentData.bossTokens -= amount;
        OnBossTokensChanged?.Invoke(persistentData.bossTokens);
        return true;
    }

public bool SpendSpecialCurrency(int amount)
    {
        if (persistentData.specialCurrency < amount) return false;
        persistentData.specialCurrency -= amount;
        OnSpecialCurrencyChanged?.Invoke(persistentData.specialCurrency);
        return true;
    }

    public void AddItem(string itemId)
    {
        persistentData.equippedItems.Add(itemId);
        OnItemsChanged?.Invoke(persistentData.equippedItems);
    }

    public void RemoveItem(string itemId)
    {
        persistentData.equippedItems.Remove(itemId);
        OnItemsChanged?.Invoke(persistentData.equippedItems);
    }

    public void SetFloor(int floor)
    {
        persistentData.currentFloor = floor;
        OnFloorChanged?.Invoke(floor);
        Debug.Log($"[GameDataManager] 현재 층: {floor}");
    }

    public void NextFloor()
    {
        SetFloor(persistentData.currentFloor + 1);
    }

    public void ResetFloor()
    {
        SetFloor(1);
    }

    public void SaveRuntimeItem(int slotIndex, EquipmentData data)
    {
        var list = persistentData.runtimeItems;
        while (list.Count <= slotIndex) list.Add(null);
        list[slotIndex] = RuntimeItemData.FromEquipmentData(data);
        Debug.Log($"[GameDataManager] 슬롯 {slotIndex} 아이템 저장: {data.itemName}");
    }

    public void LoadRuntimeItem(int slotIndex, EquipmentData target)
    {
        var list = persistentData.runtimeItems;
        if (slotIndex >= list.Count || list[slotIndex] == null) return;
        list[slotIndex].ApplyTo(target);
        Debug.Log($"[GameDataManager] 슬롯 {slotIndex} 아이템 불러오기: {target.itemName}");
    }

    public bool HasRuntimeItem(int slotIndex)
    {
        var list = persistentData.runtimeItems;
        return slotIndex < list.Count && list[slotIndex] != null;
    }

    public void SaveSkill(string skillName)
    {
        if (!persistentData.savedSkills.Contains(skillName))
            persistentData.savedSkills.Add(skillName);
    }

    public void RemoveSkill(string skillName)
    {
        persistentData.savedSkills.Remove(skillName);
    }

    public void ClearSavedSkills()
    {
        persistentData.savedSkills.Clear();
    }

    public bool HasSkillSaved(string skillName)
    {
        return persistentData.savedSkills.Contains(skillName);
    }

    public void ApplyGameOverPenalty()
    {
        ApplyGoldPenalty();
        ApplyItemPenalty();
        ResetFloor();
    }

    private void ApplyGoldPenalty()
    {
        persistentData.gold = Mathf.FloorToInt(persistentData.gold * 0.5f);
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    private void ApplyItemPenalty()
    {
        List<string> items = persistentData.equippedItems;
        if (items.Count == 0) return;

        int removeCount = Mathf.CeilToInt(items.Count * 0.5f);
        List<int> indices = new List<int>(items.Count);
        for (int i = 0; i < items.Count; i++) indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        List<int> toRemove = indices.GetRange(0, removeCount);
        toRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int idx in toRemove)
            items.RemoveAt(idx);

        OnItemsChanged?.Invoke(items);
    }

#if UNITY_EDITOR
    [ContextMenu("게임오버 패널티 테스트")]
    private void TestPenalty()
    {
        Debug.Log($"[패널티 전] 골드: {Gold}, 장비: {Items.Count}개, 층: {CurrentFloor}");
        ApplyGameOverPenalty();
        Debug.Log($"[패널티 후] 골드: {Gold}, 장비: {Items.Count}개, 층: {CurrentFloor}");
    }
#endif
}