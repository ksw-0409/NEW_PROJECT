using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public EquipmentData data; // 생성된 랜덤 스탯 데이터
    private SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Setup(EquipmentData newData)
    {
        data = newData;
        if (data.icon != null) sr.sprite = data.icon; // 엑셀에 적힌 아이콘 표시
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 플레이어의 인벤토리에 이 데이터를 추가하는 로직이 들어갈 자리입니다.
            Debug.Log($"{data.itemName} 획득! 공격력: {data.physicalDamage}");
            //K다. 

            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(data);
            }
            else
            {
                Debug.LogWarning("[FieldItem] Inventory.Instance가 없습니다.");
            }
            ItemManager.Instance.RemoveItem(gameObject);
            Destroy(gameObject);
        }
    }
}