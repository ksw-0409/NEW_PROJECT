using UnityEngine;

[CreateAssetMenu(menuName = "Skill/RotatingSlash")]
public class RotatingSlashData : SkillData
{
    public float range;
    public float angle;
    public float hitInterval;
    public GameObject effectPrefab;
}