using UnityEngine;

// 역할: 게임 시작 시 1회만 실행되는 초기화 진입점

public class BootstrapLoader : MonoBehaviour
{
    void Awake()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[Bootstrap] SceneController가 없습니다. GameObject 세팅을 확인하세요.");
            return;
        }

        if (GameDataManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameDataManager가 없습니다. GameObject 세팅을 확인하세요.");
            return;
        }

        SceneController.Instance.LoadBase();
    }
}