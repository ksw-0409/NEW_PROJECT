using UnityEngine;
using System.Collections;
using System.Linq;

public class IceRainSkill : SkillBase
{
    public EnemyManager enemyManager;
    public GameObject iceRainEffectPrefab; // 얼음 비 파티클 프리팹

    protected override void Execute()
    {
        if (enemyManager == null) enemyManager = FindFirstObjectByType<EnemyManager>();

        SkillLevelData ld = instance.GetCurrentLevelData();

        // 1. 타겟 선정 (가장 가까운 적의 위치를 따거나, 없으면 플레이어 주변 랜덤)
        Transform targetEnemy = FindNearestEnemy(transform.position, ld.range);
        Vector2 targetPos = targetEnemy != null ? (Vector2)targetEnemy.position : (Vector2)transform.position;

        // 2. 고정된 위치에서 코루틴 시작
        StartCoroutine(IceRainRoutine(targetPos, ld));
    }

    IEnumerator IceRainRoutine(Vector2 centerPos, SkillLevelData ld)
    {
        // 3. 시각적 연출: 지정된 위치(centerPos)에 생성하고 부모를 설정하지 않음 (World 공간 고정)
        if (iceRainEffectPrefab != null)
        {
            GameObject effect = Instantiate(iceRainEffectPrefab, centerPos, Quaternion.identity);
            Destroy(effect, ld.duration);
        }

        float timer = 0;
        while (timer < ld.duration)
        {
            // 4. 고정된 위치(centerPos) 기준으로 범위 내 적 탐색
            var enemiesSnapshot = enemyManager.activeEnemies.ToArray();
            foreach (var enemy in enemiesSnapshot)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

                // 플레이어(transform.position)가 아닌 생성된 지점(centerPos) 기준 거리 체크
                if (Vector2.Distance(centerPos, enemy.transform.position) <= ld.range)
                {
                    // 데미지 및 슬로우 적용
                    enemy.GetComponent<EnemyHealth>()?.TakeDamage(ld.damage * ld.tickInterval);

                    // 슬로우 기능이 있다면 여기서 호출 (예: enemy.ApplySlow(ld.slowPercent, 1.1f))
                }
            }

            timer += ld.tickInterval;
            yield return new WaitForSeconds(ld.tickInterval);
        }
    }

    private Transform FindNearestEnemy(Vector2 pos, float range)
    {
        Transform closest = null;
        float minDst = range;
        foreach (var enemy in enemyManager.activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
            float dst = Vector2.Distance(pos, enemy.transform.position);
            if (dst < minDst) { minDst = dst; closest = enemy.transform; }
        }
        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        if (instance == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, instance.GetCurrentLevelData().range);
    }
}