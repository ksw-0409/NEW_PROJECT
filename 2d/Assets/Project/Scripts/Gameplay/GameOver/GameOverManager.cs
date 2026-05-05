using UnityEngine;
using System;
using System.Collections;

// 역할: 게임 오버 시 패널티 계산 및 플레이 시간 관리

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    public static event Action<GameOverData> OnGameOver;

    [Header("사망 애니메이션 대기 시간")]
    [SerializeField] private float deathDelay = 3f;

    private float playTime = 0f;
    private bool isPlaying = false;
    private bool isGameOver = false; // 중복 실행 방지

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

    void OnEnable()
    {
        PlayerStats.OnPlayerDied += HandlePlayerDied;
    }

    void OnDisable()
    {
        PlayerStats.OnPlayerDied -= HandlePlayerDied;
    }

    public void StartTimer()
    {
        if (GameDataManager.Instance.CurrentFloor == 1)
            playTime = 0f;

        isGameOver = false; // 새 던전 시작 시 초기화
        isPlaying = true;
    }

    public void StopTimer()
    {
        isPlaying = false;
    }

    void Update()
    {
        if (isPlaying)
            playTime += Time.deltaTime;
    }

    private void HandlePlayerDied()
    {
        if (isGameOver) return; // 중복 방지
        isGameOver = true;

        isPlaying = false;
        BaseInteractable.IsUIOpen = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        int goldBefore = GameDataManager.Instance.Gold;
        int itemsBefore = Inventory.Instance.Items.Count;

        GameDataManager.Instance.ApplyGameOverPenalty();
        Inventory.Instance.ApplyDeathPenalty();
        GameDataManager.Instance.ClearSavedSkills();

        int goldAfter = GameDataManager.Instance.Gold;
        int itemsAfter = Inventory.Instance.Items.Count;

        GameOverData data = new GameOverData
        {
            floor = GameDataManager.Instance.CurrentFloor,
            playTime = playTime,
            goldBefore = goldBefore,
            goldAfter = goldAfter,
            itemsBefore = itemsBefore,
            itemsAfter = itemsAfter
        };

        OnGameOver?.Invoke(data);
    }
}

public class GameOverData
{
    public int floor;
    public float playTime;
    public int goldBefore;
    public int goldAfter;
    public int itemsBefore;
    public int itemsAfter;
}