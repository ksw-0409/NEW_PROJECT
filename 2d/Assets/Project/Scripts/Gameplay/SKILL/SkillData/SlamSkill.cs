using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class SlamSkill : SkillBase
{
    private SlamSkillData slamData;

    public GameObject effectPrefab;

    public void Init(SlamSkillData data, SkillInstance instance)
    {
        base.Init(instance);
        slamData = data;
    }

    protected override void Execute(Transform player)
    {
        if (slamData == null || instance == null) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        StartCoroutine(SlamRoutine(dir));
    }

    IEnumerator SlamRoutine(Vector2 dir)
    {
        for (int i = 0; i < GetCount(); i++)
        {
            Attack(dir);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private System.Collections.Generic.Dictionary<EnemyHealth, float> stunImmunity = new System.Collections.Generic.Dictionary<EnemyHealth, float>();

    void Attack(Vector2 dir)
    {
        float range = instance.GetCurrentLevelData().range;

        CameraShake.ShakePreset(CameraShake.Preset.Light); // ⭐ 흔들림 약화 (Heavy → Light)

        bool hasExecute = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Slam_execute");
        bool hasQuake   = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Slam_quake");
        bool hasThunder = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Slam_thunder");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (hit.transform.position - transform.position).normalized;
            float dot = Vector2.Dot(dir, toEnemy);
            float threshold = Mathf.Cos(slamData.angle * 0.5f * Mathf.Deg2Rad);
            if (dot < threshold) continue;

            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            float dmg = GetDamage();

            if (hasExecute && enemy.currentHp > 0)
            {
                if (enemy.currentHp <= dmg * 1.5f)
                {
                    dmg += enemy.currentHp * 100f;
                }
            }

            enemy.TakeDamage(dmg);

            var slow = hit.GetComponent<EnemySlow>();
            if (slow != null)
                slow.ApplySlow(slamData.slowPercent, slamData.slowDuration);

            if (hasThunder && !IsStunImmune(enemy))
            {
                var ai = hit.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    StartCoroutine(StunEnemy(enemy, ai, 1.5f, 5f));
                }
            }
        }

        float angleDeg = slamData != null ? slamData.angle : 90f;
        SkillRangeIndicator.SpawnSector(
            transform.position, dir, range, angleDeg,
            new Color(1f, 0.7f, 0.1f, 0.95f), 0.6f);

        SpawnEffect(dir, range);

        // ⭐ 라인하르트 스타일 땅 균열 이펙트 (사용자 요청)
        SlamCrackEffect.Spawn(transform.position, dir, slamData != null ? slamData.angle : 110f, range, 0.7f);

        // ⭐ 충격파+둔화 항상 발동 (변칙 체크 제거 — 사용자 요청)
        // ⭐ QuakeZone(갈색 존) 제거 — 사용자 요청. 슬로우는 위 hit 루프에서 직접 적용 중
    }

    bool IsStunImmune(EnemyHealth enemy)
    {
        if (stunImmunity.TryGetValue(enemy, out float until))
        {
            if (Time.time < until) return true;
            stunImmunity.Remove(enemy);
        }
        return false;
    }

    System.Collections.IEnumerator StunEnemy(EnemyHealth enemy, EnemyAI ai, float stunDur, float immunityDur)
    {
        if (enemy == null || ai == null) yield break;
        bool prev = ai.enabled;
        ai.enabled = false;

        var sr = enemy.GetComponentInChildren<SpriteRenderer>();
        Color origColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(1f, 1f, 0.3f, 1f);

        yield return new WaitForSeconds(stunDur);

        if (ai != null) ai.enabled = prev;
        if (sr != null) sr.color = origColor;

        stunImmunity[enemy] = Time.time + immunityDur;
    }

    void SpawnQuakeZone(Vector3 center, float radius, float duration, float slowPct, float slowDur)
    {
        var quakeGo = new GameObject("QuakeZone");
        quakeGo.transform.position = center;
        var col = quakeGo.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;

        var zone = quakeGo.AddComponent<QuakeZone>();
        zone.duration = duration;
        zone.slowPercent = slowPct;
        zone.slowDuration = slowDur;

        for (int i = 0; i < 3; i++)
        {
            var crackGo = new GameObject("Crack_" + i);
            crackGo.transform.SetParent(quakeGo.transform, false);
            var lr = crackGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 18;
            lr.startWidth = 0.07f; lr.endWidth = 0.07f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.5f, 0.3f, 0.1f, 0.9f);
            lr.endColor = new Color(0.7f, 0.4f, 0.15f, 0.7f);
            lr.sortingOrder = 30 + i;
            float ringR = radius * (0.4f + i * 0.3f);
            for (int j = 0; j < lr.positionCount; j++)
            {
                float a = j * Mathf.PI * 2f / lr.positionCount + i * 0.3f;
                lr.SetPosition(j, new Vector3(Mathf.Cos(a) * ringR, Mathf.Sin(a) * ringR, 0));
            }
        }

        Destroy(quakeGo, duration);
    }

    void SpawnEffect(Vector2 dir, float range)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab);
        fx.transform.position = transform.position + (Vector3)(dir * range * 0.5f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        var matcher = fx.GetComponent<SkillRangeMatcher>();
        if (matcher == null) matcher = fx.AddComponent<SkillRangeMatcher>();
        matcher.activeRatio = 0.83f;
        matcher.ApplyRect(range, range * 0.6f);

        Destroy(fx, 0.5f);
    }

    // ⭐ 기즈모: 실제 피격판정(OverlapCircle range + angle 부채꼴)과 일치 — 선택 시와 항상 둘 다 표시
    void OnDrawGizmosSelected()
    {
        DrawSlamGizmo();
    }

    void OnDrawGizmos()
    {
        DrawSlamGizmo();
    }

    private void DrawSlamGizmo()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.7f);

        float angle = slamData != null ? slamData.angle : 90f;
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