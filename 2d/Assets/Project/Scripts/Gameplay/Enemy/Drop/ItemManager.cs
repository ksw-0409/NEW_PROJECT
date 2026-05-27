using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance; // �̱���
    [Header("������ ����")]
    public EquipmentManager equipManager; // ����Ƽ���� EquipmentManager ������Ʈ�� ����
    public GameObject equipmentPrefab;    // ��� ���� ������ (FieldItem ��ũ��Ʈ�� ���� ��)
    public GameObject player;
    // ���� �ʵ忡 ������ �ִ� �����۵� (�ڼ� ȿ�� ���� ���� ����)
    public List<GameObject> activeItems = new List<GameObject>();

    //���� ��� Ȯ��
    float[,] dropChances = {
    { 50f, 25f, 15f, 10f }, // 1~5��
    { 25f, 35f, 25f, 15f },  // 6~10��
    { 0f, 50f, 30f, 20f }  // ����
    };

    void Awake()
    {
        Instance = this;
    }

    public void RemoveItem(GameObject item)
    {
        activeItems.Remove(item);
    }

    // ���� ���� �� ȣ���� �Լ�
    public void DropItem(Vector2 position, float equipDropChance,bool isBoss)
    {     
        // ��� Ȯ�� ���
        if (Random.Range(0f, 100f) <= equipDropChance)
        {
            DropEquipment(position, isBoss);
        }
    }
    private void DropEquipment(Vector2 position, bool isBoss)
    {
        // ��� Ȯ�� ��� K ������
        int selectedID = GetRandomIDByWeight(isBoss);

        // ��� ������ ���� (���� ���� �ο���)
        EquipmentData randomData = equipManager.CreateItem(selectedID);

        // �ʵ忡 ��� ������Ʈ ����
        GameObject equipObj = Instantiate(equipmentPrefab, position, Quaternion.identity);
        equipObj.GetComponent<FieldItem>().Setup(randomData);
        activeItems.Add(equipObj);
    }
    // 등급 확률에 따라 등급을 정하고, 그 등급의 아이템 중 랜덤으로 하나 선택
    // dropChances 컬럼 = [Common, Rare, Unique, Legendary]
    private int GetRandomIDByWeight(bool isBoss)
    {
        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 1;
        int rowIndex = (floor <= 5) ? 0 : 1;
        if (isBoss) rowIndex = 2;

        // 등급 순서 (dropChances 컬럼 순서와 일치)
        ItemGrade[] gradeOrder = { ItemGrade.Common, ItemGrade.Rare, ItemGrade.Unique, ItemGrade.Legendary };

        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;
        ItemGrade chosenGrade = ItemGrade.Common;
        for (int g = 0; g < gradeOrder.Length; g++)
        {
            cumulative += dropChances[rowIndex, g];
            if (roll < cumulative) { chosenGrade = gradeOrder[g]; break; }
            if (g == gradeOrder.Length - 1) chosenGrade = gradeOrder[g]; // 폴백
        }

        // 선택된 등급의 아이템 중 랜덤 하나
        int id = equipManager.GetRandomIdByGrade(chosenGrade);

        // 해당 등급에 아이템이 없으면 Common으로 폴백
        if (id < 0) id = equipManager.GetRandomIdByGrade(ItemGrade.Common);
        if (id < 0) id = 1001; // 최종 안전장치

        return id;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float magnetDistance = 3.0f; // �ڼ� ����
        float moveSpeed = 10.0f;

        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            float dist = Vector2.Distance(activeItems[i].transform.position, player.transform.position);

            if (dist < magnetDistance)
            {
                // �÷��̾� �������� �̵�
                activeItems[i].transform.position = Vector2.MoveTowards(
                    activeItems[i].transform.position,
                    player.transform.position,
                    moveSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
}