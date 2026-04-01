using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 역할: 씬 전환의 단일 창구
//   - 모든 씬 이동은 반드시 이 클래스를 통해서만 수행
//   - 페이드 아웃/인 같은 연출도 여기서 처리 (추후 확장)
//   - GameDataManager와 마찬가지로 DontDestroyOnLoad로 유지
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    // 씬 이름은 상수로 관리 → 오타 방지
    public static class SceneName
    {
        public const string Bootstrap = "Bootstrap";
        public const string Base = "Base";      // 거점
        public const string Dungeon = "Dungeon";    // 전투
    }

    // 씬 전환 시작/완료 이벤트 (로딩 UI 등에서 구독)
    public static event Action OnSceneLoadStart;
    public static event Action OnSceneLoadComplete;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 공개 API ────────────────────────────────────

    // 거점으로 이동 (게임오버 후 호출)
    public void LoadBase() => StartCoroutine(LoadSceneRoutine(SceneName.Base));

    // 전투 씬으로 이동 (던전 문에서 호출)
    public void LoadDungeon() => StartCoroutine(LoadSceneRoutine(SceneName.Dungeon));

    // ── 내부 로직 ────────────────────────────────────

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        OnSceneLoadStart?.Invoke();

        // 추후 페이드 아웃 연출 추가 위치
        // yield return StartCoroutine(FadeOut());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로드 완료까지 대기
        while (!op.isDone)
        {
            yield return null;
        }

        OnSceneLoadComplete?.Invoke();

        // 추후 페이드 인 연출 추가 위치
        // yield return StartCoroutine(FadeIn());
    }
}