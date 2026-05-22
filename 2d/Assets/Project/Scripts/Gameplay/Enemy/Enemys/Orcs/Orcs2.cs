using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Orcs2 : EnemyAI
{
    [Header("오크 특화 설정")]
    public float attackRange = 3.0f;     // 감지 및 공격 시작 거리
    public float skillRange = 5.0f;      // 부채꼴 공격 사거리
    public float skillAngle = 30.0f;     // 부채꼴 각도
    public float attackDelay = 0.8f;     // 찍기 전 대기 시간 (모션 시간)


    public float attackCoolDown = 3.0f; // 공격 쿨타임
    public float attackAfterDelay = 0.3f; //공격 후딜

    [Header("범위 표시 설정")]
    // 인스펙터에서 아까 만든 그 UI Image를 여기에 드래그해서 연결할 겁니다.
    public Image indicatorImage;

    private Animator anim;
    private bool isActionRunning = false;
    private bool canAttack = true;

    public GameObject dashEffectPrefab;
    public float effectDestroyTime = 0.4f;
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        indicatorImage.gameObject.SetActive(false);

    }
    public override void Init()
    {
        base.Init();
        isActionRunning = false;
        canAttack = true;
    }

    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);
        if (isDie || isActionRunning) return;

        float distance = Vector2.Distance(transform.position, playerPos);

        if (canAttack && distance <= attackRange)
        {
            // 공격 범위 안으로 들어오면 공격 시퀀스 시작
            StartCoroutine(AttackSequence(playerPos));
        }
    }

    public override void MoveTaget(Vector2 targetPos)
    {
        // 액션 중(차징/돌진/후딜)에는 이동 로직 중단
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }
    private IEnumerator AttackSequence(Vector2 targetPos)
    {
        canAttack = false;
        isActionRunning = true;
        rb.linearVelocity = Vector2.zero; // 공격 시작 시 정지
        Vector2 attackDir = (targetPos - (Vector2)transform.position).normalized;
        DrawSectorIndicator(attackDir);
        yield return new WaitForSeconds(attackDelay*0.4f);
        anim.SetTrigger("isSkill"); 
        // 땅을 찍는 시점까지의 딜레이
        yield return new WaitForSeconds(attackDelay*0.6f);

        // 3. 공격 순간 범위 표시 끄기 및 실제 타격
        indicatorImage.gameObject.SetActive(false);
        // 부채꼴 공격 수행
        PerformSectorAttack(targetPos);

        // 후딜레이 및 상태 복구
        yield return StartCoroutine(PostAttackPhase());

        // 쿨타임 (별도 루틴으로 실행하여 이동 가능하게 함)
        StartCoroutine(CoolDownPhase());
    }

    private void PerformSectorAttack(Vector2 targetPos)
    {
        GameObject effectInstance = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effectInstance, effectDestroyTime);
        // 공격 방향 계산 (플레이어 방향)
        Vector2 attackDir = (targetPos - (Vector2)transform.position).normalized;
        float attackAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        // 범위 내 모든 Collider2D 검사 (레이어 마스크를 Player로 설정하는 것이 최적화에 좋습니다)
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, skillRange);

        foreach (var col in targets)
        {
            if (col.CompareTag("Player"))
            {
                Vector2 dirToPlayer = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                float angle = Vector2.Angle(attackDir, dirToPlayer);
                // 설정한 각도의 절반(15도) 이내에 있다면 부채꼴 범위 안임
                if (angle <= skillAngle * 0.5f)
                {
                    PlayerStats pStats = col.GetComponent<PlayerStats>();
                    if (pStats != null)
                    {
                        pStats.TakeDamage(SkillDamage);
                        Debug.Log($"오크가 부채꼴 공격으로 {SkillDamage} 데미지를 입혔습니다.");
                    }
                }
            }
        }
    }

    private IEnumerator PostAttackPhase()
    {
        yield return new WaitForSeconds(attackAfterDelay);
        isActionRunning = false; // 이제 다시 MoveTaget이 작동 가능함
    }

    private IEnumerator CoolDownPhase()
    {
        yield return new WaitForSeconds(attackCoolDown);
        canAttack = true;
    }
    private void DrawSectorIndicator(Vector2 attackDir)
    {
        if (indicatorImage == null) return;
        indicatorImage.gameObject.SetActive(true);

        // 1. 부모(오크) 스케일 및 뒤집힘 완벽 무효화
        Transform canvasTr = indicatorImage.transform.parent;
        Vector3 parentScale = transform.localScale;

        canvasTr.localScale = new Vector3(
            parentScale.x != 0 ? 1f / parentScale.x : 1f,
            parentScale.y != 0 ? 1f / parentScale.y : 1f,
            parentScale.z != 0 ? 1f / parentScale.z : 1f
        );

        // 2. 사거리 및 부채꼴 비율 적용
        RectTransform rt = indicatorImage.rectTransform;
        float targetSize = skillRange * 2f;
        rt.sizeDelta = new Vector2(targetSize, targetSize);
        indicatorImage.fillAmount = skillAngle / 360f;

        // 3. 🔥 타겟 방향 완벽 조준
        float baseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        // Origin을 Right로 맞췄으므로, 정중앙 정렬을 위해 절반 각도를 '더해'줍니다.
        float finalAngle = baseAngle + (skillAngle * 0.5f);

        // 절대적인 월드 각도 회전
        indicatorImage.transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
    }
}


