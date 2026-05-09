using UnityEngine;

public class MeteorSkill : SkillBase
{
    // 스킬 컨트롤러나 외부에서 이 변수를 찾으려 한다면 아래와 같이 선언되어 있어야 합니다.
    [HideInInspector] public EnemyManager enemyManager;

    public GameObject meteorVisualPrefab; // 떨어지는 운석 프리팹
    public GameObject fireFieldPrefab;    // 바닥 장판 프리팹
    public GameObject warningCirclePrefab; // ⭐ 떨어지기 전 바닥에 표시할 마법진

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

        // ⭐ 동시 활성 메테오 수 제한 — 이전 메테오가 너무 많이 쌓이면 새 발동 스킵 (시각 누적 방지)
        var existing = GameObject.FindObjectsByType<MeteorVisual>(FindObjectsSortMode.None);
        int maxConcurrent = meteorShower ? 8 : 3;
        if (existing.Length >= maxConcurrent)
        {
            Debug.Log($"[MeteorSkill] Skip spawn — already {existing.Length} meteors active (max {maxConcurrent})");
            return;
        }

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
                    // (Setup 호출은 prefab 할당 후로 이동됨)
                    visual.fireFieldPrefab = fireFieldPrefab;
                    visual.warningCirclePrefab = warningCirclePrefab;
                    Debug.Log($"[MeteorSkill] Spawning meteor at {targetPos}, warningCirclePrefab={(warningCirclePrefab != null ? warningCirclePrefab.name : "NULL")}");
                    visual.Setup(meteorDamage, ld.multiplier, finalDuration, meteorRadius, targetPos);
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