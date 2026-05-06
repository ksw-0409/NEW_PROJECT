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

    void Attack(Vector2 dir)
    {
        float range = instance.GetCurrentLevelData().range; // ⭐ 통일

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (hit.transform.position - transform.position).normalized;

            float dot = Vector2.Dot(dir, toEnemy);
            float threshold = Mathf.Cos(slamData.angle * 0.5f * Mathf.Deg2Rad);

            if (dot < threshold) continue;

            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(GetDamage());

            // ⭐ 슬로우 적용
            var slow = hit.GetComponent<EnemySlow>();
            if (slow != null)
                slow.ApplySlow(slamData.slowPercent, slamData.slowDuration);
        }

        SpawnEffect(dir, range);
    }

    void SpawnEffect(Vector2 dir, float range)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab);

        // ⭐ 위치 (중간쯤)
        fx.transform.position = transform.position + (Vector3)(dir * range * 0.5f);

        // ⭐ 방향
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ 핵심: 이펙트 크기 = range 기반
        float scal = range * 1.5f; // 적당히 키워봄
        fx.transform.localScale = new Vector3(scal, scal, 1);

        Destroy(fx, 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // ⭐ 방향 표시 (부채꼴)
        Gizmos.color = Color.yellow;

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