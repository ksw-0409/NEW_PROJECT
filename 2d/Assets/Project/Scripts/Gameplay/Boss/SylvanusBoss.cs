using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 5층 보스 — 실바누스 (나무 보스)
/// 패턴: 일반공격 8회(또는 광폭화 시 4회) → 특수공격 1회 연계 → 7초 프리딜 → 반복
/// 일반공격: 전방 120도에 나무 파편 12발 산탄
/// 특수공격(랜덤 1/2):
///   1) 플레이어 위치에 위/아래 나무 줄기 생성 (특수 2회 끝나면 사라짐)
///   2) 플레이어 위치에 나무 뿌리 15회 생성
/// HP 30% 이하: 일반공격 8회 → 4회로 감소 (광폭화)
/// </summary>
public class SylvanusBoss : EnemyAI
{
    [Header("산탄(일반공격)")]
    public GameObject woodNeedlePrefab;
    public int needleCount = 12;
    public float needleSpread = 120f;          // 부채꼴 각도
    public float needleSpeed = 9f;
    public float normalAttackInterval = 3f;     // 평타 속도 — 3초에 한번
    public int normalAttackCountBase = 8;       // 평타 8회 후 특수
    public int normalAttackCountFrenzy = 4;     // 광폭화 시 4회
    public float frenzyHpThreshold = 0.30f;     // HP 30% 이하

    [Header("나무 줄기(특수1)")]
    public GameObject vinePrefab;
    public float vineLength = 4f;               // 위/아래 각각 길이
    public float vineDuration = 99f;            // 특수 2회 끝나면 사라짐 (코드로 관리)
    public float vineDamageInterval = 1f;
    public float vineWidth = 0.8f;

    [Header("나무 뿌리(특수2)")]
    public GameObject rootPrefab;
    public int rootCount = 15;
    public float rootInterval = 0.18f;          // 15회 빠르게
    public float rootSpawnRadius = 1.0f;        // 플레이어 위치 주변 분포
    public float rootDamageDelay = 0.6f;        // 뿌리 솟구 후 데미지 발생까지 시간

    [Header("연계 / 프리딜")]
    public float postComboFreeTime = 7f;        // 연계 후 프리딜 시간

    [Header("기타")]
    public Color rangeIndicatorColor = new Color(0.45f, 0.85f, 0.25f, 0.85f);

    private Animator anim;
    private SpriteRenderer sr;
    private Transform playerTr;

