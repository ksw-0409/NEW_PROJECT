using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelUpManager : MonoBehaviour
{
    public List<SkillData> allSkills;       // 전체 스킬 풀
    public SkillButton[] uiButtons;         // UI 버튼 3개
    public GameObject levelUpUI;            // UI 패널
    public PlayerStats playerStats;
    public PlayerSkillController skillController;

    void Start()
    {
        // 시작 시 스킬 선택창 띄우기
        ShowStartSkillUI();
    }

    private void OnEnable()
    {
        playerStats.OnLevelUp += ShowLevelUpUI;
    }

    private void OnDisable()
    {
        playerStats.OnLevelUp -= ShowLevelUpUI;
    }

    // 🟢 시작 스킬 선택
    public void ShowStartSkillUI()
    {
        levelUpUI.SetActive(true);
        Time.timeScale = 0f;

        ShowRandomSkills();
    }

    // 🟢 레벨업 시 호출
    public void ShowLevelUpUI()
    {
        levelUpUI.SetActive(true);
        Time.timeScale = 0f;

        ShowRandomSkills();
    }

    // 🎯 핵심 랜덤 로직
    private void ShowRandomSkills()
    {
        var availableSkills = allSkills
            .Where(skill =>
            {
                if (skill == null) return false;

                if (!skillController.HasSkill(skill))
                    return true;

                return !skillController.IsMaxLevel(skill);
            })
            .ToList();

        // 스킬이 부족하면 예외 처리
        if (availableSkills.Count == 0)
        {
            Debug.LogWarning("선택 가능한 스킬 없음");
            CloseUI();
            return;
        }

        var randomSkills = availableSkills
            .OrderBy(x => Random.value)
            .Take(3)
            .ToList();

        for (int i = 0; i < uiButtons.Length; i++)
        {
            if (i < randomSkills.Count)
            {
                uiButtons[i].gameObject.SetActive(true);
                uiButtons[i].Setup(randomSkills[i], this);
            }
            else
            {
                uiButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // 🟢 선택 시 호출
    public void OnSkillSelected(SkillData selected)
    {
        if (!skillController.HasSkill(selected))
        {
            skillController.AddNewSkill(selected);
        }
        else
        {
            skillController.LevelUpSkill(selected);
        }

        CloseUI();
    }

    private void CloseUI()
    {
        levelUpUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnPlayerDie()
    {
        // 인게임 스킬 인스턴스 초기화
        skillController.ResetSkills();

        // 하지만 SkillNode(스킬 트리)의 IsUnlocked는 유지됨
        Debug.Log("인게임 레벨 초기화. 스킬 트리 능력치는 보존됩니다.");
    }
}