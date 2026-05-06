using UnityEngine;

public class MeteorSkill : SkillBase
{
    // 스킬 컨트롤러나 외부에서 이 변수를 찾으려 한다면 아래와 같이 선언되어 있어야 합니다.
    [HideInInspector] public EnemyManager enemyManager;

    public GameObject meteorVisualPrefab; // 떨어지는 운석 프리팹
    public GameObject fireFieldPrefab;    // 바닥 장판 프리팹

    protected override void Execute(Transform player)
    {
        // 만약 enemyManager가 필요하다면 여기서 할당 (현재 로직에선 필수 아님)
        if (enemyManager == null) enemyManager = FindFirstObjectByType<EnemyManager>();

        SkillLevelData ld = instance.GetCurrentLevelData();
        var bonus = PlayerStats.Instance.GetSkillBonus(instance.data);

        float finalDamage = ld.damage * bonus.dmg;
        float finalRange = ld.range * bonus.rng;
        float finalRadius = ld.explosionRadius * bonus.rng;
        float finalDuration = Mathf.Max(0.05f, ld.duration * bonus.durMul);

        bool meteorShower = HasMeteorSpecialty(
            new[] { "Meteor_Shower", "MeteorRain", "Special_MeteorShower" },
            new[] { "유성우", "Shower" });
        int meteorCount = meteorShower ? 5 : 1;
        float showerRadiusMultiplier = meteorShower ? 1.5f : 1f;

        for (int i = 0; i < meteorCount; i++)
        {
            // 1. 떨어질 목표 지점 계산 (플레이어 주변 range 범위 내 랜덤)
            Vector2 targetPos = (Vector2)transform.position + Random.insideUnitCircle * (finalRange * showerRadiusMultiplier);

            // 2. 운석 소환
            if (meteorVisualPrefab != null)
            {
                // 목표 지점의 10유닛 위(하늘)에서 생성
                Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y + 10f, 0);
                GameObject meteorGo = Instantiate(meteorVisualPrefab, spawnPos, Quaternion.identity);

                // 데이터 전달
                MeteorVisual visual = meteorGo.GetComponent<MeteorVisual>();
                if (visual != null)
                {
                    float meteorDamage = meteorShower ? finalDamage * 0.55f : finalDamage;
                    float meteorRadius = meteorShower ? finalRadius * 0.55f : finalRadius;
                    visual.Setup(meteorDamage, ld.multiplier, finalDuration, meteorRadius, targetPos);
                    visual.fireFieldPrefab = fireFieldPrefab;
                    visual.ConfigureSpecialties(
                        enablePlanetCrash: HasMeteorSpecialty(
                            new[] { "Meteor_PlanetCrash", "PlanetCollision", "Special_PlanetCrash" },
                            new[] { "행성", "충돌", "Planet" }),
                        enableLavaField: HasMeteorSpecialty(
                            new[] { "Meteor_LavaField", "LavaZone", "Special_LavaField" },
                            new[] { "용암", "Lava" })
                    );
                }
            }
        }
    }

    // 기즈모로 소환 범위 확인
    private void OnDrawGizmos()
    {
        if (instance == null) return;
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, instance.GetCurrentLevelData().range);
    }

    private bool HasMeteorSpecialty(string[] exactTags, string[] containsKeywords)
    {
        if (PlayerStats.Instance == null) return false;
        if (PlayerStats.Instance.HasAnySpecialty(exactTags)) return true;
        return PlayerStats.Instance.HasAnySpecialtyContains(containsKeywords);
    }
}