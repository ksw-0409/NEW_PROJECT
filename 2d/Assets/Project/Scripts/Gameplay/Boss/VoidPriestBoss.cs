using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 10층 보스 — 공허의 대사제 (Void Priest)
///
/// [일반공격] (3초마다)
///   플레이어 방향으로 보라 구체 3개를 일자로 발사 (속도 = 플레이어 기본 속도 * 1.25).
///   일반공격 3회 후 → [강화공격]:
///     플레이어 위치에 메테오 낙하 → 지속 피해 장판 생성 + 4방향으로 보라 구체 1회 발사.
///
/// [특수공격1] (HP 50% / 5% 도달 시 1회씩)
///   맵에 봉인석 기둥 랜덤 생성. 보호막을 두르고 플레이어에게 유도 레이저 공격.
///   레이저는 5초(3초 추적 + 2초 정지) 후 발사. 플레이어가 유도해 기둥에 맞혀야 함.
///   총 5회 공격. 실패(한 번이라도 기둥 못 맞힘) 시 HP 전체 회복.
///
/// [특수공격2] (HP 70% / 40% / 10% 도달 시 1회씩)
///   보스 주변에 몬스터 소환.
///     70% → 박쥐 10마리
///     40% → 폭발 거미 5마리 + 새끼 거미 10마리
///     10% → 정예 암석 골렘 2마리
/// </summary>
public class VoidPriestBoss : EnemyAI
{
    [Header("일반공격 — 보라 구체")]
    public GameObject voidOrbPrefab;
    public float normalAttackInterval = 3f;     // 평타 3초에 한번
    public int normalAttackCountBeforeEnhanced = 3; // 3회 후 강화공격
    public float orbSpeedMultiplier = 1.25f;    // 플레이어 기본 속도 * 1.25
    public float playerBaseSpeed = 5f;          // 플레이어 기본 이동속도 (orb 속도 산정 기준)
    public float orbLineSpacing = 0.5f;         // 일자 3개 간격

    [Header("강화공격 — 메테오 장판 + 4방향 구체")]
    public float meteorRadius = 2.2f;
    public float meteorFieldDuration = 4f;
    public float meteorTickDamage = 6f;
    public float meteorWarningTime = 1.0f;

    [Header("특수공격1 — 봉인석 기둥 + 유도 레이저")]
    public GameObject sealPillarPrefab;          // 없으면 코드로 생성
    public int pillarCount = 4;                  // 맵에 생성할 기둥 수
    public float pillarSpawnRange = 6f;          // 보스 주변 분포 반경
    public int laserAttackCount = 5;             // 총 레이저 공격 횟수
    public float laserTrackDuration = 3f;
    public float laserLockDuration = 2f;
    public float laserDamage = 25f;
    public float laserBeamLength = 30f;
    public float laserBeamWidth = 0.5f;
    [Tooltip("특수공격1 발동 HP 임계값들 (이하로 떨어지면 1회 발동)")]
    public float[] special1Thresholds = new float[] { 0.50f, 0.05f };

    [Header("특수공격2 — 몬스터 소환")]
    public GameObject batPrefab;
    public GameObject explosiveSpiderPrefab;     // 폭발 거미 (Spider2)
    public GameObject babySpiderPrefab;          // 새끼 거미
    public GameObject rockGolemElitePrefab;      // 정예 암석 골렘
    public float summonRadius = 3f;

    [Header("소환 이펙트")]
    [Tooltip("몬스터가 소환되는 자리에 잠깐 표시할 summon 스프라이트 시퀀스 (summon_0 ~ summon_4 순서로 할당)")]
    public Sprite[] summonEffectSprites;
    [Header("⭐ 떨어지는 메테오 sprite (Fire_Sprit1)")]
    public Sprite fireMeteorSprite;
    [Tooltip("이펙트 표시 후 실제 몬스터가 등장할 때까지의 시간(초)")]
    public float summonEffectDuration = 1.0f;
    [Tooltip("소환 시작 시 플레이어를 보스 주변에서 밀어내는 거리")]
    public float pushPlayerDistance = 6f;

