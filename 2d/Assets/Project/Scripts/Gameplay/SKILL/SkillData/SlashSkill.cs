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

            // [공격] 잔상: 50% 확률로 2번째 타격
            if (hasPhantom && Random.value < 0.5f)
                enemy.TakeDamage(damage);

            // [유틸] 혈투: 출혈 적용
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
        effect.transform.position = player.position + (Vector3)(dir * range * 0.7f);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);
        effect.transform.localScale = Vector3.one * 2.5f;
        Destroy(effect, 0.2f);
    }
}
