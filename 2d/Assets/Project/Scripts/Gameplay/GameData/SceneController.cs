using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// ����: �� ��ȯ�� ���� â��

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    public static class SceneName
    {
        public const string Bootstrap = "Bootstrap";
        public const string Base = "base"; // ⭐ 실제 파일명 base.unity와 일치 (case-sensitive 빌드 대응)
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
    public void LoadDungeonBoss()
    {
        StartCoroutine(LoadSceneRoutine(SceneName.Dungeon));
    }

    public void LoadBase() => StartCoroutine(LoadSceneRoutine(SceneName.Base));
    public void LoadDungeon()
    {
        GameDataManager.Instance.SetFloor(1);
        StartCoroutine(LoadSceneRoutine(SceneName.Dungeon));
    }
    public void LoadScene(string sceneName) => StartCoroutine(LoadSceneRoutine(sceneName));

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        OnSceneLoadStart?.Invoke();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        OnSceneLoadComplete?.Invoke();
    }
}
