using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Fireball")]
public class FireballData : SkillData
{
    public GameObject projectilePrefab;
    public GameObject effectPrefab; // ⭐ 이거 추가해야 함
    public float projectileSpeed;
    public float explosionRadius;
}