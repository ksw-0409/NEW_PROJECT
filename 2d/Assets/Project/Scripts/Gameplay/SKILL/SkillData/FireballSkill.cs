using UnityEngine;
using UnityEngine.InputSystem;

public class FireballSkill : SkillBase
{
    private FireballData fireData;

    public void Init(FireballData data, SkillInstance instance)
    {
        base.Init(instance);
        this.fireData = data;
    }

    protected override void Execute(Transform player)
    {
        if (fireData == null || instance == null)
        {
            Debug.LogWarning("[Fireball] fireData or instance is null — skipping shot");
            return;
        }

        var bonus = PlayerStats.Instance.GetSkillBonus(fireData);

        // ✨ 마우스 입력 null 가드 — 마우스가 없거나 카메라가 없을 때는
            // 가장 가까운 적 혹은 플레이어 바라보는 방향으로 발사
        Vector2 mouseDir = Vector2.right;
        bool gotMouseDir = false;

        if (Camera.main != null && Mouse.current != null)
        {
            try
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                Vector2 raw = mousePos - (Vector2)player.position;
                if (raw.sqrMagnitude > 0.01f)
                {
                    mouseDir = raw.normalized;
                    gotMouseDir = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Fireball] mouse read failed: " + e.Message);
            }
        }

        // 마우스 방향 획득 실패 시 폴백: 가장 가까운 적 혹은 플레이어 향 반향
        if (!gotMouseDir)
        {
            Transform fallbackTarget = FindNearestEnemy(player.position, 20f);
            if (fallbackTarget != null)
                mouseDir = ((Vector2)fallbackTarget.position - (Vector2)player.position).normalized;
            else
                mouseDir = (player.localScale.x >= 0 ? Vector2.right : Vector2.left); // 플레이어 바라보는 방향
        }

        int totalShootCount = instance.GetCurrentLevelData().count + bonus.cnt;

        for (int i = 0; i < totalShootCount; i++)
        {
            Shoot(player, mouseDir, bonus);
        }
    }

    void Shoot(Transform player, Vector2 mouseDir, (float dmg, float rng, float cool, int cnt, float slowMul, float durMul) bonus)
    {
        if (fireData.projectilePrefab == null) return;

        var levelData = instance.GetCurrentLevelData();
        GameObject obj = Instantiate(fireData.projectilePrefab, player.position, Quaternion.identity);

        // ⭐ 발사체 비주얼 크기 — 이제는 적당한 크기(콜라이더 반지름 때문에)
        // 발사체는 자체 스프라이트가 있고 적이 아고는 OnTriggerEnter로 트리거만 함.
        // 폭발 판정(explosionRadius)이 실제 스킬 펠교단은 따로이므로, 발사체는 0.6배 정도로 관리 가능한 크기
        float currentScale = Mathf.Max(0.5f, levelData.explosionRadius * 0.6f);
        obj.transform.localScale = new Vector3(currentScale, currentScale, 1f);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        Vector2 finalDir = mouseDir;

        if (PlayerStats.Instance.HasSpecialty("Fireball_1_2"))
        {
            Transform target = FindNearestEnemy(player.position, 15f);
            if (target != null) finalDir = ((Vector2)target.position - (Vector2)player.position).normalized;
        }

        if (rb != null)
        {
            rb.linearVelocity = finalDir * levelData.projectileSpeed;
        }

        // 발사체 스프라이트 회전: 진행 방향에 맞춤
        float angleDeg = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.Euler(0, 0, angleDeg);

        FireballProjectile proj = obj.GetComponent<FireballProjectile>();
        if (proj != null)
        {
            float finalDamage = (levelData.damage * levelData.multiplier) * bonus.dmg;
            // [방어] 마력 과부하: 방패 활성 중 마법 데미지 +30%
            var shieldSkill = GetComponent<ShieldSkill>();
            if (shieldSkill != null) finalDamage *= shieldSkill.MagicDamageBonus;
            float finalExplosionRange = levelData.explosionRadius * bonus.rng;

            proj.Init(finalDamage, finalExplosionRange, fireData.effectPrefab, currentScale);
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