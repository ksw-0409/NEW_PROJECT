using UnityEngine;
using System;
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

    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        // 플레이어 참조 
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        // 액션 중(차징/돌진/후딜)에는 이동 로직 중단
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }
    private void Update()
    {
        // 체크 조건: 죽지 않았고, 액션 중이 아니며, 쿨타임이 끝났을 때
        if (isDie || isActionRunning || !canDash || playerTransform == null) return;

        // 거리 계산 
        float sqrDist = (playerTransform.position - transform.position).sqrMagnitude;
        if (sqrDist < detectRange * detectRange) // 3.0m 이내라면
        {
            ExecuteSequence(PrepareDash(playerTransform));
        }
    }

    // 시퀀스 실행기
    private void ExecuteSequence(IEnumerator sequence)
    {
        StartCoroutine(sequence);
    }

    // 차징 (준비)
    private IEnumerator PrepareDash(Transform target)
    {
        isActionRunning = true;
        canDash = false;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(chargeTime);

        // 다음 단계로 체이닝
        Vector2 direction = (target.position - transform.position).normalized;
        yield return StartCoroutine(PerformDash(direction));
    }

    // 돌진 
    private IEnumerator PerformDash(Vector2 dir)
    {
        HandleSpriteFlip(dir.x);
        Vector2 startPos = transform.position;
        float elapsed = 0f;

        while (Vector2.Distance(startPos, transform.position) < dashDistance && elapsed < 1.5f)
        {
            if (isDie) yield break;
            rb.linearVelocity = dir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        yield return StartCoroutine(PostDashWait());
    }

    //후딜레이 및 종료
    private IEnumerator PostDashWait()
    {
        yield return new WaitForSeconds(postDashDelay);
        isActionRunning = false; // 이제 일반 이동 가능
        // 쿨타임은 별도로 흐르게 함
        StartCoroutine(StartCoolDown());
    }

    //  쿨타임 
    private IEnumerator StartCoolDown()
    {
        yield return new WaitForSeconds(coolDown);
        canDash = true;
        Debug.Log("돌진 준비 완료");
    }
}