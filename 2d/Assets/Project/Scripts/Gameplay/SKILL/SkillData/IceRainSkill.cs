using UnityEngine;

public class IceRainSkill : SkillBase
{
    public GameObject iceRainAreaPrefab;

    public EnemyManager enemyManager;

    protected override void Execute(Transform player)
    {
        var ld = instance.GetCurrentLevelData();
        if (ld == null) return;

        var bonus = PlayerStats.Instance.GetSkillBonus(instance.data);

        Vector3 targetPos = GetNearestEnemyPosition(player);

        if (targetPos == Vector3.zero)
        {
            Debug.Log("[IceRain] 타겟팅할 적이 없어 스킬을 시전하지 않습니다.");
            return;
        }

        float distToEnemy = Vector2.Distance(player.position, targetPos);
        if (distToEnemy > ld.range)
        {
            Vector2 dir = ((Vector2)targetPos - (Vector2)player.position).normalized;
            targetPos = (Vector2)player.position + dir * ld.range;
        }

        float finalDamage = ld.damage * bonus.dmg;
        float finalRadius = ld.explosionRadius * bonus.rng;
        float finalSlowPct = Mathf.Clamp01(ld.slowPercent * bonus.slowMul);
        float finalDuration = Mathf.Max(0.05f, ld.duration * bonus.durMul);
        float finalSlowDur = ld.slowDuration > 0f ? Mathf.Max(0f, ld.slowDuration * bonus.durMul) : 0f;

        if (iceRainAreaPrefab == null)
        {
            Debug.LogWarning("[IceRain] iceRainAreaPrefab이 비어 있어 스킬을 생성할 수 없습니다.");
            return;
        }

        GameObject areaObj = Instantiate(iceRainAreaPrefab, new Vector3(targetPos.x, targetPos.y, 0f), Quaternion.identity);
        IceRainArea area = areaObj.GetComponent<IceRainArea>();
        if (area != null)
        {
            float tick = ld.tickInterval > 0f ? ld.tickInterval : 1f;
            area.Setup(finalDamage, ld.multiplier, finalDuration, finalRadius, finalSlowPct, finalSlowDur, tick);
        }
        else
            Debug.LogWarning("[IceRain] 프리팹에 IceRainArea 컴포넌트가 없습니다.");
    }

    private Vector3 GetNearestEnemyPosition(Transform player)
    {
        if (enemyManager == null || enemyManager.activeEnemies == null || enemyManager.activeEnemies.Count == 0)
            return Vector3.zero;

        EnemyAI nearestEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (var enemy in enemyManager.activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float distance = Vector2.Distance(player.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy != null ? nearestEnemy.transform.position : Vector3.zero;
    }
}
