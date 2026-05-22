using UnityEngine;
using System.Collections;

public class RockGolem2 : EnemyAI
{
    [Header("암석 골렘 특화 설정")]
    public float rockCount = 8f;
    public float DieDealy = 1.0f;
    public float rockSpeed = 10f;
    public GameObject rockPrefab;
    private bool RockDie = false;


    [SerializeField] private GameObject warningPrefab;   // 바닥 원형 예고 표시
    [SerializeField] private GameObject rockEffectPrefab; // 돌 떨어지는 이펙트
    [SerializeField] private float attackRadius = 1f;     // 판정 반지름 (1미터)
    [SerializeField] private float warningTime = 1f;      // 예고 시간 (1초)
    [SerializeField] private float rockFallInterval = 2f;  // 몇 초마다 실행할지
    private float rockFallTimer = 0f;
    private bool isRockFalling = false;  // 실행 중 중복 방지
    private Transform playerTarget;      // 플레이어 위치 참조

    public override void Init()
    {
        base.Init();
        if (playerTarget == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }
        RockDie = false;
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        if (RockDie) return;
        base.MoveTaget(targetPos);
    }

    public override void OnUpdate(Vector2 playerPos)
    {
        if (RockDie || playerTarget == null) return;
        base.OnUpdate(playerPos);
        rockFallTimer += Time.deltaTime;
        if (rockFallTimer >= rockFallInterval && !isRockFalling)
        {
            rockFallTimer = 0f;
            StartCoroutine(RockFallRoutine());
        }
    }
    public override void Die()
    {
        rb.linearVelocity = Vector2.zero;
        if (RockDie) return;
        StartCoroutine(DieRoutine());
    }
    private void SpawnDeathRocks()
    {
        float angleStep = 360f / rockCount; // 45도 간격
        for (int i = 0; i < rockCount; i++)
        {
            float targetAngle = i * angleStep;
            // 각도를 방향 벡터로 변환
            float radian = targetAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
            // 암석 생성
            GameObject rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);
            Rock rockScript = rock.GetComponent<Rock>();
            if (rockScript != null)
            {
                rockScript.Setup(SkillDamage, rockSpeed, dir);
            }
        }
    }
    
    private IEnumerator DieRoutine()
    {
        RockDie = true;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Color flashColor = Color.red; // 번쩍일 색상 (흰색 원하면 Color.white)

        float elapsed = 0f;
        float flashInterval = 0.1f; // 깜빡이는 속도 (낮을수록 빠름)

        // 1. DieDealy 시간 동안 반복해서 번쩍임
        while (elapsed < DieDealy)
        {
            // 색상 교체 (깜빡임)
            sprite.color = (sprite.color == originalColor) ? flashColor : originalColor;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        SpawnDeathRocks();
        base.Die();
    }

    private IEnumerator RockFallRoutine()
    {
        // 1. 시전 시점의 플레이어 위치를 "고정"
        Vector3 targetPos = playerTarget.position;

        // 2. 그 자리에 예고 표시 생성 (원 크기를 반지름에 맞춤)
        GameObject warning = Instantiate(warningPrefab, targetPos, Quaternion.identity);
        // 스프라이트 지름이 1유닛이라고 가정하면 scale = 반지름 * 2
        warning.transform.localScale = Vector3.one * attackRadius * 2f;

        // 3. 예고 시간만큼 대기
        yield return new WaitForSeconds(warningTime);

        // 4. 예고 표시 제거 + 돌 떨어지는 이펙트
        Destroy(warning);
        GameObject rock = Instantiate(rockEffectPrefab, targetPos, Quaternion.identity);
        Destroy(rock, 1f);

        // 5. 고정된 위치에서 판정 (예고 위치 그대로!)
        Collider2D hit = Physics2D.OverlapCircle(targetPos, attackRadius, LayerMask.GetMask("Player"));
        if (hit != null)
        {
            hit.GetComponent<PlayerStats>().TakeDamage(SkillDamage);
            Debug.Log($"돌 명중! {SkillDamage}");
        }
    }

}