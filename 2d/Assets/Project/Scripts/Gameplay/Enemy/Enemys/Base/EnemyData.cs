using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string Name; //이름
    public int Rank; //등급 (일반0 정예1)
    public int Level;  //레벨
    public float hp;         //체력
    public float damage; //플레이어한테 주는 데미지 
    public float Defense; //방어력
    public float moveSpeed; //이동속도
    public float expGrant; // 죽었을 때 주는 경험치
}
