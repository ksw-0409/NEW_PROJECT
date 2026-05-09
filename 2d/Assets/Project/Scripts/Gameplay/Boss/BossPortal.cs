using UnityEngine;

/// <summary>
/// 베이스 캠프의 테스트용 보스 포탈.
/// F키로 던전을 시작하되, BossEntryRequested 플래그만 세워 BossStageManager가 인식.
/// GameDataManager.CurrentFloor는 일반 LoadDungeon 흐름이 1로 세팅하도록 두어
/// PersistentData.asset에 5/10 같은 값이 디스크에 박히는 사고를 방지.
/// </summary>
public class BossPortal : BaseInteractable
{
    /// <summary>이 플래그가 true일 때만 BossStageManager가 보스를 스폰함</summary>
    public static bool BossEntryRequested = false;

    protected override void HandleInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[BossPortal] SceneController가 없습니다.");
            return;
        }

        Debug.Log("[BossPortal] 보스 모드로 던전 진입 요청 - BossEntryRequested=true");
        BossEntryRequested = true;
        // ⭐ 일반 LoadDungeon을 그대로 호출 — 내부에서 SetFloor(1) 처리
        // currentFloor는 1이지만, BossEntryRequested 플래그를 보고 BossStageManager가 보스로 처리
        SceneController.Instance.LoadDungeon();
    }
}
