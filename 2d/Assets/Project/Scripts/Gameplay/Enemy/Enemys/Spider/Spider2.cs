using UnityEngine;
using System.Collections;
using MCPForUnity.Editor.Tools;

public class Spider2 : EnemyAI
{
    [Header("거미 설정")]
    public float explosionRange = 3.0f;    // 폭발 피해 범위
    public float detectRange = 2.5f;       // 자폭 시퀀스 시작 거리
    public float chargeTime = 1.0f;        // 터지기 전 대기 시간
    public float explosionDamage = 50f;    // 폭발 데미지
    public float coolDown = 2.0f;

    public GameObject explosionEffect;

    private bool isActionRunning = false;

    public override void Init()
    {
        base.Init();
        isActionRunning = false;
    }

    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);
        if (isDie || isActionRunning) return;

        float sqrDist = (playerPos - (Vector2)transform.position).sqrMagnitude;
        // 2.5m 이내로 들어오면 공격 시퀀스 시작
        if (sqrDist <= detectRange * detectRange)
        {
            StartCoroutine(ExplosionSequence());
        }
    }

    public override void MoveTaget(Vector2 targetPos)
    {
        // 자폭 준비 중에는 이동 불가
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }

    private IEnumerator ExplosionSequence()
    {
        isActionRunning = true;
        // 1. 즉시 정지 및 물리 고정
        rb.linearVelocity = Vector2.zero;
        // 2. 차징 단계: 1초간 번쩍거리며 경고
        Debug.Log("자폭 카운트다운 시작!");
        yield return StartCoroutine(FlashEffect(chargeTime));
        // 3. 폭발 실행 (이펙트 생성 및 데미지)
        ExecuteExplosion();
        yield return new WaitForSeconds(coolDown);
        isActionRunning = false;
    }

    private void ExecuteExplosion()
    {
        //  시각적 이펙트 생성
        if (explosionEffect != null)
        {
            // 몬스터 위치에 폭발 이펙트 생성
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        // 데미지 판정 (뎀감 없는 고정 피해)
        Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRange, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            // 플레이어에게 고정 데미지 전달
            hit.GetComponent<PlayerStats>().TakeFixedDamage(explosionDamage);
            Debug.Log($"플레이어에게 {explosionDamage}의 고정 피해를 입혔습니다!");
        }
    }
    // 딜레이 동안 번쩍이는 효과 (선택사항)
    private IEnumerator FlashEffect(float duration)
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color origin = sprite.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            sprite.color = (sprite.color == origin) ? Color.red : origin;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        sprite.color = origin;
    }
}