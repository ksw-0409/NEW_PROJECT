using System.Collections.Generic;
using UnityEngine;

// 씬이 바뀌어도 날아가지 않아야 할 데이터만 여기 담습니다.
// ScriptableObject라서 에셋으로 저장되며, GameDataManager가 참조합니다.
[CreateAssetMenu(fileName = "PersistentData", menuName = "Scriptable Objects/PersistentData")]
public class PersistentData : ScriptableObject
{
    [Header("재화")]
    public int gold = 0;

    [Header("장비")]
    // 장비 ID 목록 (실제 아이템 시스템 연결 전까지 string ID로 관리)
    public List<string> equippedItems = new List<string>();

    // ──────────────────────────────────────────────
    // 경험치 / 레벨 / 스킬은 게임오버 시 초기화되므로
    // PersistentData에 포함하지 않습니다.
    // PlayerStats.cs 에서 런타임으로만 관리
    // ──────────────────────────────────────────────

    // 에디터에서 테스트할 때 값이 남아있지 않도록 리셋용 메서드 제공
    public void ResetAll()
    {
        gold = 0;
        equippedItems.Clear();
    }
}