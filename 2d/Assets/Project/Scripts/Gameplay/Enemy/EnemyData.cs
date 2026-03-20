using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float hp=10;
    public float moveSpeed=3;
    public float damage; //플레이어한테 주는 데미지 
    public float expGrant; // 죽었을 때 주는 경험치
}
