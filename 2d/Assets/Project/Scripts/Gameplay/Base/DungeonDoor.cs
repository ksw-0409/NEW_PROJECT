using UnityEngine;

// 역할: 던전 입장 문 — E키로 Battle 씬으로 이동

public class DungeonDoor : BaseInteractable
{
    protected override void HandleInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[DungeonDoor] SceneController가 없습니다.");
            return;
        }

        SceneController.Instance.LoadDungeon();
    }
}