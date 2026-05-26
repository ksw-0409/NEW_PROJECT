using System.Collections;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BabySpider2 : EnemyAI
{
    [Header("새끼 거미 특화 설정")]
    public float detectRange = 3.0f;
    public float chargeTime = 2.0f;
    public float dashDistance = 5.0f;
    public float dashSpeed = 15f;
    public float postDashDelay = 0.5f;
    public float coolDown = 5.0f;

    private bool isActionRunning = false;
    private bool canDash = true;


    [Header("시각 효과 설정")]
    public Transform spriteTransform;    // 흔들림 효과를 줄 부모/자식 스프라이트의 Transform
    public float trembleIntensity = 0.25f; // 흔들림강도
    public float trembleStepTime = 0.2f;
    public float indicatorWidth = 0.5f;   // 빨간색 범위 가이드선의 두께
    private LineRenderer lineRenderer; 
    public GameObject dashEffectPrefab; 
    public float effectDestroyTime = 1.0f;

    public Animator animator;

    public override void Init()
    {
        base.Init();
        isActionRunning = false;
        canDash = true;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false; // 평소에는 숨김
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        // 액션 중(차징/돌진/후딜)에는 이동 로직 중단
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }
    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);
        if (isDie || isActionRunning || !canDash) return;

        // 거리 계산 
        float sqrDist = (playerPos - (Vector2)transform.position).sqrMagnitude;
        if (sqrDist < detectRange * detectRange) // 3.0m 이내라면
        {
            StartCoroutine(DashSequence(playerPos));
        }
    }

    // 시퀀스 실행기// --- 메서드 체이닝: 전체 돌진 흐름 제어 ---
    private IEnumerator DashSequence(Vector2 playerPos)
    {
        isActionRunning = true;
        canDash = false;

        // 돌진을 시작할 때 플레이어의 최신 위치를 기반으로 방향을 미리 확정합니다.
        Vector2 currentTargetPos = EnemyManager.Instance.player.position;
        Vector2 direction = (currentTargetPos - (Vector2)transform.position).normalized;

        animator.SetTrigger("is_ready");
        // 1단계: 차징 (준비 + 흔들림 + 범위 표시)
        yield return StartCoroutine(ChargePhase(direction));

        animator.SetTrigger("is_d");
        // 2단계: 돌진 실행 (범위 표시 끄고 돌격)
        yield return StartCoroutine(PerformDashPhase(direction));

        animator.SetTrigger("is_dend");
        // 3단계: 후딜레이 및 상태 복구
        yield return StartCoroutine(PostDashPhase());

        animator.SetTrigger("is_cool");
        // 4단계: 쿨타임
        StartCoroutine(CoolDownPhase());
    }

    private IEnumerator ChargePhase(Vector2 dir)
    {
        rb.linearVelocity = Vector2.zero;
        Debug.Log(" 돌진 준비 중...");

        Vector3 originalSpritePos = spriteTransform != null ? spriteTransform.localPosition : Vector3.zero;
        // 매번 new Material을 하지 않고, 이미 LineRenderer에 붙어있는 재질의 색상만 바꿉니다.
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = indicatorWidth;
            lineRenderer.endWidth = indicatorWidth;

            // 인스펙터에 등록된 기본 재질이 없다면 최초 1번만 생성하도록 예외처리하거나 
            // 그냥 인스펙터에서 마우스로 Material을 넣어두는 것이 가장 좋습니다.
            if (lineRenderer.sharedMaterial == null)
            {
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            lineRenderer.startColor = new Color(1f, 0f, 0f, 0.6f);
            lineRenderer.endColor = new Color(1f, 0f, 0f, 0.1f);
            lineRenderer.enabled = true;
            Vector2 startLine = transform.position;
            Vector2 endLine = startLine + (dir * dashDistance);
            lineRenderer.SetPosition(0, startLine);
            lineRenderer.SetPosition(1, endLine);
        }

        //유니티의 프레임 밀림 현상을 무력화하기 위해 절대 시간 계산법 도입
        float startTime = Time.time;
        float endTime = startTime + chargeTime;

        while (Time.time < endTime)
        {
            if (isDie) break;
            if (spriteTransform != null)
            {
                // trembleIntensity를 인스펙터에서 0.3 ~ 0.5 정도로 줘보세요 (2D 유닛 기준)
                float offsetX = Random.Range(-trembleIntensity, trembleIntensity);
                float offsetY = Random.Range(-trembleIntensity, trembleIntensity);
                spriteTransform.localPosition = originalSpritePos + new Vector3(offsetX, offsetY, 0);
            }
            yield return null;
        }

        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    private IEnumerator PerformDashPhase(Vector2 dir)
    {
        HandleSpriteFlip(dir.x);
        Debug.Log(" 돌진!");
        GameObject effectInstance = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        Vector3 effectScale = effectInstance.transform.localScale;
        if (isFlip)
        {
            effectScale.x = -Mathf.Abs(effectScale.x); // 왼쪽을 보고 있으면 이펙트도 마이너스
        }
        else
        {
            effectScale.x = Mathf.Abs(effectScale.x);  // 오른쪽을 보고 있으면 플러스
        }
        effectInstance.transform.localScale = effectScale;
        // 이펙트가 무한히 남아 메모리를 갉아먹지 않도록 지정된 시간 뒤에 자동 삭제
        Destroy(effectInstance, effectDestroyTime);
        Vector2 startPos = transform.position;
        float elapsed = 0f;

        // 거리 기준 또는 시간 제한(끼임 방지) 동안 돌진
        while (Vector2.Distance(startPos, transform.position) < dashDistance && elapsed < 1.5f)
        {
            if (isDie) yield break;
            rb.linearVelocity = dir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator PostDashPhase()
    {
        yield return new WaitForSeconds(postDashDelay);
        isActionRunning = false; // 이제 다시 일반 이동(MoveTaget) 가능
    }

    private IEnumerator CoolDownPhase()
    {
        yield return new WaitForSeconds(coolDown);
        canDash = true;
        Debug.Log("돌진 쿨타임 완료");
    }
}