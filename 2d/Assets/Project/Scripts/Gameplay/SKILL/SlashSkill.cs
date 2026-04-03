using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SlashSkill : SkillBase
{
    private SlashSkillData data;

    private int skillLevel = 1;
    private int treeLevel = 0;

    private float lastHitTime;
    private float hitDelay = 0.2f; // 히트 딜레이

    private GameObject slashEffectPrefab;

    public void Init(SlashSkillData data, int treeLevel, GameObject effectPrefab)
    {
        this.data = data;
        this.treeLevel = treeLevel;
        this.slashEffectPrefab = effectPrefab;

        base.Init(data.cooldown);
    }

    protected override void Execute(Transform player)
    {
        // 히트 딜레이
        if (Time.time - lastHitTime < hitDelay) return;
        lastHitTime = Time.time;

        // 마우스 방향
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Vector2 dir = (mousePos - (Vector2)player.position).normalized;

        // 데미지 계산
        float multiplier = data.levelMultiplier[skillLevel - 1];

        float playerAtk = player.GetComponent<PlayerStats>().data.attackPower;

        float finalDamage = data.baseDamage * multiplier + (playerAtk * 0.5f);

        // 범위 계산
        float range = data.baseRange;

        if (treeLevel >= 6)
            range *= 1.5f;

        float angleRange = 60f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, range);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = (hit.transform.position - player.position).normalized;

            float angle = Vector2.Angle(dir, toEnemy);

            if (angle <= angleRange)
            {
                hit.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            }
        }

        SpawnEffect(player, dir, range);
    }

    private void SpawnEffect(Transform player, Vector2 dir, float range)
    {
        GameObject effect = Object.Instantiate(slashEffectPrefab);
        //var effect = EffectManager.Instance.GetEffect();
        effect.transform.position = player.position + (Vector3)(dir * range * 0.5f);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);

        Object.Destroy(effect, 0.3f);
    }
}