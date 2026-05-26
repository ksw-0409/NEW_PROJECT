using UnityEngine;

/// <summary>
/// 베이스 캠프의 보스 포탈.
/// F키로 던전을 시작하되, BossEntryRequested 플래그만 세워 BossStageManager가 인식.
/// bossId로 어느 보스(5층 실바누스 / 10층 공허의 대사제)를 스폰할지 구분.
/// GameDataManager.CurrentFloor는 일반 LoadDungeon 흐름이 1로 세팅하도록 두어
/// PersistentData.asset에 5/10 같은 값이 디스크에 박히는 사고를 방지.
/// </summary>
public class BossPortal : BaseInteractable
{
    [Header("보스 식별")]
    [Tooltip("스폰할 보스 ID. 5 = 실바누스(5층), 10 = 공허의 대사제(10층)")]
    public int bossId = 5;

    /// <summary>이 플래그가 true일 때만 BossStageManager가 보스를 스폰함</summary>
    public static bool BossEntryRequested = false;

    /// <summary>진입 시 요청된 보스 ID (BossStageManager가 읽어 어느 보스를 스폰할지 결정)</summary>
    public static int RequestedBossId = 5;

    protected override void HandleInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[BossPortal] SceneController가 없습니다.");
            return;
        }

        Debug.Log($"[BossPortal] 보스 모드로 던전 진입 요청 - bossId={bossId}");
        BossEntryRequested = true;
        RequestedBossId = bossId;
        GameDataManager.Instance.SetFloor(bossId);
        SceneController.Instance.LoadDungeonBoss();
    }
}
