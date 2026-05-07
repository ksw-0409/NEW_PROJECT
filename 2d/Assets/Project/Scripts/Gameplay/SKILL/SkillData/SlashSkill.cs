using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SlashSkill : SkillBase
{
    public GameObject effectPrefab;

    // 혈투: 출혈 DoT 추적
    private System.Collections.Generic.Dictionary<EnemyHealth, Coroutine> bleedCoroutines = new();

    protected override void Execute(Transform player)
    {
        var levelData = instance.GetCurrentLevelData();
        var bonus = PlayerStats.Instance.GetSkillBonus(instance.data);

        float range = levelData.range * bonus.rng;
        float angle = levelData.angle;
        float damage = levelData.damage * levelData.multiplier * bonus.dmg;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mousePos - (Vector2)player.position).normalized;

        SpawnEffect(player, dir, range);

        // ⭐ 피격범위 가시화: 전방 부채꼴 (Slash는 코드상 OverlapCircle이지만 dot threshold로 부채꼴 필터링)
        SkillRangeIndicator.SpawnSector(
            player.position,
            dir,
            range,
            angle,
            new Color(1f, 0.85f, 0.2f, 0.95f),
            0.4f
        );

        // [변칙] 패링 베기: 주변 투사체 제거
        if (PlayerStats.Instance.HasSpecialty("SL_parry"))
        {
            var projectiles = Physics2D.OverlapCircleAll(player.position, range, LayerMask.GetMask("Projectile"));
            foreach (var p in projectiles) Destroy(p.gameObject);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, range);
        bool hasPhantom = PlayerStats.Instance.HasSpecialty("SL_phantom");
        bool hasBloodbath = PlayerStats.Instance.HasSpecialty("SL_bloodbath");

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (hit.transform.position - player.position).normalized;
            float dot = Vector2.Dot(dir, toEnemy);
            float threshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);
            if (dot < threshold) continue;

            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy == null) continue;

            enemy.TakeDamage(damage);

            // [공격] 잔상
            if (hasPhantom && Random.value < 0.5f)
                enemy.TakeDamage(damage);

            // [유틸] 혈투
            if (hasBloodbath)
                ApplyBleed(enemy, damage);
        }
    }

    void ApplyBleed(EnemyHealth enemy, float damage)
    {
        if (enemy == null) return;
        if (bleedCoroutines.ContainsKey(enemy) && bleedCoroutines[enemy] != null)
            StopCoroutine(bleedCoroutines[enemy]);
        var co = StartCoroutine(BleedRoutine(enemy, damage * 0.2f, 5f));
        bleedCoroutines[enemy] = co;
    }

    IEnumerator BleedRoutine(EnemyHealth enemy, float dotDamage, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
            if (enemy == null) yield break;

            var enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth == null) yield break;

            bool wasDead = enemyHealth.currentHp <= 0;
            enemy.TakeDamage(dotDamage);

            // [유틸] 혈투: 출혈로 적 처치 시 체력 2% 회복
            if (!wasDead && enemyHealth.currentHp <= 0)
            {
                float healAmount = PlayerStats.Instance.MaxHealth * 0.02f;
                PlayerStats.Instance.currentHealth = Mathf.Min(
                    PlayerStats.Instance.currentHealth + healAmount,
                    PlayerStats.Instance.MaxHealth);
                Debug.Log($"[혈투] 처치 회복: {healAmount:F1}");
            }
        }
    }

    void SpawnEffect(Transform player, Vector2 dir, float range)
    {
        if (effectPrefab == null) return;
        GameObject effect = Instantiate(effectPrefab);
        FollowEffect follow = effect.AddComponent<FollowEffect>();
        follow.target = player;
        follow.dir = dir;
        follow.range = range;
        effect.transform.position = player.position + (Vector3)(dir * range * 0.5f);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angleDeg);
        const float spriteNative = 0.95f;
        const float activeRatio = 0.77f;
        float visualScale = (range * 2f) / (spriteNative * activeRatio);
        effect.transform.localScale = new Vector3(visualScale, visualScale, 1f);
        Destroy(effect, 0.3f);
    }


    // ⭐ 기즈모: 실제 피격판정(OverlapCircle range)과 정확히 같은 영역
    void OnDrawGizmos()
    {
        if (instance == null) return;
        var ld = instance.GetCurrentLevelData();
        float range = ld.range;
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, range);

        // 부채꼴 각도(angle) 표시 — 단, dir은 마우스 방향이므로 캐스트 시점 기준이 아님.
        // 캐스터 우측 기준으로 부채꼴 가이드를 그려 사거리·각도 동시 시각화
        var slashData = instance.data as SlashData;
        if (slashData != null)
        {
            float angle = slashData.angle;
            Vector3 forward = transform.right;
            int step = 20;
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.8f);
            for (int i = 0; i <= step; i++)
            {
                float currentAngle = -angle / 2f + (angle / step) * i;
                Vector3 d = Quaternion.Euler(0, 0, currentAngle) * forward;
                Gizmos.DrawLine(transform.position, transform.position + d * range);
            }
        }
    }
}
