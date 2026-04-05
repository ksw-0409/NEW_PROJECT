using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance; // 싱글톤

    public GameObject itemPrefab;
    public GameObject player;
    private IObjectPool<Item> pool;

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