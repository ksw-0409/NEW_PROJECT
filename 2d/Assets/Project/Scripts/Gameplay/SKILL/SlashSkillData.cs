using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Slash")]
public class SlashSkillData : ScriptableObject
{
    public float baseDamage;
    public float cooldown;

    public float[] levelMultiplier;
    // ex) 1레벨: 0.6, 2레벨: 0.7 ...

    public float baseRange = 1.5f;
}