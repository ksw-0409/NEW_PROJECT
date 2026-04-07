using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public string description;

    public SkillLevelData[] levels;
}