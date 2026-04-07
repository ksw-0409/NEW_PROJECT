[System.Serializable]
public class SkillLevelData
{
    public float cooldown;   // 발동 주기
    public int count;        // 횟수
    public float damage;     // 데미지
    public float multiplier; // 배율

    public float range;           // 근접/범위용
    public float angle;           // 부채꼴용
    public float projectileSpeed; // 투사체용
    public float explosionRadius; // 폭발용

    public float slowPercent;     // 상태이상
    public float slowDuration;

    public float duration;     // 지속시간 (메테오, 방패 등)
    public float tickInterval; // 도트딜 간격
    public int pierce;         // 관통 수
}