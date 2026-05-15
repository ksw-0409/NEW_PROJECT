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

    [Header("✨ 투사체 설정")]
    [Tooltip("검기가 날아가는 초당 속도")]
    public float projectileSpeed = 18f;

    [Tooltip("검기 판정 박스의 두께(세로 폭)")]
    public float projectileThickness = 1.5f;
}
