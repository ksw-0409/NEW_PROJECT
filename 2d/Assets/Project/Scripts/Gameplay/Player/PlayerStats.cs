using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public PlayerData data;
    public event Action OnLevelUp; //레벨업 이벤트
    [HideInInspector] public float currentHealth; //HideInInspector는 public 이지만 엔진에서 안보이는
    public float currentLevel = 1;
    public float currentExp=0;
    public float nextLevelExp = 2;
    //data테이블에서 가져온 데이터들 
    public float CurrentMoveSpeed => data.moveSpeed;
    
    void Awake()
    {
        currentHealth = data.maxHealth;
    }

    public void GainExp(float amount)
    {
        currentExp += amount;
        if (currentExp >= nextLevelExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp -= nextLevelExp;
        nextLevelExp *= 1.2f; // 다음 레벨업에 필요한 경험치 증가

        Debug.Log("레벨업! 현재 레벨: " + currentLevel);

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
