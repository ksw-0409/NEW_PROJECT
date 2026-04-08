using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public PlayerData data;

    public event Action OnLevelUp; // 레벨업 이벤트 

    public float currentHealth;
    public float currentLevel = 1;
    public float currentExp = 0;

    //data테이블에서 가져온 데이터들 
    public float MoveSpeed => data.moveSpeed;

    void Awake()
    {
        currentHealth = data.maxHealth;
    }

    public void TakeExp(float exp)
    {
        currentExp += exp;
        Debug.Log(currentExp);
        if (currentExp >= 2)
        {
            LevelUp();
        }
    }
    private void LevelUp()
    {
        currentLevel++;
        currentExp = 0;
        Debug.Log("Level++" + currentLevel);
        OnLevelUp?.Invoke();
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("사망");
        //추가
    }


}