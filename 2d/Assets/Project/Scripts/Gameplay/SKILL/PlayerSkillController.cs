using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    private List<SkillBase> skills = new List<SkillBase>();

    void Update()
    {
        foreach (var skill in skills)
        {
            skill.Tick(transform);
        }
    }

    public void AddSkill(SkillBase skill)
    {
        skills.Add(skill);
    }

    public void AddSlashSkill(SlashSkillData data, int treeLevel, GameObject effectPrefab)
    {
        SlashSkill skill = new SlashSkill();
        skill.Init(data, treeLevel, effectPrefab);

        AddSkill(skill);
    }
}