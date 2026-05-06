using UnityEngine;

public class ChainLightningSkill : SkillBase
{
    public EnemyManager enemyManager;
    public GameObject lightningEffectPrefab;
    public GameObject cloudVisualPrefab;
    private float cloudTimer;
    private Transform cloudAnchor;
    private const float CloudHeight = 1.8f;

    protected override void Execute(Transform player)
    {
        if (IsCloudEnabled()) return;

        LaunchLightning(player);
    }

    void Update()
    {
        if (IsCloudEnabled())
        {
            EnsureCloudVisual();
            cloudTimer += Time.deltaTime;
            if (cloudTimer >= 1.0f)
            {
                cloudTimer = 0;
                if (cloudAnchor != null)
                    cloudAnchor.position = transform.position + Vector3.up * CloudHeight;
                LaunchLightning(cloudAnchor != null ? cloudAnchor : transform, true);
            }
        }
        else
        {
            CleanupCloudVisual();
        }
    }

    // ChainLightningSkill.cs 내부의 LaunchLightning 함수 수정
    private void LaunchLightning(Transform caster, bool singleStrike = false)
    {
        var ld = instance.GetCurrentLevelData();
        // ⭐ instance 대신 instance.data를 전달하여 에러 해결
        var bonus = PlayerStats.Instance.GetSkillBonus(instance.data);

        Transform target = GetNearestEnemy(caster, ld.range);

        if (target != null)
        {
            GameObject obj = Instantiate(lightningEffectPrefab, caster.position, Quaternion.identity);
            ChainLightning cl = obj.GetComponent<ChainLightning>();

            if (cl != null)
            {
                // 보너스가 반영된 최종 수치 전달
                float finalDamage = ld.damage * bonus.dmg;
                int finalCount = singleStrike ? 1 : Mathf.RoundToInt(ld.count + bonus.cnt);
                float finalRange = ld.range * bonus.rng;

                cl.Setup(finalDamage, finalCount, finalRange, caster);
                cl.StartChain(target);
            }
        }
    }

    private Transform GetNearestEnemy(Transform caster, float range)
    {
        if (enemyManager == null || enemyManager.activeEnemies == null) return null;

        Transform nearest = null;
        float minTargetDist = range;

        for (int i = 0; i < enemyManager.activeEnemies.Count; i++)
        {
            var enemy = enemyManager.activeEnemies[i];

            // ⭐ 수정: enemy.activeInHierarchy -> enemy.gameObject.activeInHierarchy
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float dist = Vector2.Distance(caster.position, enemy.transform.position);
            if (dist < minTargetDist)
            {
                minTargetDist = dist;
                nearest = enemy.transform; // EnemyAI의 트랜스폼 참조
            }
        }
        return nearest;
    }

    private bool IsCloudEnabled()
    {
        if (PlayerStats.Instance == null) return false;
        return PlayerStats.Instance.HasAnySpecialty("Lightning_Cloud", "Special_LightningCloud", "ChainLightning_Cloud");
    }

    private void EnsureCloudVisual()
    {
        if (cloudAnchor != null)
        {
            cloudAnchor.position = transform.position + Vector3.up * CloudHeight;
            return;
        }

        GameObject cloudObject;
        if (cloudVisualPrefab != null)
        {
            cloudObject = Instantiate(cloudVisualPrefab, transform.position + Vector3.up * CloudHeight, Quaternion.identity);
        }
        else
        {
            cloudObject = CreateFallbackCloudVisual();
            cloudObject.transform.position = transform.position + Vector3.up * CloudHeight;
        }

        cloudObject.name = "LightningCloudVisual";
        cloudObject.transform.SetParent(transform, true);
        cloudAnchor = cloudObject.transform;
    }

    private void CleanupCloudVisual()
    {
        if (cloudAnchor == null) return;
        Destroy(cloudAnchor.gameObject);
        cloudAnchor = null;
    }

    private GameObject CreateFallbackCloudVisual()
    {
        GameObject cloudObject = new GameObject("LightningCloudFallback");
        ParticleSystem ps = cloudObject.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 0.1f;
        main.startSize = 0.45f;
        main.maxParticles = 30;
        main.startColor = new Color(0.6f, 0.8f, 1f, 0.9f);

        var emission = ps.emission;
        emission.rateOverTime = 20f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 200;

        return cloudObject;
    }
}