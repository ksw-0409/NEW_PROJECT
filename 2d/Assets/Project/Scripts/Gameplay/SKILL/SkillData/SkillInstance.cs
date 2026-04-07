using UnityEngine;

public class SkillInstance
{
    public SkillData data;
    public int level = 1;

    public SkillInstance(SkillData data)
    {
        this.data = data;
    }

    public void LevelUp()
    {
        level++;
    }

    public bool IsMaxLevel()
    {
        return level >= data.levels.Length;
    }

    public SkillLevelData GetCurrentLevelData()
    {
        int index = Mathf.Clamp(level - 1, 0, data.levels.Length - 1);
        return data.levels[index];
    }
}