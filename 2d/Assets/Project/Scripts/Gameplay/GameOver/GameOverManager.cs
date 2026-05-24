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
    private bool isGameOver = false;

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

        isGameOver = false;
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
        if (isGameOver) return;
        isGameOver = true;

        isPlaying = false;
        BaseInteractable.IsUIOpen = true;
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        OnGameOver?.Invoke(BuildGameOverData());
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        isPlaying = false;
        BaseInteractable.IsUIOpen = true;
        StartCoroutine(GameOverRoutineImmediate());
    }

    private IEnumerator GameOverRoutineImmediate()
    {
        yield return null;
        OnGameOver?.Invoke(BuildGameOverData());
    }

    private GameOverData BuildGameOverData()
    {
        int goldBefore = GameDataManager.Instance.Gold;

        // 패널티 전 아이템 목록 복사
        var allItems = new System.Collections.Generic.List<InventoryItem>(Inventory.Instance.Items);

        GameDataManager.Instance.ApplyGameOverPenalty();
        Inventory.Instance.ApplyDeathPenalty();
        GameDataManager.Instance.ClearSavedSkills();

        // 패널티 후 살아남은 아이템
        var survivedItems = new System.Collections.Generic.List<InventoryItem>(Inventory.Instance.Items);

        // 사라진 아이템 = 전체 - 살아남은 것
        var lostItems = new System.Collections.Generic.List<InventoryItem>();
        var survivedCopy = new System.Collections.Generic.List<InventoryItem>(survivedItems);
        foreach (var item in allItems)
        {
            if (survivedCopy.Contains(item))
                survivedCopy.Remove(item);
            else
                lostItems.Add(item);
        }

        return new GameOverData
        {
            floor = GameDataManager.Instance.CurrentFloor,
            playTime = playTime,
            goldBefore = goldBefore,
            goldAfter = GameDataManager.Instance.Gold,
            itemsBefore = allItems.Count,
            itemsAfter = survivedItems.Count,
            monstersKilled = EnemyManager.Instance != null ? EnemyManager.Instance.totalKillCount : 0,
            totalDamageDealt = EnemyManager.Instance != null ? EnemyManager.Instance.totalDamageDealt : 0f,
            allItems = allItems,
            lostItems = lostItems,
        };
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
    public int monstersKilled;
    public float totalDamageDealt;
    public System.Collections.Generic.List<InventoryItem> allItems;
    public System.Collections.Generic.List<InventoryItem> lostItems;
}