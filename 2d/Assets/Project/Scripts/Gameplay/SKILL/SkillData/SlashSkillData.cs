using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Slash")]
public class SlashData : SkillData

{
    public float range;
    public float angle; // 👉 부채꼴 각도
    public GameObject effectPrefab;
}