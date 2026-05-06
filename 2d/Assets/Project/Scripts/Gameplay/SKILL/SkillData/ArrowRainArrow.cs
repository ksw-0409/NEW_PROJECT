using UnityEngine;

public class ArrowRainArrow : MonoBehaviour
{
    private float damage;
    private float radius;
    private Vector3 targetPos;
    private bool hasImpacted = false;

    [Header("Settings")]
    public float fallSpeed = 25f; // 화살이 떨어지는 속도
    public GameObject impactEffectPrefab; // 폭발 이펙트

    // 스킬 스크립트에서 호출하여 초기 데이터를 설정합니다.
    public void SetData(float dmg, float rad, Vector3 target)
    {
        this.damage = dmg;
        this.radius = rad;
        this.targetPos = target;
        this.hasImpacted = false;

        // 화살이 떨어지는 방향을 바라보게 회전 (선택 사항)
        transform.right = Vector3.down;
    }

    void Update()
    {
        if (hasImpacted) return;

        // 가상의 바닥 지점을 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        // 가상의 바닥 지점에 거의 도달했는지 체크
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            PerformImpact();
        }
    }

    private void PerformImpact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        // 실제 범위 내 적에게 데미지 입히기
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var target in targets)
        {
            if (target.CompareTag("Enemy"))
            {
                EnemyHealth health = target.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }

        // 폭발 이펙트 생성
        if (impactEffectPrefab != null)
        {
            GameObject fx = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.5f);
        }

        // 화살 오브젝트 삭제
        Destroy(gameObject);
    }

    // 혹시 모를 물리 충돌 대비 (선택 사항)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Ground"))
        {
            PerformImpact();
        }
    }
}