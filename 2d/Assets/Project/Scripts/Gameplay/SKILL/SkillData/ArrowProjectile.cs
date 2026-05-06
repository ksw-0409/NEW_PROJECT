using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArrowProjectile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject fireFieldPrefab;

    [Header("기본 화살 스프라이트 (패시브 미발동 시 이 이미지 사용)")]
    public Sprite defaultArrowSprite;
    public Color  defaultArrowColor = Color.white;

    [Header("Life Settings")]
    public float lifeTime = 3f;

    // ─── 내부 상태 ───────────────────────────
    private float baseDamage;
    private float extraDamage   = 0;
    private int   pierceCount   = 0;
    private float damageDecay   = 1.0f;

    private List<string> appliedPassives = new List<string>();

    private float explosionRadius    = 0;
    private float poisonDmgPerTick   = 0;
    private float poisonDuration     = 0;
    private float iceSlowAmount      = 0;
    private float iceDuration        = 0;

    private bool   useAllowedArea;
    private Vector2 allowedAreaCenter;
    private float  allowedAreaRadius;

    // ─── 초기화 ──────────────────────────────
    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 기본 스프라이트 저장 (Awake 시점의 스프라이트를 기본값으로)
        if (defaultArrowSprite == null && spriteRenderer != null)
            defaultArrowSprite = spriteRenderer.sprite;
        if (defaultArrowColor == Color.white && spriteRenderer != null)
            defaultArrowColor = spriteRenderer.color;
    }

    void Start() => Destroy(gameObject, lifeTime);

    // ─── 세팅 ────────────────────────────────
    public void Setup(float baseDmg, float multiplier, float speed, Vector2 dir)
    {
        baseDamage = baseDmg * multiplier;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = dir * speed;

        // 기본 이미지로 초기화 (이전 프레임 잔상 방지)
        ResetVisual();
    }

    public void SetPierce(SkillLevelData ld)
    {
        pierceCount = Mathf.RoundToInt(ld.count);
        damageDecay = ld.multiplier;
    }

    public void SetAllowedArea(Vector2 center, float radius)
    {
        useAllowedArea    = true;
        allowedAreaCenter = center;
        allowedAreaRadius = Mathf.Max(0.1f, radius);
    }

    // ─── 패시브 추가 ─────────────────────────
    // ★ 핵심: 확률 판정은 BowSkill에서 이미 통과한 것만 여기까지 옴
    //         → 스프라이트/색상 변경은 여기서 확정
    public void AddPassive(SkillLevelData ld, string type, Color color, Sprite newSprite = null)
    {
        if (appliedPassives.Contains(type)) return;
        appliedPassives.Add(type);

        // 가장 먼저 붙은 패시브가 이미지 결정 (우선순위: Ice > Explosion > Poison)
        bool isFirst = appliedPassives.Count == 1;
        if (isFirst && spriteRenderer != null)
        {
            // 패시브 전용 스프라이트가 있으면 변경, 없으면 색상만 변경
            if (newSprite != null) spriteRenderer.sprite = newSprite;
            spriteRenderer.color = color;
        }

        switch (type)
        {
            case "Explosion":
                explosionRadius = ld.explosionRadius;
                extraDamage    += ld.damage * ld.multiplier;
                break;
            case "Poison":
                float totalStat  = PlayerStats.Instance.data.physicalDamage
                                 + PlayerStats.Instance.data.magicDamage;
                poisonDmgPerTick = ld.damage + totalStat * 0.1f;
                poisonDuration   = ld.duration;
                break;
            case "Ice":
                iceSlowAmount = ld.multiplier;
                iceDuration   = ld.duration;
                break;
        }
    }

    // ─── 시각 초기화 ─────────────────────────
    // 패시브가 없을 때(기본 화살) → 기본 스프라이트·색상 복원
    public void ResetVisual()
    {
        if (spriteRenderer == null) return;
        if (defaultArrowSprite != null) spriteRenderer.sprite = defaultArrowSprite;
        spriteRenderer.color = defaultArrowColor;
    }

    // ─── 충돌 ────────────────────────────────
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;

        if (useAllowedArea)
        {
            float dist = Vector2.Distance(col.transform.position, allowedAreaCenter);
            if (dist > allowedAreaRadius) return;
        }

        var enemy = col.GetComponent<EnemyHealth>();
        if (enemy == null) return;

        enemy.TakeDamage(baseDamage + extraDamage);

        if (appliedPassives.Contains("Explosion")) DoExplosion(col.transform.position);
        if (appliedPassives.Contains("Poison") && enemy.gameObject.activeInHierarchy) ApplyPoison(enemy);
        if (appliedPassives.Contains("Ice")) ApplyIce(enemy);

        if (pierceCount > 0) { pierceCount--; baseDamage *= damageDecay; }
        else Destroy(gameObject);
    }

    // ─── 폭발 ────────────────────────────────
    void DoExplosion(Vector2 pos)
    {
        if (fireFieldPrefab != null)
            Destroy(Instantiate(fireFieldPrefab, pos, Quaternion.identity), 1f);

        foreach (var col in Physics2D.OverlapCircleAll(pos, explosionRadius))
            if (col.CompareTag("Enemy"))
                col.GetComponent<EnemyHealth>()?.TakeDamage(extraDamage);
    }

    // ─── 독 ──────────────────────────────────
    void ApplyPoison(EnemyHealth enemy)
    {
        if (enemy.gameObject.activeInHierarchy)
            enemy.StartCoroutine(PoisonRoutine(enemy));
    }

    IEnumerator PoisonRoutine(EnemyHealth enemy)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && enemy != null && enemy.gameObject.activeInHierarchy)
        {
            enemy.TakeDamage(poisonDmgPerTick);
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    // ─── 빙결 ────────────────────────────────
    void ApplyIce(EnemyHealth enemy)
    {
        var mv = enemy.GetComponent<EnemyAI>();
        if (mv != null) mv.ApplySlow(iceSlowAmount, iceDuration);
        Debug.Log($"{enemy.name} 빙결 둔화 {iceSlowAmount*100:F0}%");
    }
}
