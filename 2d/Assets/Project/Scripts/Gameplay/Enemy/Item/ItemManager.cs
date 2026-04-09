using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Pool;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance; // 싱글톤

    [Header("아이템 설정")]

    public EquipmentManager equipManager; // 유니티에서 EquipmentManager 오브젝트를 연결
    public GameObject equipmentPrefab;    // 장비 전용 프리팹 (FieldItem 스크립트가 붙은 것)
    public GameObject itemPrefab; // 경험치 프리팹

    public GameObject player;
    private IObjectPool<Item> pool;
    [Header("아이템 드랍 확률")]
    [Range(0, 100)] public float equipDropChance = 10f; // 장비 드랍 확률 (ex: 10% 확률로 장비 드랍)

    // 현재 필드에 떨어져 있는 아이템들 (자석 효과 등을 위해 관리)
    public List<Item> activeItems = new List<Item>();

    void Awake()
    {
        Instance = this;

        pool = new ObjectPool<Item>(
            OnCreateItem,
            OnGetItem,
            OnReleaseItem,
            OnDestroyItem,
            maxSize: 500 // 아이템은 넉넉하게 설정
        );
    }

    private Item OnCreateItem()
    {
        GameObject obj = Instantiate(itemPrefab, transform);
        Item item = obj.GetComponent<Item>();
        item.SetPool(pool);
        return item;
    }

    private void OnGetItem(Item item)
    {
        item.gameObject.SetActive(true);
        activeItems.Add(item);
    }

    private void OnReleaseItem(Item item)
    {
        item.gameObject.SetActive(false);
        activeItems.Remove(item);
    }

    private void OnDestroyItem(Item item)
    {
        Destroy(item.gameObject);
    }


    // 적이 죽을 때 호출할 함수
    public void DropItem(Vector2 position)
    {
        Item item = pool.Get();
        item.transform.position = position;

        // 드랍 확률 계산
        if (Random.Range(0f, 100f) <= equipDropChance)
        {
            DropEquipment(position);
        }
    }
    private void DropEquipment(Vector2 position)
    {
        // 등급 확률 계산 (예: 일반 70%, 레어 20%, 에픽 8%, 전설 2%)
        int selectedID = GetRandomIDByWeight();

        // 장비 데이터 생성 (랜덤 스탯 부여됨)
        EquipmentData randomData = equipManager.CreateItem(selectedID);

        // 필드에 장비 오브젝트 생성
        GameObject equipObj = Instantiate(equipmentPrefab, position, Quaternion.identity);
        equipObj.GetComponent<FieldItem>().Setup(randomData);
    }
    private int GetRandomIDByWeight()
    {
        float roll = Random.Range(0f, 100f);

        // 테스트용: 50% 확률로 1013번, 아니면 1001번 드랍
        if (roll < 50f)
        {
            return 1013; // 연마된 검 (RARE)
        }
        else
        {
            return 1001; // 낡은 검 (COMMON)
        }
    }
    void FixedUpdate()
    {
        if (player == null) return;

        float magnetDistance = 3.0f; // 자석 범위
        float moveSpeed = 10.0f;

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            float dist = Vector2.Distance(activeItems[i].transform.position, player.transform.position);

            if (dist < magnetDistance)
            {
                // 플레이어 방향으로 이동
                activeItems[i].transform.position = Vector2.MoveTowards(
                    activeItems[i].transform.position,
                    player.transform.position,
                    moveSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
}