    [Header("추격 / 이동")]
    [Tooltip("플레이어와 이 거리 이상이면 보스가 다가옴. 이하면 정지하고 공격")]
    public float chaseDistance = 5f;
    [Tooltip("추격 시 이동 속도 (유닛/초)")]
    public float chaseSpeed = 2.5f;

        [Header("기타")]
    public float introDelay = 0.5f;

    private Animator anim;
    private SpriteRenderer sr;
    private Transform playerTr;
    private EnemyHealth hp;

    private bool patternActive = false;

    // 페이즈 트리거 플래그 (한 번씩만 발동)
    private List<float> special1Pending = new List<float>();
    private bool[] special2Fired = new bool[3]; // 70%, 40%, 10%
    private readonly float[] special2Thresholds = new float[] { 0.70f, 0.40f, 0.10f };

    // 보호막 상태 (특수1 진행 중 무적)
        private bool shielded = false;
    private bool specialResting = false; // 특수공격 직후 5초 휴식 중인지
    private float shieldHp = -1f; // 보호막 진입 시 HP 저장 (무적 유지용);
    private GameObject shieldVisual;

    private System.Random rng = new System.Random();

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        hp = GetComponent<EnemyHealth>();
    }

    public override void Init()
    {
        isDie = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 10;
        int startFloor = data != null ? data.startfloor : 10;
        float scaledHp = data != null ? data.hp * (1f + Mathf.Max(0, floor - startFloor) * 0.3f) : 2000f;
        hp = GetComponent<EnemyHealth>();
        if (fireMeteorSprite != null) VoidMeteorField.SetFireSpriteCache(fireMeteorSprite);
        if (hp != null) hp.init(scaledHp);

        SkillDamage = data != null ? data.skillDamage * (1f + Mathf.Max(0, floor - startFloor) * 0.15f) : 25f;

        patternActive = false;
        shielded = false;
        special2Fired = new bool[3];

        // 특수1 임계값 등록
        special1Pending = new List<float>();
        if (special1Thresholds != null)
            foreach (var th in special1Thresholds) special1Pending.Add(th);

        if (EnemyManager.Instance != null) playerTr = EnemyManager.Instance.player;
        if (playerTr == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) playerTr = pgo.transform;
        }

        Debug.Log($"[VoidPriestBoss] Init - HP:{scaledHp} SkillDmg:{SkillDamage} player:{(playerTr != null ? playerTr.name : "NULL")}");

        StartCoroutine(MainPatternLoop());
    }

    // ---- 피격 플래시 ----
    private float lastHp = -1f;
    private float flashEndTime = 0f;
    private const float FLASH_DURATION = 0.12f;

    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);
        if (isDie) return;

        if (sr != null && playerTr != null)
            sr.flipX = playerTr.position.x > transform.position.x; // ⭐ 조건 반전 (보스가 앞을 보도록)
        if (sr != null && !sr.enabled) sr.enabled = true;

        if (hp != null && sr != null)
        {
            if (flashEndTime <= 0)
            {
                if (sr.color != Color.white) sr.color = Color.white;
            }
            else if (Time.time >= flashEndTime)
            {
                sr.color = Color.white;
                flashEndTime = 0f;
            }
            if (lastHp > 0 && hp.currentHp < lastHp - 0.001f)
            {
                if (flashEndTime <= 0) sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                flashEndTime = Time.time + FLASH_DURATION;
            }
                        // 보호막 중 무적: HP가 줄어들면 저장된 값으로 즉시 복원
            if (shielded && shieldHp > 0f && hp.currentHp < shieldHp)
                hp.currentHp = shieldHp;

            lastHp = hp.currentHp;

            // 페이즈 트리거 체크 (보호막 중 아닐 때만)
            if (!shielded) CheckPhaseTriggers();
        }
    }

    public override void MoveTaget(Vector2 targetPos)
    {
        if (rb == null || isDie) return;

        // 특수공격/보호막 중이거나 넉백/스턴 상태면 이동 정지
        if (shielded || specialResting || isKnockedBack || isStun)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어와 거리 측정 → 멀면 추격, 가까우면 정지하고 공격
        if (playerTr == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 toPlayer = (Vector2)playerTr.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;

        if (dist > chaseDistance)
        {
            // 멀면 추격
            Vector2 dir = toPlayer.normalized;
            rb.linearVelocity = dir * chaseSpeed;
            if (anim != null && !IsAttackAnimPlaying()) anim.Play("Walk", 0);
        }
        else
        {
            // 가까우면 정지하고 공격 패턴 진행
            rb.linearVelocity = Vector2.zero;
            if (anim != null && !IsAttackAnimPlaying()) anim.Play("Idle", 0);
        }
    }

    public override void Die()
    {
        if (isDie) return;
        StopAllCoroutines();
        RemoveShield();
        base.Die();
    }

    private float HpRatio => hp != null ? hp.currentHp / Mathf.Max(1f, hp.MaxHp) : 1f;

    // ===================== 메인 패턴 루프 =====================
    private IEnumerator MainPatternLoop()
    {
        patternActive = true;
        yield return new WaitForSeconds(introDelay);

        while (!isDie)
        {
            // 보호막(특수1) 또는 특수공격 후 휴식 중이면 대기
            if (shielded || specialResting) { yield return null; continue; }

            // 일반공격 N회
            for (int i = 0; i < normalAttackCountBeforeEnhanced; i++)
            {
                if (isDie) yield break;
                if (shielded || specialResting) break; // 페이즈/휴식 진입 시 중단
                yield return StartCoroutine(NormalAttack());
                if (anim != null) anim.Play("Idle", 0, 0f);
                yield return new WaitForSeconds(normalAttackInterval);
            }

            if (isDie) yield break;
            if (shielded || specialResting) continue;

            // 강화공격 1회
            yield return StartCoroutine(EnhancedAttack());
            yield return new WaitForSeconds(normalAttackInterval);
        }
    }

    // ===================== 일반공격: 보라 구체 3개 일자 =====================
    private IEnumerator NormalAttack()
    {
        if (anim != null) anim.Play("Skill1", 0, 0f);
        yield return new WaitForSeconds(0.25f);

        if (voidOrbPrefab == null || playerTr == null) yield break;

        float orbSpeed = playerBaseSpeed * orbSpeedMultiplier;

        // 같은 자리에서 시간차로 3발 빵빵빵 (세로 일렬로 줄지어 날아감)
        for (int i = 0; i < 3; i++)
        {
            if (isDie || playerTr == null) yield break;
            // 매번 플레이어의 최신 위치로 방향 갱신 (자연스러운 추적)
            Vector2 dir = ((Vector2)playerTr.position - (Vector2)transform.position).normalized;
            Vector3 spawn = transform.position + (Vector3)(dir * 0.6f);
            GameObject o = Instantiate(voidOrbPrefab, spawn, Quaternion.identity);
            var orb = o.GetComponent<VoidOrb>();
            if (orb != null) orb.Setup(SkillDamage, orbSpeed, dir);
            yield return new WaitForSeconds(0.18f); // 빵-빵-빵 간격
        }
    }

    // ===================== 강화공격: 메테오 장판 + 4방향 구체 =====================
    private IEnumerator EnhancedAttack()
    {
        if (anim != null) anim.Play("Skill2", 0, 0f);
        yield return new WaitForSeconds(0.4f);

        if (playerTr == null) yield break;

        // 1) 플레이어 위치에 메테오 장판
        Vector3 meteorPos = playerTr.position;
        var fieldGo = new GameObject("VoidMeteorField");
        fieldGo.transform.position = meteorPos;
        var field = fieldGo.AddComponent<VoidMeteorField>();
        field.Setup(meteorTickDamage, meteorRadius, meteorFieldDuration, meteorWarningTime);

        // 2) 4방향 보라 구체 발사 — 메테오 터진 자리(meteorPos)에서 나가도록
        //    메테오 착지 타이밍에 맞춰 살짝 지연
        yield return new WaitForSeconds(meteorWarningTime);
        if (voidOrbPrefab != null)
        {
            float orbSpeed = playerBaseSpeed * orbSpeedMultiplier;
            Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            foreach (var d in dirs)
            {
                GameObject o = Instantiate(voidOrbPrefab, meteorPos + (Vector3)(d * 0.6f), Quaternion.identity);
                var orb = o.GetComponent<VoidOrb>();
                if (orb != null) orb.Setup(SkillDamage, orbSpeed, d);
            }
        }
        yield return new WaitForSeconds(0.3f);
    }

    // ===================== 페이즈 트리거 체크 =====================
    private void CheckPhaseTriggers()
    {
        float ratio = HpRatio;

        // 특수공격2 (몬스터 소환) — 70/40/10
        for (int i = 0; i < special2Thresholds.Length; i++)
        {
            if (!special2Fired[i] && ratio <= special2Thresholds[i])
            {
                special2Fired[i] = true;
                StartCoroutine(SpecialAttack2_Summon(i));
                return; // 한 프레임에 하나만
            }
        }

        // 특수공격1 (봉인석 + 레이저) — 50/5
        for (int i = special1Pending.Count - 1; i >= 0; i--)
        {
            if (ratio <= special1Pending[i])
            {
                float th = special1Pending[i];
                special1Pending.RemoveAt(i);
                StartCoroutine(SpecialAttack1_SealAndLaser(th));
                return;
            }
        }
    }

    // ===================== 특수공격1: 봉인석 기둥 + 유도 레이저 =====================
    private IEnumerator SpecialAttack1_SealAndLaser(float threshold)
    {
        Debug.Log($"[VoidPriestBoss] === SPECIAL1 (Seal+Laser) at HP {threshold:P0} ===");
        float hpBeforeSnapshot = hp != null ? hp.currentHp : 0f;

        // 보호막 ON (무적)
        AddShield();
        if (anim != null) anim.Play("Skill2", 0, 0f);
        yield return new WaitForSeconds(0.6f);

        // 봉인석 기둥 랜덤 생성
        List<SealPillar> pillars = SpawnPillars();

        // 유도 레이저 5회
        bool allSuccess = true;
        for (int i = 0; i < laserAttackCount; i++)
        {
            if (isDie) { RemoveShield(); yield break; }

            // 살아있는 기둥이 없으면 다시 생성
            pillars.RemoveAll(p => p == null || p.IsDestroyed);
            // ⭐ 모든 기둥 깨졌으면 즉시 성공 (HP 회복 X, 더 이상 레이저 발사 X)
            if (pillars.Count == 0)
            {
                UnityEngine.Debug.Log("[VoidPriestBoss] 모든 기둥 파괴 → 패턴 성공 (즉시 종료)");
                break;
            }

            bool resolved = false;
            bool hitPillar = false;
            var laserGo = new GameObject("GuidedLaser");
            var laser = laserGo.AddComponent<GuidedLaser>();
            laser.Setup(transform, playerTr, laserTrackDuration, laserLockDuration,
                        laserDamage, laserBeamLength, laserBeamWidth,
                        (success) => { hitPillar = success; resolved = true; });

            // 레이저 1회 끝날 때까지 대기
            while (!resolved && !isDie) yield return null;

            // ⭐ 명중 여부 무관 — 최종은 기둥 생존 여부로 판정
            yield return new WaitForSeconds(0.5f);
        }

        // 실패 시 HP 전체 회복
        if (!allSuccess && !isDie)
        {
            if (hp != null) hp.init(hp.MaxHp);
            Debug.Log("[VoidPriestBoss] SPECIAL1 실패 → HP 전체 회복!");
            // 회복했으므로 이 임계값 다시 트리거되게 재등록 (단, 50%에서만 의미)
            // 재도전 유도: 같은 임계값 재등록
            special1Pending.Add(threshold);
            special1Pending.Sort();
        }
        else
        {
            Debug.Log("[VoidPriestBoss] SPECIAL1 성공!");
        }

        // 남은 기둥 정리
        foreach (var p in pillars) if (p != null) Destroy(p.gameObject);

        RemoveShield();

        // ⭐ 특수공격 후 5초 휴식 (플레이어 숨 돌릴 시간) — 메인 루프도 멈춤
        specialResting = true;
        yield return new WaitForSeconds(5f);
        specialResting = false;
    }

    private List<SealPillar> SpawnPillars()
    {
        var list = new List<SealPillar>();
        for (int i = 0; i < pillarCount; i++)
        {
            float ang = (i / (float)pillarCount) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
            float dist = Random.Range(pillarSpawnRange * 0.5f, pillarSpawnRange);
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * dist;

            GameObject pgo;
            if (sealPillarPrefab != null)
                pgo = Instantiate(sealPillarPrefab, pos, Quaternion.identity);
            else
            {
                pgo = new GameObject("SealPillar");
                pgo.transform.position = pos;
                var bc = pgo.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(1f, 2f);
                bc.isTrigger = true;
            }
            var pillar = pgo.GetComponent<SealPillar>();
            if (pillar == null) pillar = pgo.AddComponent<SealPillar>();
            pillar.Setup();
            list.Add(pillar);
        }
        Debug.Log($"[VoidPriestBoss] 봉인석 기둥 {list.Count}개 생성");
        return list;
    }

    private void AddShield()
    {
                shielded = true;
        shieldHp = hp != null ? hp.currentHp : -1f; // 현재 HP 저장 → 무적 유지
        if (shieldVisual == null && sr != null)
        {
            shieldVisual = new GameObject("Shield");
            shieldVisual.transform.SetParent(transform, false);
            shieldVisual.transform.localPosition = Vector3.zero;
            var ssr = shieldVisual.AddComponent<SpriteRenderer>();
            ssr.sprite = sr.sprite;
            ssr.color = new Color(0.6f, 0.4f, 1f, 0.4f);
            ssr.sortingOrder = (sr.sortingOrder) + 1;
            shieldVisual.transform.localScale = Vector3.one * 1.3f;
        }
        else if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    private void RemoveShield()
    {
                shielded = false;
        shieldHp = -1f;
        if (shieldVisual != null) Destroy(shieldVisual);
    }

    // ===================== 특수공격2: 몬스터 소환 =====================
    private IEnumerator SpecialAttack2_Summon(int phaseIndex)
    {
        Debug.Log($"[VoidPriestBoss] === SPECIAL2 (Summon) phase {phaseIndex} (HP {special2Thresholds[phaseIndex]:P0}) ===");
        // ⭐ 소환 시작 → 플레이어를 보스 주변에서 안전 거리로 밀쳐냄 (즉사 방지)
        PushPlayerAway();

        if (anim != null) anim.Play("Summon", 0, 0f);
        // 보스 Summon 애니메이션(11프레임 ≈ 0.92초)이 완전히 끝나도록 대기
        yield return new WaitForSeconds(1.0f);

        switch (phaseIndex)
        {
            case 0: // 70% → 박쥐 10마리
                yield return StartCoroutine(SummonManyCoroutine(batPrefab, 10));
                break;
            case 1: // 40% → 폭발 거미 5 + 새끼 거미 10
                yield return StartCoroutine(SummonManyCoroutine(explosiveSpiderPrefab, 5));
                yield return StartCoroutine(SummonManyCoroutine(babySpiderPrefab, 10));
                break;
            case 2: // 10% → 정예 암석 골렘 2
                yield return StartCoroutine(SummonManyCoroutine(rockGolemElitePrefab, 2));
                break;
        }
        // ⭐ 특수공격 후 5초 휴식 — 메인 루프도 멈춤
        specialResting = true;
        yield return new WaitForSeconds(5f);
        specialResting = false;
    }

    private IEnumerator SummonManyCoroutine(GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[VoidPriestBoss] 소환 프리팹 미할당 (count={count})");
            yield break;
        }

        // 1) 모든 소환 위치를 먼저 계산하고, 각 위치에 summon 이펙트를 표시
        var spawnPositions = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(summonRadius * 0.5f, summonRadius);
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * dist;
            spawnPositions.Add(pos);
            PlaySummonEffect(pos);
        }

        // 2) 이펙트가 보이는 동안 잠깐 대기 (플레이어가 소환 위치 확인 가능)
        yield return new WaitForSeconds(summonEffectDuration);

        // 3) 실제 몬스터 등장
        foreach (var pos in spawnPositions)
        {
            if (isDie) yield break;
            GameObject m = Instantiate(prefab, pos, Quaternion.identity);
            var ai = m.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.Init();
                if (EnemyManager.Instance != null && !EnemyManager.Instance.activeEnemies.Contains(ai))
                    EnemyManager.Instance.activeEnemies.Add(ai);
            }
        }
        Debug.Log($"[VoidPriestBoss] {prefab.name} {count}마리 소환");
    }

    /// <summary>소환 시작 시 플레이어를 보스에서 안전 거리로 밀어냄 (즉사 방지)</summary>
    private void PushPlayerAway()
    {
        if (playerTr == null) return;
        Vector2 toPlayer = (Vector2)playerTr.position - (Vector2)transform.position;
        // 보스랑 같은 자리면 임의 방향
        if (toPlayer.sqrMagnitude < 0.01f) toPlayer = Vector2.right;
        Vector2 dir = toPlayer.normalized;
        Vector3 newPos = transform.position + (Vector3)(dir * pushPlayerDistance);
        playerTr.position = newPos;
        Debug.Log($"[VoidPriestBoss] 플레이어를 {pushPlayerDistance}유닛 밀쳐냄 → {newPos}");
    }

    /// <summary>소환 위치에 summon 이펙트 스프라이트들을 잠시 표시</summary>
    private void PlaySummonEffect(Vector3 pos)
    {
        var go = new GameObject("SummonEffect");
        go.transform.position = pos;
        var esr = go.AddComponent<SpriteRenderer>();
        esr.sortingOrder = 50;
        esr.color = new Color(0.85f, 0.55f, 1f, 1f); // 보라 틴트
        go.transform.localScale = Vector3.one * 1.5f;
        StartCoroutine(PlaySpriteSequence(esr, summonEffectSprites, go, summonEffectDuration));
    }

    /// <summary>스프라이트 배열을 SpriteRenderer에 순차 재생 후 오브젝트 제거 (런타임 안전)</summary>
    private IEnumerator PlaySpriteSequence(SpriteRenderer target, Sprite[] sprites, GameObject host, float duration)
    {
        if (sprites == null || sprites.Length == 0 || target == null)
        {
            yield return new WaitForSeconds(duration);
            if (host != null) Destroy(host);
            yield break;
        }
        float frameInterval = duration / sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (target == null) yield break;
            target.sprite = sprites[i];
            yield return new WaitForSeconds(frameInterval);
        }
        if (host != null) Destroy(host);
    }

    /// <summary>공격/특수기 애니메이션 중일 때 Walk/Idle 강제 전환 방지</summary>
    private bool IsAttackAnimPlaying()
    {
        if (anim == null) return false;
        var st = anim.GetCurrentAnimatorStateInfo(0);
        return st.IsName("Skill1") || st.IsName("Skill2") || st.IsName("Summon");
    }
}