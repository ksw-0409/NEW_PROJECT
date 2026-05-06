using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowSkill : SkillBase
{
    public GameObject arrowPrefab;

    [Header("Passive Cards Connection")]
    public ArrowPassiveData iceCard;
    public ArrowPassiveData explosionCard;
    public ArrowPassiveData poisonCard;
    public ArrowPassiveData pierceCard;

    protected override void Execute(Transform player)
    {
        var ld = instance.GetCurrentLevelData();
        if (ld == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        Vector2 fireDir = ((Vector2)mouseWorldPos - (Vector2)player.position).normalized;

        for (int i = 0; i < ld.count; i++)
        {
            ShootArrow(player.position, fireDir);
        }
    }

    void ShootArrow(Vector2 pos, Vector2 dir)
    {
        var ld = instance.GetCurrentLevelData();
        GameObject obj = Instantiate(arrowPrefab, pos, Quaternion.identity);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.Euler(0, 0, angle);

        ArrowProjectile arrow = obj.GetComponent<ArrowProjectile>();
        if (arrow != null)
        {
            arrow.Setup(ld.damage, ld.multiplier, ld.projectileSpeed, dir);
            // ⭐ 모든 패시브 체크를 이 안에서만 수행
            CheckAndApplyPassives(arrow);
        }
    }

    void CheckAndApplyPassives(ArrowProjectile arrow)
    {
        // 관통 카드가 연결되어 있고, 레벨이 1 이상인지 체크
        var pierceLd = pierceCard?.GetCurrentLevelData();
        if (pierceLd != null)
        {
            arrow.SetPierce(pierceLd);
        }

        ApplyIfSuccess(arrow, iceCard, "Ice");
        ApplyIfSuccess(arrow, explosionCard, "Explosion");
        ApplyIfSuccess(arrow, poisonCard, "Poison");
    }

    void ApplyIfSuccess(ArrowProjectile arrow, ArrowPassiveData data, string type)
    {
        if (data == null) return;

        // GetCurrentLevelData가 위에서 수정된 대로 null을 반환하면 여기서 컷 됩니다.
        var ld = data.GetCurrentLevelData();
        if (ld == null) return;

        // 인스턴스가 있을 때만 확률을 계산합니다.
        float chance = data.CurrentProcChance;

        // 로그를 통해 현재 상태 확인
        Debug.Log($"[BowSkill] {type} 체크 - 확률: {chance * 100}%");

        if (Random.value < chance)
        {
            arrow.AddPassive(ld, type, data.arrowColor, data.arrowSprite);
            Debug.Log($"<color=yellow>{type} 적용 성공!</color>");
        }
    }
}