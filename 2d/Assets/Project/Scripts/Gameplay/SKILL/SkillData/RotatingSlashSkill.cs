using UnityEngine;
using System.Collections;

public class RotatingSlashSkill : SkillBase
{
    public GameObject effectPrefab;
    private RotatingSlashData rotData;
    private bool isLoopRunning = false;

    public void Init(RotatingSlashData data, SkillInstance instance)
    {
        this.data = data;
        this.instance = instance;
        this.rotData = data;
        // ⭐ AutoCast 제거 — 회전베기는 "상시 회전" 시스템으로 (사용자 요청)
        // ⭐ 중복 시작 방지 (Init이 또 호출되어도 코루틴은 1개만)
        if (!isLoopRunning)
        {
            isLoopRunning = true;
            StartCoroutine(ContinuousAttackLoop());
        }
    }

    // 상시 공격이라 AutoCast/Execute는 안 쓰지만, SkillBase abstract이라 빈 구현 필요
    protected override void Execute(Transform player) { /* unused */ }

    /// <summary>회전베기 칼날이 일정 간격으로 계속 돌면서 주변에 데미지 (cooldown 무시)</summary>
    /// <summary>회전베기 통합 루프 — 칼날오라 찍었으면 상시 회전 모드, 안 찍었으면 쿨다운 모드</summary>
    IEnumerator ContinuousAttackLoop()
    {
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            // ⭐ 매 루프마다 칼날오라 specialty 보유 여부 체크 (게임 도중 찍으면 자동 전환)
            bool hasAura = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("RotSlash_aura");

            if (instance != null && HasEnemiesToAttack()) Attack();

            float waitTime;
            if (hasAura)
            {
                // 칼날오라 모드: hitInterval마다 상시 회전
                var rsBonus = PlayerStats.Instance != null && data != null ? PlayerStats.Instance.GetSkillBonus(data) : (dmg:1f, rng:1f, cool:1f, cnt:0, slowMul:1f, durMul:1f);
                waitTime = (rotData != null && rotData.hitInterval > 0f) ? Mathf.Max(0.3f, rotData.hitInterval * rsBonus.cool) : 0.6f;
            }
            else
            {
                // 일반 모드: GetCooldown()마다 1회 데미지
                waitTime = Mathf.Max(0.5f, GetCooldown());
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    // ✨ 특수효과 상태 변수
    private int crushStackCount = 0;
    private float crushStackResetTime = 0f;
    private const float CrushStackDuration = 5f;
    private const float CrushStackMaxBonus = 0.5f;

    void Attack()
    {
        // CameraShake 제거 — 사용자 요청 (화면 흔들림 줄이기)

        float range = GetRange() * 1.5f; // 이펙트 범위 (그대로)
        float hitRange = range * 1.4f; // ⭐ 피격 범위 40% 더 크게 (이펙트 끝에 닿아도 데미지)
        float dmg = GetDamage(); // SkillBase에서 bonus.dmg 자동 적용

        bool hasCrush = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("RotSlash_crush");
        bool hasBlackhole = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("RotSlash_blackhole");

        if (hasCrush && Time.time > crushStackResetTime)
            crushStackCount = 0;
        float crushBonus = hasCrush ? Mathf.Min(crushStackCount * 0.05f, CrushStackMaxBonus) : 0f;
        float finalDmg = dmg * (1f + crushBonus);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRange);
        int enemyHitCount = 0;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyHealth>()?.TakeDamage(finalDmg);
                enemyHitCount++;
                if (hasBlackhole)
                {
                    var enemyRb = hit.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        Vector2 pullDir = ((Vector2)transform.position - (Vector2)hit.transform.position).normalized;
                        enemyRb.AddForce(pullDir * 10f, ForceMode2D.Impulse);
                    }
                }
            }
        }

        if (hasCrush && enemyHitCount >= 5)
        {
            crushStackCount = Mathf.Min(crushStackCount + 1, 10);
            crushStackResetTime = Time.time + CrushStackDuration;
        }

        SpawnEffect(range);
    }

    void SpawnEffect(float range)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
        effect.transform.localPosition = Vector3.zero;

        var rb = effect.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        var matcher = effect.GetComponent<SkillRangeMatcher>();
        if (matcher == null) matcher = effect.AddComponent<SkillRangeMatcher>();
        matcher.activeRatio = 1.0f;
        matcher.ApplyRadius(range);

        float lifetime = rotData != null ? Mathf.Max(0.3f, rotData.hitInterval) : 0.3f;
        Destroy(effect, lifetime);
    }

    void OnDrawGizmos()
    {
        if (instance == null) return;
        float range = instance.GetCurrentLevelData().range;
        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}