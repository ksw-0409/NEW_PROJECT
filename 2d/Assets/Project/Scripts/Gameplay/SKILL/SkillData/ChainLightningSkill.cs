using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningSkill : SkillBase
{
    public EnemyManager enemyManager;
    public GameObject lightningEffectPrefab; // 번개 이펙트 프리팹

    protected override void Execute()
    {
        if (enemyManager == null) enemyManager = FindFirstObjectByType<EnemyManager>();

        SkillLevelData current = instance.GetCurrentLevelData();
        Transform firstTarget = FindNearestEnemy(transform.position, current.range);

        if (firstTarget != null)
        {
            StartCoroutine(ChainRoutine(firstTarget, current));
        }
    }

    IEnumerator ChainRoutine(Transform firstTarget, SkillLevelData ld)
    {
        Transform currentTarget = firstTarget;
        HashSet<Transform> hitTargets = new HashSet<Transform>();

        for (int i = 0; i < ld.count; i++)
        {
            if (currentTarget == null) break;

            // 시각적 연출: 루프 내부에서 생성할 때 바로 Destroy를 걸어줘야 합니다.
            if (lightningEffectPrefab != null)
            {
                GameObject effect = Instantiate(lightningEffectPrefab, currentTarget.position, Quaternion.identity);
                // 생성하자마자 0.3~0.5초 뒤 삭제 예약
                Destroy(effect, 0.5f);
            }

            float finalDamage = ld.damage * Mathf.Pow(0.75f, i);
            ApplyDamage(currentTarget, finalDamage);
            hitTargets.Add(currentTarget);

            Transform nextTarget = FindNearestEnemy(currentTarget.position, ld.range, hitTargets);
            if (nextTarget == null) break;

            yield return new WaitForSeconds(0.1f);
            currentTarget = nextTarget;
        }

        // ❌ 기존 루프 바깥에 있던 이펙트 생성 코드는 이제 필요 없으므로 삭제해도 됩니다.
    }

    private void ApplyDamage(Transform target, float dmg)
    {
        target.GetComponent<EnemyHealth>()?.TakeDamage(dmg);
    }

    private Transform FindNearestEnemy(Vector2 pos, float range, HashSet<Transform> ignore = null)
    {
        Transform closest = null;
        float minDst = range;
        foreach (var enemy in enemyManager.activeEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || (ignore != null && ignore.Contains(enemy.transform))) continue;
            float dst = Vector2.Distance(pos, enemy.transform.position);
            if (dst < minDst) { minDst = dst; closest = enemy.transform; }
        }
        return closest;
    }

    // 에디터에서 공격 사거리를 표시
    private void OnDrawGizmosSelected()
    {
        if (instance == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, instance.GetCurrentLevelData().range);
    }


}