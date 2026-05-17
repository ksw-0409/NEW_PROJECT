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

    // ✨ 약점 사격: 치명타 적중 시 다음 화살 치명타 데미지 강화 플래그
    private bool nextArrowCritBoost = false;

    public void NotifyArrowHit(bool wasCritical)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Bow_weakpoint") && wasCritical)
        {
            nextArrowCritBoost = true;
        }
    }

    void ShootArrow(Vector2 pos, Vector2 dir)
    {
        var ld = instance.GetCurrentLevelData();
        
        // ✨ [유틸] 속사: 1회 발사 시 화살 2발 연속
        bool hasRapid = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Bow_rapid");
        int shotsThisCall = hasRapid ? 2 : 1;
        
        for (int shot = 0; shot < shotsThisCall; shot++)
        {
            GameObject obj = Instantiate(arrowPrefab, pos, Quaternion.identity);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            obj.transform.rotation = Quaternion.Euler(0, 0, angle);

            ArrowProjectile arrow = obj.GetComponent<ArrowProjectile>();
            if (arrow != null)
            {
                arrow.Setup(ld.damage, ld.multiplier, ld.projectileSpeed, dir);

                // ✨ [공격] 약점 사격: 이전 치명타 적중 이후의 화살은 치명타 데미지 +50%
                if (nextArrowCritBoost)
                {
                    arrow.weakpointCritBoost = 0.5f;
                    nextArrowCritBoost = false;
                }

                // ✨ [변칙] 화살 도탄: 치명타 시 주변 적으로 튱김
                arrow.ricochetEnabled = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Bow_ricochet");

                // 일반 화살과 도탄 화살이 본인을 다시 맞추지 않도록 프로젝타일에서 관리
                arrow.bowSkillRef = this;

                CheckAndApplyPassives(arrow);
            }
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