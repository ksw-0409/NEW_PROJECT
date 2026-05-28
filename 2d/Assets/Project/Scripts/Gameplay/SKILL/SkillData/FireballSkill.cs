using UnityEngine;
using UnityEngine.InputSystem;

public class FireballSkill : SkillBase
{
    // ⭐ 디버그 로그 토글: 빌드 디버깅 필요할 때 true
    private const bool LOG_VERBOSE = false;

    private FireballData fireData;

    public void Init(FireballData data, SkillInstance instance)
    {
        base.Init(instance);
        this.fireData = data;
    }

    protected override void Execute(Transform player)
    {
        print("파이어볼 준비");
        if (fireData == null || instance == null) return;
        if (player == null) return;

        var bonus = PlayerStats.Instance != null
            ? PlayerStats.Instance.GetSkillBonus(fireData)
            : (dmg: 1f, rng: 1f, cool: 1f, cnt: 0, slowMul: 1f, durMul: 1f);

        Vector2 mouseDir = GetShootDirection(player);

        int totalShootCount = instance.GetCurrentLevelData().count + bonus.cnt;
        if (totalShootCount <= 0) totalShootCount = 1;

        for (int i = 0; i < totalShootCount; i++)
        {
            Shoot(player, mouseDir, bonus);
        }
    }

    private Vector2 GetShootDirection(Transform player)
    {
        Vector2 mouseDir = Vector2.right;
        bool gotMouseDir = false;

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = Object.FindAnyObjectByType<Camera>();
        }

        if (cam != null)
        {
            try
            {
                if (Mouse.current != null)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    if (TryScreenToDir(cam, player, screenPos, out Vector2 dir))
                    {
                        mouseDir = dir;
                        gotMouseDir = true;
                    }
                }
            }
            catch { /* swallow */ }

            if (!gotMouseDir)
            {
                try
                {
                    Vector3 legacyPos = Input.mousePosition;
                    if (TryScreenToDir(cam, player, legacyPos, out Vector2 dir))
                    {
                        mouseDir = dir;
                        gotMouseDir = true;
                    }
                }
                catch { /* swallow */ }
            }
        }

        if (!gotMouseDir)
        {
            Transform fallbackTarget = FindNearestEnemy(player.position, 20f);
            if (fallbackTarget != null)
            {
                mouseDir = ((Vector2)fallbackTarget.position - (Vector2)player.position).normalized;
            }
            else
            {
                mouseDir = player.localScale.x >= 0 ? Vector2.right : Vector2.left;
            }
        }

        return mouseDir;
    }

    private bool TryScreenToDir(Camera cam, Transform player, Vector3 screenPos, out Vector2 dir)
    {
        dir = Vector2.right;
        screenPos.z = Mathf.Abs(cam.transform.position.z - player.position.z);
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        Vector2 raw = (Vector2)world - (Vector2)player.position;
        if (raw.sqrMagnitude > 0.01f)
        {
            dir = raw.normalized;
            return true;
        }
        return false;
    }

    void Shoot(Transform player, Vector2 mouseDir, (float dmg, float rng, float cool, int cnt, float slowMul, float durMul) bonus)
    {
        if (fireData.projectilePrefab == null)
        {
            if (LOG_VERBOSE) print("[Fireball] projectilePrefab이 null");
            return;
        }
        print("파이어볼 발사!");

        var levelData = instance.GetCurrentLevelData();
        GameObject obj = Instantiate(fireData.projectilePrefab, player.position, Quaternion.identity);

        float currentScale = Mathf.Max(0.5f, levelData.explosionRadius * 0.6f);
        obj.transform.localScale = new Vector3(currentScale, currentScale, 1f);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        Vector2 finalDir = mouseDir;

        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Fireball_1_2"))
        {
            Transform target = FindNearestEnemy(player.position, 15f);
            if (target != null) finalDir = ((Vector2)target.position - (Vector2)player.position).normalized;
        }

        if (rb != null)
        {
            rb.linearVelocity = finalDir * levelData.projectileSpeed;
        }

        float angleDeg = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        FireballProjectile proj = obj.GetComponent<FireballProjectile>();
        if (proj != null)
        {
            float finalDamage = (levelData.damage * levelData.multiplier) * bonus.dmg;
            var shieldSkill = GetComponent<ShieldSkill>();
            if (shieldSkill != null) finalDamage *= shieldSkill.MagicDamageBonus;
            float finalExplosionRange = levelData.explosionRadius * bonus.rng;

            if (finalDamage <= 0f) finalDamage = 10f;
            if (finalExplosionRange <= 0.1f) finalExplosionRange = 1.5f;

            proj.Init(finalDamage, finalExplosionRange, fireData.effectPrefab, currentScale);
        }
        else
        {
            Destroy(obj);
            return;
        }

        if (rb != null && rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = finalDir * 8f;
        }
    }

    Transform FindNearestEnemy(Vector2 pos, float range)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, range);
        Transform nearest = null;
        float minCDist = Mathf.Infinity;
        foreach (var e in enemies)
        {
            if (e.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(pos, e.transform.position);
                if (dist < minCDist) { minCDist = dist; nearest = e.transform; }
            }
        }
        return nearest;
    }
}
