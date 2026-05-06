using UnityEngine;

[CreateAssetMenu(menuName = "Skill/ArrowRain")]
public class ArrowRainData : SkillData
{
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
    public float homingRange = 15f;
}