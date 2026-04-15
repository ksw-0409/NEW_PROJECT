using UnityEngine;

// 역할: 스테이지 클리어 후 생성되는 포탈

public class Portal : BaseInteractable
{
    private string nextScene;

    // StageManager에서 호출
    public void SetNextScene(string sceneName)
    {
        nextScene = sceneName;
        Debug.Log($"[Portal] 목적지 설정: {nextScene}");
    }

    protected override void HandleInteract()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[Portal] nextScene이 설정되지 않았습니다.");
            return;
        }

        if (SceneController.Instance == null)
        {
            Debug.LogError("[Portal] SceneController가 없습니다.");
            return;
        }

        // 다음 층으로 이동 전 층 증가
        if (nextScene == SceneController.SceneName.Dungeon)
            GameDataManager.Instance.NextFloor();

        else if (nextScene == SceneController.SceneName.Base)
        {
            // 거점으로 돌아갈 때 스킬 초기화
            GameDataManager.Instance.ClearSavedSkills();
        }

        SceneController.Instance.LoadScene(nextScene);
    }
}