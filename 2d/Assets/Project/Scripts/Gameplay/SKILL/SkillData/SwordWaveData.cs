using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SwordWave")]
public class SwordWaveData : SkillData
{
    public float cooldown;
    public float count;
    public float damage;
    public float multiplier;
    public float range;
    public GameObject effectPrefab;
}