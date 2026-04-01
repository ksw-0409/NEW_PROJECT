using UnityEngine;

// 역할: 게임 시작 시 1회만 실행되는 초기화 진입점
//   - Bootstrap 씬의 유일한 스크립트
//   - GameDataManager, SceneController가 준비되면 Base 씬으로 자동 이동
//
// 유니티 세팅
//   1. Bootstrap 씬에 빈 GameObject 생성
//   2. GameDataManager, SceneController, BootstrapLoader 컴포넌트 추가
//   3. Build Settings 씬 순서: 0.Bootstrap / 1.Base / 2.Battle
public class BootstrapLoader : MonoBehaviour
{
    void Awake()
    {
        // GameDataManager, SceneController는 같은 GameObject에 있으므로
        // 이 시점에서 이미 각자의 Awake()가 실행되어 Instance가 준비된 상태
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