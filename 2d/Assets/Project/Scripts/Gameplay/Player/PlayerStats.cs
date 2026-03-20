using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public PlayerData data;

    [HideInInspector] public float currentHealth; //HideInInspector는 public 이지만 엔진에서 안보이는
    public float currentLevel = 1;
    public float currentExp=0;

    //data테이블에서 가져온 데이터들 
    public float MoveSpeed => data.moveSpeed;
    
    void Awake()
    {
        currentHealth = data.maxHealth;
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
