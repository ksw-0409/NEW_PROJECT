using UnityEngine;

[CreateAssetMenu(menuName = "Skill/Fireball")]
public class FireballData : SkillData
{
    public GameObject projectilePrefab;
    public GameObject effectPrefab;
    public float projectileSpeed;
    public float explosionRadius; // ◀ 유니티 인스펙터에서 5 -> 10으로 고친 그 값
    public GameObject subProjectilePrefab;
    public float homingRange = 15f;
}