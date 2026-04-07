using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SlamSkillData")]
public class SlamSkillData : SkillData
{
    public float range;
    public float angle;

    public float slowPercent;   // ⭐ 0.3 = 30%
    public float slowDuration;
}