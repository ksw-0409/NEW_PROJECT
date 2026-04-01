using UnityEngine;

// 역할: 던전 입장 문 — E키로 Battle 씬으로 이동
//
// 유니티 세팅
//   - 문 GameObject에 이 컴포넌트 추가
public class DungeonDoor : BaseInteractable
{
    protected override void OnInteract()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[DungeonDoor] SceneController가 없습니다.");
            return;
        }

        SceneController.Instance.LoadDungeon();
    }
}