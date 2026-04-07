using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerData : ScriptableObject
{
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

    public float physicalDamage = 10f;
    public float magicDamage = 10f;
}