using UnityEngine;
using System.Collections;

public class Wolf2 : EnemyAI
{
    [Header("늑대 특화 설정")]
    private float timer=2.0f;
    private bool isCharging = false;
    private float rushM = 5.0f; 
    private float dashSpeed = 15f;    // 돌진 속도

    // 매니저의 FixedUpdate에서 매 프레임 호출됨
    public override void MoveTaget(Vector2 targetPos)
    {
        if (isCharging) return;
        base.MoveTaget(targetPos);
    }

    // 플레이어가 3m 트리거 콜라이더 안으로 들어오면 자동 실행
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isCharging && other.CompareTag("Player"))
        { 
            StartCoroutine(ChargeAndDash(other.transform));
        }
    }
    IEnumerator ChargeAndDash(Transform playerTransform)
    {
        isCharging = true;

        // 차징 단계 (2초 대기)
        Debug.Log("차징 시작...");

        rb.linearVelocity = Vector2.zero; // 차징 중에는 멈춤
        yield return new WaitForSeconds(timer);

        Vector2 dir = (playerTransform.position - transform.position).normalized;
        HandleSpriteFlip(dir.x);
        // 돌진 단계 (5m 이동)
        Debug.Log("돌진!");
        Vector2 startPos = transform.position;
        float maxDashTime = 1.0f; // 5m 가는데 1초 이상 안걸리게
        float elapsed = 0f;

        while (Vector2.Distance(startPos, transform.position) < rushM && elapsed < maxDashTime)
        {
            if (isDie) yield break;

            rb.linearVelocity = dir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // 후딜레이
        yield return new WaitForSeconds(1.0f);
        isCharging = false;

    }
}