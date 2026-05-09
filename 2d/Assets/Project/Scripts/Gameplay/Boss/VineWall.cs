using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 특수공격: 위/아래로 뻗는 나무 줄기 벽
/// vein3_0 → 1 → 2 → 3 순서로 자라는 1회 애니메이션
/// 플레이어가 닿으면 일정 간격마다 데미지
/// </summary>
public class VineWall : MonoBehaviour
{
    private Vector2 growDir;
    private float length;
    private float width;
    private float damage;
    private float damageInterval;
    private float lifetime;

    private float damageTimer = 0f;
    private PlayerController playerHealth;

    private SpriteRenderer sr;
    private BoxCollider2D bc;
    private Animator anim;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;

        // Animator는 grow 애니메이션을 재생하므로 비활성화하지 않음
        anim = GetComponentInChildren<Animator>();
    }

    public void Setup(Vector2 dir, float length, float width, float damage, float damageInterval, float lifetime)
    {
        this.growDir = dir.normalized;
        this.length = length;
        this.width = width;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.lifetime = lifetime;

        // 콜라이더: 풀 사이즈로 즉시 설정
        bc.size = new Vector2(width, length);
        bc.offset = growDir * (length * 0.5f);

        // 비주얼: dir 방향으로 회전 + 사용자 지정 12.5×12.5 스케일
        if (sr != null)
        {
            float ang = (growDir.y > 0) ? 0f : 180f;
            sr.transform.localRotation = Quaternion.Euler(0, 0, ang);

            // ⭐ 사용자 요구: x, y 둘 다 12.5로
            sr.transform.localScale = new Vector3(12.5f, 12.5f, 1f);

            // 자식 위치를 dir 방향으로 length/2만큼 이동
            sr.transform.localPosition = (Vector3)(growDir * (length * 0.5f));

            sr.sortingOrder = 60;
        }

        Debug.Log($"[VineWall] Setup OK - dir={dir} length={length} lifetime={lifetime} pos={transform.position}");
        StartCoroutine(LifeCycle());
    }

    private IEnumerator LifeCycle()
    {
        float lived = 0f;
        while (lived < lifetime)
        {
            lived += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        Debug.Log($"[VineWall] DESTROYED at {transform.position} time={Time.time}");
    }

        void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerHealth = other.GetComponent<PlayerController>();
            if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerController>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
            damageTimer = 0f;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            if (playerHealth == null)
            {
                playerHealth = other.GetComponent<PlayerController>();
                if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerController>();
            }
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
    }
}
