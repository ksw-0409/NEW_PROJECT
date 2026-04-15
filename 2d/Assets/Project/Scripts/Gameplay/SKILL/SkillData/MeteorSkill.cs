using UnityEngine;

public class MeteorSkill : SkillBase
{
    public EnemyManager enemyManager;
    public GameObject meteorVisualPrefab; // 떨어지는 운석 프리팹
    public GameObject fireFieldPrefab;    // 바닥 장판 프리팹 (아까 만든 FireField)

    protected override void Execute()
    {
        if (enemyManager == null) enemyManager = FindFirstObjectByType<EnemyManager>();
        SkillLevelData ld = instance.GetCurrentLevelData();
        Vector2 targetPos = (Vector2)transform.position + Random.insideUnitCircle * ld.range;

        // 1. 시각적 연출: 하늘에서 떨어지는 운석 소환
        if (meteorVisualPrefab != null)
        {
            Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y + 10f, 0); // 10만큼 위에서 생성
            Instantiate(meteorVisualPrefab, spawnPos, Quaternion.identity);
        }

        // 2. 즉발 폭발 데미지
        Collider2D[] hit = Physics2D.OverlapCircleAll(targetPos, ld.explosionRadius);
        foreach (var col in hit)
        {
            if (col.CompareTag("Enemy")) col.GetComponent<EnemyHealth>()?.TakeDamage(ld.damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (instance == null) return;
        Gizmos.color = Color.red;
        // 메테오 소환 가능 범위
        Gizmos.DrawWireSphere(transform.position, instance.GetCurrentLevelData().range);
    }
}