    private bool patternActive = false;
    private int specialCounter = 0;             // 특수공격 발동 횟수 (vine 사라짐 트리거용)
    private int vinePicks = 0;                  // 진단용: vine이 얼마나 뽑혔는지
    private int rootsPicks = 0;                 // 진단용: roots가 얼마나 뽑혔는지
    private System.Random patternRng = new System.Random(); // 보스 패턴 전용 RNG (System.Random은 시간 시드로 매번 다름)
    private List<GameObject> activeVines = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public override void Init()
    {
        // 보스 전용 — base.Init()을 호출하면 풀링/Speed 설정 등이 NRE를 낼 수 있어 직접 처리
        isDie = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        int floor = GameDataManager.Instance != null ? GameDataManager.Instance.CurrentFloor : 5;
        int startFloor = data != null ? data.startfloor : 5;
        float scaledHp = data != null ? data.hp * (1f + Mathf.Max(0, floor - startFloor) * 0.3f) : 1500f;
        var hp = GetComponent<EnemyHealth>();
        if (hp != null) hp.init(scaledHp);

        SkillDamage = data != null ? data.skillDamage * (1f + Mathf.Max(0, floor - startFloor) * 0.15f) : 25f;

        patternActive = false;
        specialCounter = 0;
        ClearAllVines();

        if (EnemyManager.Instance != null) playerTr = EnemyManager.Instance.player;
        if (playerTr == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) playerTr = pgo.transform;
        }
        Debug.Log($"[SylvanusBoss] Init - HP:{scaledHp} SkillDmg:{SkillDamage} playerTr:{(playerTr != null ? playerTr.name : "NULL")} anim:{(anim != null ? anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NoController" : "NoAnimator")}");

        StartCoroutine(MainPatternLoop());
    }

    private float diagTimer = 0f;

    // 피격 효과
    private float lastHp = -1f;
    private Color originalColor;
    private float flashEndTime = 0f;
    private const float FLASH_DURATION = 0.12f;
    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);
        if (isDie) return;

        // 보스는 좌우 플립으로 플레이어를 바라보게
        if (sr != null && playerTr != null)
        {
            sr.flipX = playerTr.position.x < transform.position.x;
        }

        // sr 가시성 강제 보장 (다른 시스템이 disable하거나 sprite를 null로 만드는 것 방지)
        if (sr != null && !sr.enabled) sr.enabled = true;

        // 피격 감지 — currentHp가 줄면 flash. 평소엔 매 프레임 흰색으로 강제 (다른 시스템이 투명으로 만드는 것 방지)
        var hpComp = GetComponent<EnemyHealth>();
        if (hpComp != null && sr != null)
        {
            // flash 중이 아니면 항상 흰색 불투명으로 강제
            if (flashEndTime <= 0)
            {
                if (sr.color != Color.white) sr.color = Color.white;
            }
            else if (Time.time >= flashEndTime)
            {
                // flash 끝 → 흰색으로 복원
                sr.color = Color.white;
                flashEndTime = 0f;
            }

            // 새 데미지 감지
            if (lastHp > 0 && hpComp.currentHp < lastHp - 0.001f)
            {
                if (flashEndTime <= 0)
                {
                    sr.color = new Color(1f, 0.3f, 0.3f, 1f);
                    Debug.Log($"[SylvanusBoss] HIT FLASH | hp {lastHp} -> {hpComp.currentHp}");
                }
                flashEndTime = Time.time + FLASH_DURATION;
            }
            lastHp = hpComp.currentHp;
        }

        // 진단: 5초마다 상태 로그 (patternActive false인 채로 유지되면 패턴이 죽었다는 뜻)
        diagTimer += Time.deltaTime;
        if (diagTimer >= 5f)
        {
            diagTimer = 0f;
            Debug.Log($"[SylvanusBoss] alive | patternActive={patternActive} playerTr={(playerTr != null)} hp={GetComponent<EnemyHealth>()?.currentHp}");
        }
    }

    // 보스는 일반 추격 안 함(고정형 보스)
    public override void MoveTaget(Vector2 targetPos)
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void Die()
    {
        if (isDie) return;
        ClearAllVines();
        StopAllCoroutines();
        base.Die();
    }

    private bool IsFrenzy()
    {
        var hp = GetComponent<EnemyHealth>();
        if (hp == null) return false;
        return hp.currentHp / Mathf.Max(1f, hp.MaxHp) <= frenzyHpThreshold;
    }

    private IEnumerator MainPatternLoop()
    {
        patternActive = true;
        Debug.Log("[SylvanusBoss] Pattern loop started");
        yield return new WaitForSeconds(0.5f); // 등장 직후 짧게 대기

        while (!isDie)
        {
            int normalCount = IsFrenzy() ? normalAttackCountFrenzy : normalAttackCountBase;

            // 1) 일반공격 N회
            for (int i = 0; i < normalCount; i++)
            {
                if (isDie) yield break;
                yield return StartCoroutine(NormalAttack());
                // Skill 애니메이션이 끝났을 시점에 idle 강제 — 다음 패턴 위해
                if (anim != null) anim.Play("Idle", 0, 0f);
                yield return new WaitForSeconds(normalAttackInterval);
            }

            if (isDie) yield break;

            // 2) 특수공격 1회 (랜덤)
            int pick = patternRng.Next(0, 2); // 0 = 줄기, 1 = 뿌리 (System.Random)
            Debug.Log($"[SylvanusBoss] === SPECIAL pick={pick} ({(pick == 0 ? "Vine" : "Roots")}) ==="); 
            if (pick == 0)
                yield return StartCoroutine(SpecialAttack_Vine());
            else
                yield return StartCoroutine(SpecialAttack_Roots());

            // specialCounter 누적 로직 제거 — vine 정리는 SpecialAttack_Vine 시작 시 자동으로 처리됨
            // (특수공격 2번 마다 강제로 지우지 않음 — 다음 vine 발동될 때까지 살아있음)

            // 3) 프리딜 7초
            if (isDie) yield break;
            yield return new WaitForSeconds(postComboFreeTime);
        }
    }

    // ===================== 일반공격: 산탄 =====================
    private IEnumerator NormalAttack()
    {
        Debug.Log("[SylvanusBoss] NormalAttack start");
        if (anim != null) anim.Play("Skill1", 0, 0f);
        yield return new WaitForSeconds(0.25f); // 휘두르는 순간까지 살짝 대기

        if (woodNeedlePrefab == null || playerTr == null) yield break;

        Vector2 dirToPlayer = ((Vector2)playerTr.position - (Vector2)transform.position).normalized;
        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        float halfSpread = needleSpread * 0.5f;

        for (int i = 0; i < needleCount; i++)
        {
            float t = needleCount == 1 ? 0.5f : (float)i / (needleCount - 1);
            float angle = baseAngle - halfSpread + needleSpread * t;
            // 약간의 랜덤 산란
            angle += Random.Range(-3f, 3f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject n = Instantiate(woodNeedlePrefab, transform.position + (Vector3)(dir * 0.6f), Quaternion.Euler(0, 0, angle));
            var needle = n.GetComponent<WoodNeedle>();
            if (needle != null) needle.Setup(SkillDamage, needleSpeed, dir);
        }

        // 산탄 부채꼴 가시화 — 플레이어가 어디까지가 위험한지 보이게
        {
            SkillRangeIndicator.SpawnSector(
                transform.position,
                dirToPlayer,
                12f,
                needleSpread,
                new Color(0.45f, 0.85f, 0.25f, 0.6f),
                0.45f
            );
        }
    }

    // ===================== 특수공격 1: 위/아래 나무 줄기 =====================
    private IEnumerator SpecialAttack_Vine()
    {
        // 새 vine 발동 시 이전 vine부터 정리 (한 번에 보이는 vine은 위/아래 1쌍만)
        ClearAllVines();

        if (anim != null) anim.Play("Skill2", 0, 0f);
        yield return new WaitForSeconds(0.5f);

        if (vinePrefab == null || playerTr == null) yield break;

        Vector3 anchor = playerTr.position;

        // 위쪽 줄기
        GameObject up = Instantiate(vinePrefab, anchor, Quaternion.identity);
        var vUp = up.GetComponent<VineWall>();
        if (vUp != null) vUp.Setup(Vector2.up, vineLength, vineWidth, SkillDamage, vineDamageInterval, vineDuration);
        activeVines.Add(up);

        // 아래쪽 줄기
        GameObject down = Instantiate(vinePrefab, anchor, Quaternion.identity);
        var vDown = down.GetComponent<VineWall>();
        if (vDown != null) vDown.Setup(Vector2.down, vineLength, vineWidth, SkillDamage, vineDamageInterval, vineDuration);
        activeVines.Add(down);

        yield return new WaitForSeconds(0.4f);
    }

    // ===================== 특수공격 2: 나무 뿌리 15회 =====================
    private IEnumerator SpecialAttack_Roots()
    {
        if (anim != null) anim.Play("Skill2", 0, 0f);
        yield return new WaitForSeconds(0.4f);

        if (rootPrefab == null || playerTr == null) yield break;

        for (int i = 0; i < rootCount; i++)
        {
            if (isDie) yield break;
            // 플레이어 현재 위치 + 약간의 랜덤
            Vector2 origin = playerTr.position;
            Vector2 offset = Random.insideUnitCircle * rootSpawnRadius;
            Vector3 spawn = (Vector3)(origin + offset);

            GameObject r = Instantiate(rootPrefab, spawn, Quaternion.identity);
            var root = r.GetComponent<TreeRoot>();
            if (root != null) root.Setup(SkillDamage, rootDamageDelay);

            yield return new WaitForSeconds(rootInterval);
        }
    }

    private void ClearAllVines()
    {
        foreach (var v in activeVines)
        {
            if (v != null) Destroy(v);
        }
        activeVines.Clear();
    }
}
