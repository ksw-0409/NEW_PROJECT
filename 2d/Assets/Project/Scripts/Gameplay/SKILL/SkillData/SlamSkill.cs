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

        // ⭐ 피격범위 가시화: 내려찍기는 부채꼴 영역
        float angleDeg = slamData != null ? slamData.angle : 90f;
        SkillRangeIndicator.SpawnSector(
            transform.position,
            dir,
            range,
            angleDeg,
            new Color(1f, 0.7f, 0.1f, 0.95f),
            0.6f
        );

        SpawnEffect(dir, range);
    }

    void SpawnEffect(Vector2 dir, float range)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab);

        // 위치: 부채꼴 중심(캐스터와 사거리의 절반 지점)
        fx.transform.position = transform.position + (Vector3)(dir * range * 0.5f);

        // 방향
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ explosion-f sprite native(0.48), 가로 활성 83% — 가로는 사거리, 세로는 사거리 절반
        const float spriteNativeX = 0.48f;
        const float spriteNativeY = 0.48f;
        const float activeRatioX = 0.83f;
        const float activeRatioY = 0.83f; // explosion-f는 프레임마다 활성 영역 다름; 평균값
        // 가로는 사거리의 1배 (접에서 앞까지), 세로는 사거리의 60% 정도
        float scaleX = range / (spriteNativeX * activeRatioX);
        float scaleY = (range * 0.6f) / (spriteNativeY * activeRatioY);
        fx.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        Destroy(fx, 0.5f);
    }

    // ⭐ 기즈모: 실제 피격판정(OverlapCircle range + angle 부채꼴)과 일치 — 선택 시와 항상 둘 다 표시
    void OnDrawGizmosSelected()
    {
        DrawSlamGizmo();
    }

    void OnDrawGizmos()
    {
        DrawSlamGizmo();
    }

    private void DrawSlamGizmo()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.7f);

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