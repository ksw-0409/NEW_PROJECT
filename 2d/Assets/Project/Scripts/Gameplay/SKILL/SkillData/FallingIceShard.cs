using UnityEngine;

public class FallingIceShard : MonoBehaviour
{
    private float damage;
    private float radius;
    private Vector3 targetPos;
    private bool hasImpacted = false;

    private float slowPercent;
    private float slowDuration;
    private bool  burstEnabled = false;  // ✨ IceRain_burst: 둔화/빙결 적이 죽으면 파편 폭발

    public float fallSpeed = 25f;

    // 호환 유지: 기존 5-인자 SetData
    public void SetData(float dmg, float rad, Vector3 target, float slowPct, float slowDur)
    {
        SetData(dmg, rad, target, slowPct, slowDur, false);
    }

    // ✨ 신규 6-인자 SetData (burst 효과 포함)
    public void SetData(float dmg, float rad, Vector3 target, float slowPct, float slowDur, bool burst)
    {
        damage = dmg;
        radius = rad;
        targetPos = target;
        slowPercent = slowPct;
        slowDuration = slowDur;
        burstEnabled = burst;
        hasImpacted = false;

        transform.right = Vector3.down;
    }

    void Update()
    {
        if (hasImpacted) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            fallSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            PerformImpact();
        }
    }

    private void PerformImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var target in targets)
        {
            if (!target.CompareTag("Enemy")) continue;

            EnemyHealth health =
                target.GetComponentInParent<EnemyHealth>() ??
                target.GetComponent<EnemyHealth>();

            if (health != null)
            {
                float hpBefore = health.currentHp;
                health.TakeDamage(damage);
                float hpAfter = health.currentHp;

                // ✨ [공격] 쇄빙 폭발: 둔화/빙결된 적이 이 데미지로 죽으면 주변에 파편 폭발
                if (burstEnabled && hpBefore > 0 && hpAfter <= 0)
                {
                    SpawnBurst(target.transform.position);
                }
            }

            EnemyAI ai =
                target.GetComponentInParent<EnemyAI>() ??
                target.GetComponent<EnemyAI>();

            if (ai != null)
            {
                float speedMul = Mathf.Clamp(1f - slowPercent, 0.1f, 1f);
              //  ai.ApplySlow(speedMul, slowDuration);
            }
        }

        Destroy(gameObject);
    }

    // ✨ 쇄빙 폭발 — 청백색 파편이 사방으로 튀며 주변 적에게 데미지
    void SpawnBurst(Vector3 pos)
    {
        float burstRadius = radius * 3f;
        float burstDamage = damage * 0.6f;

        // 시각: 외곽 청백 링
        var ringGo = new GameObject("IceBurstRing");
        ringGo.transform.position = pos;
        var lr = ringGo.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 24;
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0.7f, 0.95f, 1f, 0.95f);
        lr.endColor = new Color(0.5f, 0.85f, 1f, 0.9f);
        lr.sortingOrder = 100;
        for (int i = 0; i < lr.positionCount; i++)
        {
            float a = i * Mathf.PI * 2f / lr.positionCount;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * burstRadius, Mathf.Sin(a) * burstRadius, 0));
        }
        Destroy(ringGo, 0.35f);

        // 사방으로 튀는 파편 (8개)
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            var fragmentGo = new GameObject("IceFragment");
            fragmentGo.transform.position = pos;
            var fSr = fragmentGo.AddComponent<SpriteRenderer>();
            fSr.sprite = GetWhitePixelSprite();
            fSr.color = new Color(0.6f, 0.9f, 1f, 0.95f);
            fSr.sortingOrder = 99;
            fragmentGo.transform.localScale = Vector3.one * 0.15f;

            // 파편 이동 코루틴 (수명 동안 멀리 날아감)
            var mover = fragmentGo.AddComponent<IceFragmentMover>();
            mover.velocity = (Vector2)dir * burstRadius * 3f;
            mover.lifetime = 0.35f;

            Destroy(fragmentGo, 0.35f);
        }

        // 데미지 판정
        var hits = Physics2D.OverlapCircleAll(pos, burstRadius);
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            var enemy = h.GetComponentInParent<EnemyHealth>() ?? h.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(burstDamage);
        }

        Debug.Log($"[IceBurst] 쇄빙 폭발 at {pos}");
    }

    private static Sprite cachedWhitePixel;
    private static Sprite GetWhitePixelSprite()
    {
        if (cachedWhitePixel != null) return cachedWhitePixel;
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        cachedWhitePixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedWhitePixel;
    }
}

// ─── 헬퍼: 파편 이동 컴포넌트 ─────────
public class IceFragmentMover : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime;
    private float elapsed;

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;
        // 끝으로 갈수록 감속 + 페이드
        transform.position += (Vector3)(velocity * (1f - t) * Time.deltaTime);
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }
    }
}
