using UnityEngine;
using System.Collections;

public class ArrowRainSkill : SkillBase
{
    [Header("Prefabs")]
    // ⭐ 이름을 호출부와 똑같이 rainAreaPrefab으로 변경했습니다.
    public GameObject rainAreaPrefab;

    [Header("References")]
    public EnemyManager enemyManager;

    protected override void Execute(Transform player)
    {
        var ld = instance.GetCurrentLevelData();
        if (ld == null) return;

        // PlayerStats에서 보너스 데이터를 가져옵니다.
        var bonus = PlayerStats.Instance != null ? PlayerStats.Instance.GetSkillBonus(instance.data) : (dmg:1f, rng:1f, cool:1f, cnt:0, slowMul:1f, durMul:1f);

        // 1. 가장 가까운 적의 위치 찾기
        Vector3 targetPos = GetNearestEnemyPosition(player);

        // 2. 적이 한 마리도 없을 경우 시전 취소
        if (targetPos == Vector3.zero)
        {
            Debug.Log("[ArrowRain] 타겟팅할 적이 없어 스킬을 시전하지 않습니다.");
            return;
        }

        // 3. 사거리(Range) 제한 로직
        float distToEnemy = Vector2.Distance(player.position, targetPos);
        float skillRange = ld.range * bonus.rng;
            if (distToEnemy > skillRange)
        {
            Vector2 dir = ((Vector2)targetPos - (Vector2)player.position).normalized;
            targetPos = (Vector2)player.position + (dir * skillRange);
        }

        // 4. 최종 계산된 수치
        float finalDamage = ld.damage * bonus.dmg;
        float finalRadius = ld.explosionRadius * bonus.rng;

        // 5. 장판 생성 후 장판 내부에만 화살비를 떨어뜨립니다.
        if (rainAreaPrefab == null)
        {
            Debug.LogWarning("[ArrowRain] rainAreaPrefab이 비어 있어 스킬을 생성할 수 없습니다.");
            return;
        }

        GameObject areaObj = Instantiate(rainAreaPrefab, new Vector3(targetPos.x, targetPos.y, 0f), Quaternion.identity);
        ArrowRainArea area = areaObj.GetComponent<ArrowRainArea>();
        if (area != null)
            area.Setup(finalDamage, 1f, ld.duration, false, false, finalRadius);
        else
            Debug.LogWarning("[ArrowRain] rainAreaPrefab에 ArrowRainArea 컴포넌트가 없습니다.");
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