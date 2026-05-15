using UnityEngine;

/// <summary>
/// 검기 방출 스킬.
/// 준비 동작 모션 후, 전방으로 검기 투사체를 발사하여
/// 경로상의 적에게 데미지를 줍니다. (이즈리얼 R 스타일)
/// </summary>
public class SwordWaveSkill : SkillBase
{
    private SkillLevelData levelData;

    [Tooltip("폴백용 검기 투사체 프리팹. SwordWaveData의 effectPrefab이 비어있을 때만 사용됩니다.")]
    public GameObject effectPrefab;

    private Vector2 lastFireDir;

    protected override void Execute(Transform player)
    {
        levelData = instance.GetCurrentLevelData();

        float lookDirection = player.localScale.x > 0 ? 1f : -1f;
        Vector2 fireDir = Vector2.right * lookDirection;
        lastFireDir = fireDir;

        for (int i = 0; i < GetCount(); i++)
        {
            Fire(player.position, fireDir);
        }
    }

    void Fire(Vector2 startPos, Vector2 dir)
    {
        // ⭐ SwordWaveData(SO)의 effectPrefab을 우선 사용. 비어있을 때만 컴포넌트 필드로 폴백.
        var swordWaveData = instance.data as SwordWaveData;
        GameObject prefabToUse = swordWaveData != null && swordWaveData.effectPrefab != null
            ? swordWaveData.effectPrefab
            : effectPrefab;

        if (prefabToUse == null)
        {
            Debug.LogWarning("[SwordWaveSkill] effectPrefab이 비어있어 검기를 발사할 수 없습니다.");
            return;
        }

        float speed = swordWaveData != null ? swordWaveData.projectileSpeed : 18f;
        float thickness = swordWaveData != null ? swordWaveData.projectileThickness : 1.5f;
        float range = levelData.range;
        float damage = levelData.damage * levelData.multiplier;

        // 1. 투사체 생성
        GameObject projectile = Instantiate(prefabToUse, (Vector3)startPos, Quaternion.identity);

        // 2. 투사체 초기화 (방향/데미지/사거리/속도/두께)
        var projectileScript = projectile.GetComponent<SwordWaveProjectile>();
        if (projectileScript == null)
        {
            projectileScript = projectile.AddComponent<SwordWaveProjectile>();
        }
        projectileScript.Init(dir, damage, range, speed, thickness);

        // 3. 피격범위 가시화 (선택)
        ShowRangeIndicator(startPos, dir, range, thickness);
    }

    void ShowRangeIndicator(Vector2 startPos, Vector2 dir, float range, float thickness)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var indicator = new GameObject("SwordWaveRangeBox");
        Vector3 center = (Vector3)startPos + (Vector3)(dir * range * 0.5f);
        indicator.transform.position = center;
        indicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        var ind = indicator.AddComponent<SkillRangeIndicator>();
        ind.shape = SkillRangeIndicator.Shape.Box;
        ind.radius = range;
        ind.boxHeight = thickness;
        ind.edgeColor = new Color(0.85f, 0.6f, 1f, 0.95f);
        ind.fillColor = new Color(0.85f, 0.6f, 1f, 0.18f);
        ind.useWorldSpace = false;
        ind.holdDuration = 0.2f;
        ind.fadeOutDuration = 0.3f;
    }

    void OnDrawGizmos()
    {
        if (instance == null) return;

        var data = instance.GetCurrentLevelData();
        const float thickness = 1.5f;
        Gizmos.color = new Color(0.85f, 0.6f, 1f, 0.9f);

        float lookDir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 dir = Vector3.right * lookDir;
        Vector3 center = transform.position + (dir * data.range * 0.5f);

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0, 0, 90), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(data.range, thickness, 0.1f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
