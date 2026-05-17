using UnityEngine;
using System.Collections;

public class RotatingSlashSkill : SkillBase
{
    public GameObject effectPrefab;
    private RotatingSlashData rotData;

    public void Init(RotatingSlashData data, SkillInstance instance)
    {
        this.data = data;
        this.instance = instance;
        this.rotData = data;
        StartCoroutine(AutoCast());
    }

    protected override void Execute(Transform player)
    {
        StartCoroutine(SlashRoutine());
    }

    // ✨ [변칙] 칼날 오라: 액티브 대신 주변에 상시 데미지 (초당 1/10)
    private float auraTickTimer = 0f;
    private const float AuraTickInterval = 1f;

    void Update()
    {
        if (instance == null) return;
        if (PlayerStats.Instance == null) return;
        if (!PlayerStats.Instance.HasSpecialty("RotSlash_aura")) return;

        auraTickTimer += Time.deltaTime;
        if (auraTickTimer < AuraTickInterval) return;
        auraTickTimer = 0f;

        float range = instance.GetCurrentLevelData().range;
        float dmg = GetDamage() * 0.1f; // 초당 기본 데미지의 1/10

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyHealth>()?.TakeDamage(dmg);
            }
        }

        // 아주 가볍운 시각 표시 (청백 세는 링)
        var ringGo = new GameObject("AuraTickRing");
        ringGo.transform.position = transform.position;
        var lr = ringGo.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 32;
        lr.startWidth = 0.04f; lr.endWidth = 0.04f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.5f, 0.85f, 1f, 0.4f);
        lr.endColor = new Color(0.5f, 0.85f, 1f, 0.4f);
        lr.sortingOrder = 40;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / lr.positionCount;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * range, Mathf.Sin(a) * range, 0));
        }
        Destroy(ringGo, 0.25f);
    }

    IEnumerator SlashRoutine()
    {
        for (int i = 0; i < GetCount(); i++)
        {
            Attack();
            yield return new WaitForSeconds(rotData.hitInterval);
        }
    }

    // ✨ 특수효과 상태 변수
    private int crushStackCount = 0;
    private float crushStackResetTime = 0f;
    private const float CrushStackDuration = 5f;
    private const float CrushStackMaxBonus = 0.5f; // 최대 +50%

    void Attack()
    {
        // 임팩트 흔들림
        CameraShake.ShakePreset(CameraShake.Preset.Light);

        float range = instance.GetCurrentLevelData().range;
        float dmg = GetDamage();

        // ✨ 특수효과 체크
        bool hasCrush = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("RotSlash_crush");
        bool hasBlackhole = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("RotSlash_blackhole");
        // RotSlash_aura는 Init에서 관리

        // ✨ [공격] 분쇄: 5명 이상 적중 시 5%씩 중첩 (최대 +50%, 5초 유지)
        if (hasCrush && Time.time > crushStackResetTime)
        {
            crushStackCount = 0; // 시간 경과 대기열 초기화
        }
        float crushBonus = hasCrush ? Mathf.Min(crushStackCount * 0.05f, CrushStackMaxBonus) : 0f;
        float finalDmg = dmg * (1f + crushBonus);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        int enemyHitCount = 0;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyHealth>()?.TakeDamage(finalDmg);
                enemyHitCount++;

                // ✨ [유틸] 블랙홀: 주변 적을 자신에게 끌어당김
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

        // 분쇄 스택 증가 조건은 5명 이상 적중
        if (hasCrush && enemyHitCount >= 5)
        {
            crushStackCount = Mathf.Min(crushStackCount + 1, 10); // 최대 10스택 = +50%
            crushStackResetTime = Time.time + CrushStackDuration;
        }

        // 피격범위 가시화
        SkillRangeIndicator.Spawn(
            transform.position,
            range,
            new Color(0.3f, 0.85f, 1f, 0.95f),
            0.35f,
            SkillRangeIndicator.Shape.Circle
        );

        SpawnEffect(range);
    }

    void SpawnEffect(float range)
    {
        if (effectPrefab == null) return;
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
        const float spriteNative = 0.96f;
        const float activeRatio = 0.30f;
        // 이펙트 크기 = 데미지 판정 영역(지름 = range*2)과 일치
        float finalScale = (range * 2f) / spriteNative; // active ratio 제거
        effect.transform.localScale = new Vector3(finalScale, finalScale, 1f);
        Destroy(effect, 0.3f);
    }

    // 기즈모 색상을 검은색으로 변경
    // ⭐ 기즈모: 실제 피격판정(OverlapCircle range)과 완벽 일치
    void OnDrawGizmos()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, range);

        float angle = (rotData != null) ? rotData.angle : 360f;
        if (angle < 360f)
        {
            Vector3 forward = transform.right;
            int step = 20;
            for (int i = 0; i <= step; i++)
            {
                float currentAngle = -angle / 2 + (angle / step) * i;
                Vector3 dir = Quaternion.Euler(0, 0, currentAngle) * forward;
                Gizmos.DrawLine(transform.position, transform.position + dir * range);
            }
        }
    }
}