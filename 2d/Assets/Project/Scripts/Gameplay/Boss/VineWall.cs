using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 특수공격: 나무 줄기 벽
/// 여러 vein 인스턴스를 길이 방향으로 줄지어 생성하고, 각각 vein3_0 → 1 → 2 → 3 grow 애니메이션을 시간 차로 재생.
/// 결과: vein이 한 칸씩 줄지어 자라나는 자연스러운 효과.
/// 자라기 완료 후 솔리드 콜라이더 활성화 (벽 + 데미지).
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
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

    private BoxCollider2D bc;

    [Header("Grow Animation")]
    [Tooltip("각 vein 단위의 월드 사이즈 (sprite native 0.62 기준 — scale 곱해 표시)")]
    [SerializeField] private float perVeinLength = 2.5f; // 사용자 요청: vein 간격 띄움
    [Tooltip("한 vein이 다 자라는 데 걸리는 시간")]
    [SerializeField] private float perVeinGrowTime = 0.25f;
    [Tooltip("다음 vein이 자라기 시작하는 간격 (작을수록 동시에 자라남)")]
    [SerializeField] private float perVeinSpawnInterval = 0.08f;

    [Header("Sprite Frames")]
    [SerializeField] private Sprite[] growFrames; // vein3_0, _1, _2, _3 순서

    private List<GameObject> spawnedVeins = new List<GameObject>();
    private bool growComplete = false;

    void Awake()
    {
        bc = GetComponent<BoxCollider2D>();
        if (bc == null) bc = gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = false;
        bc.enabled = false; // 자라는 동안 비활성

        // prefab의 자식 Visual을 비활성화 (단일 sprite — 이제 안 씀)
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Setup(Vector2 dir, float length, float width, float damage, float damageInterval, float lifetime)
    {
        // ⭐ 나무 줄기 시각/콜라이더 2배 스케일 (사용자 요청)
        transform.localScale = new Vector3(2f, 2f, 1f);

        this.growDir = dir.normalized;
        this.length = length;
        this.width = width;
        this.damage = damage;
        this.damageInterval = damageInterval;
        this.lifetime = lifetime;

        // 콜라이더 size, offset
        bool isHorizontal = Mathf.Abs(growDir.x) > Mathf.Abs(growDir.y);
        if (isHorizontal) bc.size = new Vector2(length, width);
        else              bc.size = new Vector2(width, length);
        bc.offset = growDir * (length * 0.5f);

        Debug.Log($"[VineWall] Setup OK - dir={dir} length={length} perVein={perVeinLength}");

        StartCoroutine(GrowSequenceRoutine());
        StartCoroutine(LifeCycle());
    }

    private IEnumerator GrowSequenceRoutine()
    {
        if (growFrames == null || growFrames.Length == 0)
        {
            Debug.LogWarning("[VineWall] growFrames not assigned!");
            yield break;
        }

        int count = Mathf.Max(1, Mathf.RoundToInt(length / perVeinLength));
        float step = length / count;

        for (int i = 0; i < count; i++)
        {
            // i번째 vein 위치 — 가운데 anchor에서 dir 방향으로 (i + 0.5) * step 만큼
            Vector3 pos = (Vector3)(growDir * (step * (i + 0.5f)));
            StartCoroutine(SpawnAndGrowOne(pos, step));
            yield return new WaitForSeconds(perVeinSpawnInterval);
        }

        // 모든 vein이 grow 시작했으면 마지막 vein이 끝날 때까지 대기
        yield return new WaitForSeconds(perVeinGrowTime);

        // 자라기 완료 — 콜라이더 활성화
        if (bc != null) bc.enabled = true;
        growComplete = true;
    }

    private IEnumerator SpawnAndGrowOne(Vector3 localPos, float lenSlot)
    {
        // 새 GameObject (자식)에 SpriteRenderer + Animator 없이 직접 sprite swap
        var go = new GameObject("Vein");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;

        // 회전: dir 방향대로 (sprite native 가로 0.62 × 세로 0.20)
        float ang = Mathf.Atan2(growDir.y, growDir.x) * Mathf.Rad2Deg;
        go.transform.localRotation = Quaternion.Euler(0, 0, ang);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 60;
        sr.sprite = growFrames[0];
        // 한 vein 단위가 lenSlot 길이를 차지하도록 — sprite native 비율 (0.62×0.20) 그대로 유지하기 위해 X/Y 동일 scale
        // 1:1 비율: scaleX = scaleY = lenSlot / 0.62 → vein sprite가 뭉개지지 않음
        float uniformScale = lenSlot / 0.62f;
        sr.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);

        spawnedVeins.Add(go);

        // grow 애니: vein3_0 → 1 → 2 → 3 swap (perVeinGrowTime 동안)
        float frameTime = perVeinGrowTime / growFrames.Length;
        for (int f = 0; f < growFrames.Length; f++)
        {
            if (sr == null) yield break;
            sr.sprite = growFrames[f];
            yield return new WaitForSeconds(frameTime);
        }
        // 마지막 프레임(vein3_3)에서 정지
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
        Debug.Log($"[VineWall] DESTROYED at {transform.position}");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerHealth = collision.collider.GetComponent<PlayerController>();
            if (playerHealth == null) playerHealth = collision.collider.GetComponentInParent<PlayerController>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);
            damageTimer = 0f;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            if (playerHealth == null)
            {
                playerHealth = collision.collider.GetComponent<PlayerController>();
                if (playerHealth == null) playerHealth = collision.collider.GetComponentInParent<PlayerController>();
            }
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
    }
}
