using UnityEngine;
using System.Collections;

public class Wolf2 : EnemyAI
{
    [Header("늑대 특화 설정")]
    public float detectRange = 3.0f; 
    public float chargeTime = 2.0f;
    public float dashDistance = 5.0f;
    public float dashSpeed = 15f;
    public float postDashDelay = 0.5f;
    public float coolDown = 5.0f;

    private bool isActionRunning = false;
    private bool canDash = true;
 
    public override void Init()
    {
        base.Init();
        isActionRunning = false;
        canDash = true;
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        // 액션 중(차징/돌진/후딜)에는 이동 로직 중단
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }
    public override void OnUpdate(Vector2 playerPos)
    {
        // 체크 조건: 죽지 않았고, 액션 중이 아니며, 쿨타임이 끝났을 때
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

        // 1단계: 차징 (준비)
        yield return StartCoroutine(ChargePhase());

        // EnemyManager 사용하여 실시간 위치 확보
        Vector2 currentTargetPos = EnemyManager.Instance.player.position;
        // 최신 위치를 기준으로 방향 계산
        Vector2 direction = (currentTargetPos - (Vector2)transform.position).normalized;
        yield return StartCoroutine(PerformDashPhase(direction));

        // 3단계: 후딜레이 및 상태 복구 (이동 가능해짐)
        yield return StartCoroutine(PostDashPhase());

        // 4단계: 쿨타임 (별도 루틴으로 실행)
        StartCoroutine(CoolDownPhase());
    }

    private IEnumerator ChargePhase()
    {
        rb.linearVelocity = Vector2.zero;
        Debug.Log("늑대: 돌진 준비 중...");
        yield return new WaitForSeconds(chargeTime);
    }

    private IEnumerator PerformDashPhase(Vector2 dir)
    {
        Debug.Log("늑대: 돌진!");
        HandleSpriteFlip(dir.x);
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
        Debug.Log("늑대: 돌진 쿨타임 완료");
    }
}