using UnityEngine;
using UnityEngine.InputSystem;

public class SlashSkill : SkillBase
{
    public GameObject effectPrefab;

    [SerializeField] private float range;
    [SerializeField] private float angle;

    protected override void Execute()
    {
        float range = instance.GetCurrentLevelData().range; // ⭐ 여기
        float angle = instance.GetCurrentLevelData().angle; // ⭐ 여기
        Transform player = transform;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Vector2 dir = (mousePos - (Vector2)player.position).normalized;

        SpawnEffect(player, dir, range);

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, range);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (hit.transform.position - player.position).normalized;

            float dot = Vector2.Dot(dir, toEnemy);
            float threshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

            if (dot < threshold) continue;

            var enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(GetDamage());
        }
    }

    void SpawnEffect(Transform player, Vector2 dir, float range)
    {
        if (effectPrefab == null)
        {
            Debug.LogError("effectPrefab NULL");
            return;
        }

        GameObject effect = Instantiate(effectPrefab);

        // ⭐ FollowEffect 붙이기
        FollowEffect follow = effect.AddComponent<FollowEffect>();
        follow.target = player;
        follow.dir = dir;
        follow.range = range;

        // ⭐ 초기 위치
        effect.transform.position = player.position + (Vector3)(dir * range * 0.7f);

        // ⭐ 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ 크기
        effect.transform.localScale = Vector3.one * 2.5f;

        Destroy(effect, 0.2f);
    }
}