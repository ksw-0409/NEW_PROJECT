using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 역할: 씬 전환의 단일 창구
//   - 일반 전환: LoadBase(), LoadDungeon()
//   - Additive 전환: LoadAdditive(), UnloadAdditive()
//   - 거점 오브젝트(모루/모닥불 등) → Additive 방식 사용
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public static class SceneName
    {
        public const string Bootstrap = "Bootstrap";
        public const string Base = "Base";
        public const string Dungeon = "Dungeon";
        public const string Anvil = "Anvil";
        public const string SkillTree = "SkillTree";
    }

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

    // ── 일반 전환 ────────────────────────────────────

    public void LoadBase() => StartCoroutine(LoadSceneRoutine(SceneName.Base));
    public void LoadDungeon() => StartCoroutine(LoadSceneRoutine(SceneName.Dungeon));

    // ── Additive 전환 ────────────────────────────────
    // 거점 씬을 유지한 채로 위에 씬을 추가 로드
    // 호출 예시: SceneController.Instance.LoadAdditive(SceneName.Anvil)

    public void LoadAdditive(string sceneName)
        => StartCoroutine(LoadAdditiveRoutine(sceneName));

    public void UnloadAdditive(string sceneName)
        => StartCoroutine(UnloadAdditiveRoutine(sceneName));

    // ── 내부 로직 ────────────────────────────────────

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        OnSceneLoadStart?.Invoke();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        OnSceneLoadComplete?.Invoke();
    }

    private IEnumerator LoadAdditiveRoutine(string sceneName)
    {
        // 이미 로드된 씬이면 중복 로드 방지
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"[SceneController] {sceneName} 이미 로드된 씬입니다.");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;
    }

    private IEnumerator UnloadAdditiveRoutine(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.LogWarning($"[SceneController] {sceneName} 로드되지 않은 씬입니다.");
            yield break;
        }

        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }
}