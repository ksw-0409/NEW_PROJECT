using UnityEngine;
using System.Collections;

public class Spider2 : EnemyAI
{
    [Header("거미 설정")]
    public float explosionRange = 3.0f;    // 폭발 피해 범위
    public float detectRange = 2.5f;       // 자폭 시퀀스 시작 거리
    public float chargeTime = 1.0f;        // 터지기 전 대기 시간
    public float explosionDamage = 50f;    // 폭발 데미지
    public float coolDown = 2.0f;


    private bool isActionRunning = false;

    public GameObject dashEffectPrefab;
    public float effectDestroyTime = 0.4f;

    [Header("장판 설정")]
    public GameObject indicatorObj;        // 자식으로 넣은 원형 스프라이트 오브젝트 연결

    public Animator anim;
    public override void Init()
    {
        base.Init();
        isActionRunning = false;
        indicatorObj.SetActive(false);
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
        if (isActionRunning)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        base.MoveTaget(targetPos);
    }

    private IEnumerator ExplosionSequence()
    {
        isActionRunning = true;
        // 1. 즉시 정지 및 물리 고정
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("is_ready");
        // 2. 차징 단계: 1초간 번쩍거리며 경고
        Debug.Log("자폭 카운트다운 시작!");
        DrawRange();
        yield return StartCoroutine(FlashEffect(chargeTime));

        indicatorObj.SetActive(false);
        anim.SetTrigger("is_boom");
        ExecuteExplosion();
        yield return new WaitForSeconds(0.3f);
        anim.SetTrigger("is_cool");
        yield return new WaitForSeconds(coolDown-0.3f);
        anim.SetTrigger("is_walk");
        isActionRunning = false;
    }

    private void ExecuteExplosion()
    {
        // 몬스터 위치에 폭발 이펙트 생성
        GameObject effectInstance = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
        Destroy(effectInstance, effectDestroyTime);
        
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
    private void DrawRange()
    {
        indicatorObj.SetActive(true);

        // 부모의 절대 월드 스케일을 가져옵니다 (플립 -1 값 무시)
        float parentScaleX = Mathf.Abs(transform.lossyScale.x);
        float parentScaleY = Mathf.Abs(transform.lossyScale.y);

        if (parentScaleX == 0f) parentScaleX = 1f;
        if (parentScaleY == 0f) parentScaleY = 1f;

        // 최종적으로 화면에 보여야 할 지름 = 반지름(explosionRange) * 2
        float targetDiameter = explosionRange * 2f;

        // 자식의 로컬 스케일 = (목표 지름) / (부모 스케일)
        indicatorObj.transform.localScale = new Vector3(targetDiameter / parentScaleX, targetDiameter / parentScaleY, 1f);
    }
}