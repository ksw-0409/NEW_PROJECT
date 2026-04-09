using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float maxHealth = 100f; // 최대 체력 수치 (예: 100 = 100 HP)
    public float moveSpeed = 5f; // 이동 속도 수치 (예: 5 = 5 유닛/초)
    public float physicalDefense = 0f; // 방어력 수치 (예: 20 = 20 방어력)
    public float physicalDamage = 5f; // 물리 공격력 수치
    public float magicDamage = 10f; // 마법 공격력 수치
    public float criticalChance = 0.1f; // 10% 확률
    public float criticalDamage = 1.5f; // 크리티컬 시 1.5배 피해
    public float defenseRate = 0.0f; // 방어력 감소 수치 (예: 0.2f = 20% 감소)
    public float attackcooldown = 1f; // 공격 쿨타임 (예: 1배율) 쿨타임 줄이면 스킬 쿨타임이 줄어듬 ( 0.8 * 기존 스킬 쿨타임 = 0.8배율로 스킬 쿨타임 감소)

}