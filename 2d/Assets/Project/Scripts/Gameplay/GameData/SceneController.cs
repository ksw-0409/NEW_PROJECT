using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 역할: 씬 전환의 단일 창구

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public static class SceneName
    {
        public const string Bootstrap = "Bootstrap";
        public const string Base = "Base";
        public const string Dungeon = "Dungeon";
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

    public void LoadBase() => StartCoroutine(LoadSceneRoutine(SceneName.Base));
    public void LoadDungeon() => StartCoroutine(LoadSceneRoutine(SceneName.Dungeon));

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        OnSceneLoadStart?.Invoke();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        OnSceneLoadComplete?.Invoke();
    }
}
