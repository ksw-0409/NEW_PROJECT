using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance; // 싱글톤
    [Header("아이템 설정")]
    public EquipmentManager equipManager; // 유니티에서 EquipmentManager 오브젝트를 연결
    public GameObject equipmentPrefab;    // 장비 전용 프리팹 (FieldItem 스크립트가 붙은 것)
    public GameObject player;
    // 현재 필드에 떨어져 있는 아이템들 (자석 효과 등을 위해 관리)
    public List<GameObject> activeItems = new List<GameObject>();

    //무기 드랍 확률
    float[,] dropChances = {
    { 50f, 25f, 15f, 10f }, // 1~5층
    { 25f, 35f, 25f, 15f },  // 6~10층
    { 0f, 50f, 30f, 20f }  // 보스
    };

    void Awake()
    {
        Instance = this;
    }

    public void RemoveItem(GameObject item)
    {
        activeItems.Remove(item);
    }

    // 적이 죽을 때 호출할 함수
    public void DropItem(Vector2 position, float equipDropChance,bool isBoss)
    {     
        // 드랍 확률 계산
        if (Random.Range(0f, 100f) <= equipDropChance)
        {
            DropEquipment(position, isBoss);
        }
    }
    private void DropEquipment(Vector2 position, bool isBoss)
    {
        // 등급 확률 계산 K 구현완
        int selectedID = GetRandomIDByWeight(isBoss);

        // 장비 데이터 생성 (랜덤 스탯 부여됨)
        EquipmentData randomData = equipManager.CreateItem(selectedID);

        // 필드에 장비 오브젝트 생성
        GameObject equipObj = Instantiate(equipmentPrefab, position, Quaternion.identity);
        equipObj.GetComponent<FieldItem>().Setup(randomData);
        activeItems.Add(equipObj);
    }
    private int GetRandomIDByWeight(bool isBoss)
    {
        int floor = GameDataManager.Instance.CurrentFloor;
        int rowIndex = (floor <= 5) ? 0 : 1;
        if (isBoss) rowIndex = 2;
        float roll = Random.Range(0f, 100f);

        // 테스트용: 50% 확률로 1013번, 아니면 1001번 드랍
        // 일반
        if (roll < dropChances[rowIndex, 0])
        {
            return 1013; // 연마된 검 (RARE)
        }
        // 레어 (일반 + 레어)
        else if (roll < dropChances[rowIndex, 0] + dropChances[rowIndex, 1])
        {
            return 1001; // 낡은 검 (COMMON)
        }
        // 희귀 (일반 + 레어 + 희귀)
        else if (roll < dropChances[rowIndex, 0] + dropChances[rowIndex, 1] + dropChances[rowIndex, 2])
        {
            return 1001; // 낡은 검 (COMMON)
        }
        // 전설
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