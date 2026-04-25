using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public int id; //ID
    public string Name; //이름
    public int Rank; //등급 (일반0 정예1)
    public float hp;         //체력
    public float damage; //플레이어한테 주는 데미지 
    public float Defense; //방어력

    public float DropGold; // 골드 드랍
    public float DropExp; //경험치 드랍 소1 중2 대3 특대4
    public float DropWeapon; //무기 드랍 
    public float DropNormalEnhancement; //일반강화아이템 

    public int[] floor; //등장 층
    public float moveSpeed; //이동속도
}
