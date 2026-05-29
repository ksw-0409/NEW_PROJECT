using UnityEngine;

/// <summary>
/// ArrowRainArea가 떨어뜨리는 화살 1발.
/// BowSkill처럼 활 패시브(빙결/폭발/독/관통)를 확률 기반으로 적용해서,
/// 플레이어가 가진 활 패시브가 화살비에도 동일하게 발동한다.
/// </summary>
public class FallingArrow : MonoBehaviour
{
    private ArrowProjectile projectile;
    private Vector2 targetPos;
    private float speed = 25f;

    public void Initialize(float dmg, bool exec, bool conc, Vector2 target, Vector2 rainCenter, float rainRadius)
    {
        projectile = GetComponent<ArrowProjectile>();
        targetPos = target;

        // 1) ArrowProjectile 기본 셋업 (아래로 떨어짐)
        projectile.Setup(dmg, conc ? 2.0f : 1.0f, speed, Vector2.down);
        projectile.SetAllowedArea(rainCenter, rainRadius);

        // 2) 활 패시브 적용 — BowSkill과 동일한 확률 기반 로직
        ApplyRainPassives(exec, conc);
    }

    void Update()
    {
        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>BowSkill.CheckAndApplyPassives와 동일한 로직 — 각 활 패시브를 확률로 적용</summary>
    private void ApplyRainPassives(bool exec, bool conc)
    {
        var controller = PlayerSkillController.Instance;
        if (controller == null || projectile == null) return;

        // 관통 — 확률 기반
        ApplyPierceIfSuccess(controller.pierceCardAsset);

        // 빙결 / 폭발 / 독 — 확률 기반
        ApplyIfSuccess(controller.iceCardAsset, "Ice");
        ApplyIfSuccess(controller.explosionCardAsset, "Explosion");
        ApplyIfSuccess(controller.poisonCardAsset, "Poison");
    }

    private void ApplyPierceIfSuccess(ArrowPassiveData data)
    {
        if (data == null) return;
        var ld = data.GetCurrentLevelData();
        if (ld == null) return;

        float chance = data.CurrentProcChance;
        if (Random.value < chance)
        {
            projectile.SetPierce(ld);
        }
    }

    private void ApplyIfSuccess(ArrowPassiveData data, string type)
    {
        if (data == null) return;

        var ld = data.GetCurrentLevelData();
        if (ld == null) return;

        float chance = data.CurrentProcChance;
        if (Random.value < chance)
        {
            projectile.AddPassive(ld, type, data.arrowColor, data.arrowSprite);

            // 폭발화살 발동 시 폭발 이펙트 프리팹 주입
            if (type == "Explosion")
            {
                var psc = PlayerSkillController.Instance;
                if (psc != null && psc.bowExplosionEffectPrefab != null)
                    projectile.fireFieldPrefab = psc.bowExplosionEffectPrefab;
            }
        }
    }
}
