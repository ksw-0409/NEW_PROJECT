using UnityEngine;
using UnityEngine.InputSystem;

public class SwordWaveSkill : SkillBase
{
    private SkillLevelData levelData;
    private SlamSkillData slamData;

    public GameObject effectPrefab;

    private Vector2 lastDir;
    protected override void Execute()
    {
        levelData = instance.GetCurrentLevelData();

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        lastDir = dir; // ⭐ 추가

        for (int i = 0; i < GetCount(); i++)
        {
            Fire(dir);
        }
    }

    void Fire(Vector2 dir)
    {
        float range = levelData.range;

        // ⭐ 이펙트 생성
        SpawnEffect(dir, range);

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            dir,
            range
        );

        foreach (var hit in hits)
        {
            if (!hit.collider.CompareTag("Enemy")) continue;

            var enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(GetDamage());
        }
    }

    void OnDrawGizmosSelected()
    {
        if (instance == null) return;

        var data = instance.GetCurrentLevelData();

        Gizmos.color = Color.cyan;

        Vector3 dir = lastDir;

        // ⭐ dir이 없을 때 대비
        if (dir == Vector3.zero)
            dir = transform.right;

        Gizmos.DrawLine(
            transform.position,
            transform.position + dir * data.range
        );
    }

    void SpawnEffect(Vector2 dir, float range)
    {
        if (effectPrefab == null)
        {
            Debug.LogError("SwordWave effectPrefab NULL");
            return;
        }

        GameObject fx = Instantiate(effectPrefab);

        // ⭐ 위치 (앞쪽)
        fx.transform.position = transform.position + (Vector3)(dir * range * 0.5f);

        // ⭐ 방향 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ 크기 (range 기반)
        fx.transform.localScale = new Vector3(range, 1f, 1f);

        Destroy(fx, 0.3f);
    }
}