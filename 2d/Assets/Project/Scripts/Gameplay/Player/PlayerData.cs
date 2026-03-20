using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;

}