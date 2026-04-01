using System;
using System.Collections.Generic;
using UnityEngine;

// 역할: 씬 전환에도 살아남는 유일한 데이터 관리자
//   - DontDestroyOnLoad로 Bootstrap 씬에서 생성 후 게임 내내 유지
//   - 게임오버 패널티(골드 50%, 장비 절반 랜덤 삭제) 처리
//   - 씬 전환은 SceneController에 위임 (직접 하지 않음)
public class GameDataManager : MonoBehaviour
{
    // ── 싱글톤 ──────────────────────────────────────
    // 유일하게 싱글톤을 허용하는 이유:
    // "씬을 넘어 데이터를 유지해야 하는 단 하나의 책임"만 가지기 때문
    // 외부에서 데이터를 직접 수정하지 말고 반드시 공개 메서드를 통해 변경할 것
    public static GameDataManager Instance { get; private set; }

    // ── 이벤트 ──────────────────────────────────────
    // UI나 다른 스크립트는 이 이벤트를 구독해서 변화를 감지
    // GameDataManager.Instance.gold 를 직접 읽지 말 것
    public static event Action<int> OnGoldChanged;           // 현재 골드
    public static event Action<List<string>> OnItemsChanged; // 현재 장비 목록

    // ── 데이터 ──────────────────────────────────────
    [SerializeField] private PersistentData persistentData;

    // 외부 읽기 전용 프로퍼티 (쓰기는 메서드로만)
    public int Gold => persistentData.gold;
    public IReadOnlyList<string> Items => persistentData.equippedItems;

    // ────────────────────────────────────────────────
    void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 골드 ────────────────────────────────────────

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
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

    // ── 장비 ────────────────────────────────────────

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

    // ── 게임오버 패널티 ──────────────────────────────
    // 호출 주체: GameOverHandler.cs
    // 규칙:
    //   골드  → 50% 손실 (반올림)
    //   장비  → 보유 개수의 절반을 랜덤 삭제 (홀수면 올림, 즉 더 많이 잃음)
    public void ApplyGameOverPenalty()
    {
        ApplyGoldPenalty();
        ApplyItemPenalty();
    }

    private void ApplyGoldPenalty()
    {
        // 소수점 버림: 99골드 → 49골드
        persistentData.gold = Mathf.FloorToInt(persistentData.gold * 0.5f);
        OnGoldChanged?.Invoke(persistentData.gold);
    }

    private void ApplyItemPenalty()
    {
        List<string> items = persistentData.equippedItems;
        if (items.Count == 0) return;

        // 삭제 개수: 올림 (3개 보유 → 2개 삭제, 2개 보유 → 1개 삭제)
        int removeCount = Mathf.CeilToInt(items.Count * 0.5f);

        // Fisher-Yates 셔플로 랜덤 인덱스 선택 (GC 최소화: List 복사 없이 인덱스만 사용)
        List<int> indices = new List<int>(items.Count);
        for (int i = 0; i < items.Count; i++) indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        // 인덱스 내림차순 정렬 후 삭제 (앞에서 지우면 인덱스 밀리는 문제 방지)
        List<int> toRemove = indices.GetRange(0, removeCount);
        toRemove.Sort((a, b) => b.CompareTo(a));
        foreach (int idx in toRemove)
        {
            items.RemoveAt(idx);
        }

        OnItemsChanged?.Invoke(items);
    }

    // ── 에디터/테스트용 ──────────────────────────────
#if UNITY_EDITOR
    [ContextMenu("게임오버 패널티 테스트")]
    private void TestPenalty()
    {
        Debug.Log($"[패널티 전] 골드: {Gold}, 장비: {Items.Count}개");
        ApplyGameOverPenalty();
        Debug.Log($"[패널티 후] 골드: {Gold}, 장비: {Items.Count}개");
    }
#endif
}