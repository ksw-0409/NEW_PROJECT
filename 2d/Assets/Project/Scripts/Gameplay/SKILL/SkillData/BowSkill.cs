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

    [Header("Explosion FX Source (for ExplosionArrow)")]
    [Tooltip("폭발화살이 발동될 때 사용할 폭발 이펙트 프리팹. FireballData.effectPrefab과 동일한 것을 사용하면 파이어볼과 같은 폭발 이펙트가 나옵니다.")]
    public GameObject explosionEffectPrefab;

    protected override void Execute(Transform player)
    {
        var ld = instance.GetCurrentLevelData();
        if (ld == null) return;

        Vector2 fireDir = GetFireDirection(player);

        // ⭐ 전설 어빌리티 3 (페일노트): 화살 2발 추가 (부채꼴)
        bool palenoteOn = PlayerStats.Instance != null && PlayerStats.Instance.HasAbility(3);
        float spreadDeg = 25f;

        for (int i = 0; i < ld.count; i++)
        {
            // 정중앙(메인) 1발
            ShootArrow(player.position, fireDir);

            // 페일노트가 있으면 좌/우로 +2발 부채꼴
            if (palenoteOn)
            {
                float baseAngle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
                for (int side = -1; side <= 1; side += 2)
                {
                    float ang = (baseAngle + side * spreadDeg) * Mathf.Deg2Rad;
                    Vector2 spreadDir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    ShootArrow(player.position, spreadDir);
                }
            }
        }
    }

    private Vector2 GetFireDirection(Transform player)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();

        if (cam != null)
        {
            try
            {
                if (Mouse.current != null)
                {
                    Vector2 screenPos = Mouse.current.position.ReadValue();
                    Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z - player.position.z)));
                    Vector2 raw = (Vector2)worldPos - (Vector2)player.position;
                    if (raw.sqrMagnitude > 0.01f) return raw.normalized;
                }
            }
            catch { /* fall through */ }

            try
            {
                Vector3 legacyPos = Input.mousePosition;
                legacyPos.z = Mathf.Abs(cam.transform.position.z - player.position.z);
                Vector3 worldPos = cam.ScreenToWorldPoint(legacyPos);
                Vector2 raw = (Vector2)worldPos - (Vector2)player.position;
                if (raw.sqrMagnitude > 0.01f) return raw.normalized;
            }
            catch { /* fall through */ }
        }

        return player.localScale.x >= 0 ? Vector2.right : Vector2.left;
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

                if (nextArrowCritBoost)
                {
                    arrow.weakpointCritBoost = 0.5f;
                    nextArrowCritBoost = false;
                }

                arrow.ricochetEnabled = PlayerStats.Instance != null && PlayerStats.Instance.HasSpecialty("Bow_ricochet");
                arrow.bowSkillRef = this;

                CheckAndApplyPassives(arrow);
            }
        }
    }

    void CheckAndApplyPassives(ArrowProjectile arrow)
    {
        // ✨ [관통] 카드도 다른 패시브와 동일한 확률 체크 적용
        ApplyPierceIfSuccess(arrow);

        ApplyIfSuccess(arrow, iceCard, "Ice");
        ApplyIfSuccess(arrow, explosionCard, "Explosion");
        ApplyIfSuccess(arrow, poisonCard, "Poison");
    }

    void ApplyPierceIfSuccess(ArrowProjectile arrow)
    {
        if (pierceCard == null) return;
        var ld = pierceCard.GetCurrentLevelData();
        if (ld == null) return;

        // ⭐ Pierce도 다른 패시브처럼 multiplier(=확률) 기반으로 발동
        // 인스펙터의 levels[].multiplier에 0.1(10%), 0.2(20%)식으로 입력
        float chance = pierceCard.CurrentProcChance;
        if (Random.value < chance)
        {
            arrow.SetPierce(ld);
        }
    }

    void ApplyIfSuccess(ArrowProjectile arrow, ArrowPassiveData data, string type)
    {
        if (data == null) return;

        var ld = data.GetCurrentLevelData();
        if (ld == null) return;

        float chance = data.CurrentProcChance;

        if (Random.value < chance)
        {
            arrow.AddPassive(ld, type, data.arrowColor, data.arrowSprite);

            // ⭐ 폭발화살 발동 시 화염구의 폭발 이펙트를 ArrowProjectile에 주입
            if (type == "Explosion" && explosionEffectPrefab != null)
            {
                arrow.fireFieldPrefab = explosionEffectPrefab;
            }
        }
    }
}