using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class LevelUpManager : MonoBehaviour
{
    public List<SkillData> allSkills; // 전체 스킬 리스트 
    public SkillButton[] uiButtons;    // 화면의 버튼 3개
    public PlayerStats playerStats;
    public GameObject LevelUpUI; //게임 버튼

    private void OnEnable()
    {
        playerStats.OnLevelUp += ShowLevelUpUI;
    }
    private void OnDisable()
    {
        playerStats.OnLevelUp -= ShowLevelUpUI;
    }
    public void ShowLevelUpUI()
    {
        Debug.Log("정지");
        LevelUpUI.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지

        // 중복 없이 랜덤 3개 추출 
        var randomSkills = allSkills.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();

        for (int i = 0; i < uiButtons.Length; i++)
        {
            uiButtons[i].Setup(randomSkills[i], this);
        }
    }

    public void OnSkillSelected(SkillData selected)
    {
        // 실제 플레이어 능력치더하기 or 스킬에 더하기 로직 추가 

        Debug.Log($"{selected.skillName} 적용 완료!");

        LevelUpUI.SetActive(false);
        Time.timeScale = 1f; // 게임 다시 시작
    }
}