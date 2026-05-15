using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직선으로 날아가며 경로상의 적에게 데미지를 주는 검기 투사체.
/// 시각 스프라이트의 실제 크기와 BoxCast 판정 영역을 1:1 동기화합니다.
/// </summary>
public class SwordWaveProjectile : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("초당 이동 속도")]
    public float speed = 18f;

    [Tooltip("최대 사거리. 이 거리만큼 전진 후 자동 파괴")]
    public float maxRange = 6f;

    [Header("판정 설정")]
    [Tooltip("판정 박스의 두께(세로). Init에서 스프라이트 세로 크기와 일치하도록 자동 조정")]
    public float thickness = 1.5f;

    [Tooltip("같은 적을 다시 타격할 수 있는지 (false 권장)")]
    public bool canHitSameTargetTwice = false;

    [Header("스프라이트 보정")]
    [Tooltip("프리팹 원본 스프라이트의 진행방향(가로) native 크기 (units)")]
    public float nativeSpriteWidth = 6.0f;

    [Tooltip("프리팹 원본 스프라이트의 두께방향(세로) native 크기 (units)")]
    public float nativeSpriteHeight = 6.0f;

    [Tooltip("스프라이트의 가로 길이를 사거리(range)에 맞춰 늘릴지 여부.\n" +
            "false면 등비 확대(모양 유지), true면 가로만 range에 맞춰 길게 늘림.")]
    public bool stretchToRange = false;

    [Tooltip("판정 박스를 시각 스프라이트보다 살짝 작게 잡는 비율 (0.7~1.0 권장).\n" +
            "픽셀 아트는 알파 여백이 있어서 1.0이면 너무 관대하게 맞을 수 있음.")]
    [Range(0.3f, 1.0f)]
    public float hitboxFitRatio = 0.75f;

    // 런타임 상태
    private Vector2 direction = Vector2.right;
    private float damage;
    private float traveledDistance;

    // ⭐ 시각 크기에 맞춘 실제 판정 크기 (월드 단위)
    private float hitWidth;   // 진행 방향(가로) 판정 너비
    private float hitHeight;  // 수직 방향(세로) 판정 두께

    private readonly HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();

    public void Init(Vector2 dir, float dmg, float range, float moveSpeed, float boxThickness)
    {
        direction = dir.normalized;
        damage = dmg;
        maxRange = range;
        speed = moveSpeed;
        thickness = boxThickness;

        // 발사 방향에 맞춰 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 1) 스프라이트 스케일 계산
        float scaleY = thickness / Mathf.Max(0.001f, nativeSpriteHeight);
        float scaleX = stretchToRange
            ? maxRange / Mathf.Max(0.001f, nativeSpriteWidth)
            : scaleY; // 등비 확대 - 모양 유지

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // 2) ⭐ 시각 크기와 판정 박스를 동기화
        //    스프라이트가 화면상 차지하는 실제 크기 = native * scale
        //    판정은 그보다 살짝 작게 (hitboxFitRatio) 잡아 픽셀 아트 알파 여백 보정
        hitWidth = nativeSpriteWidth * scaleX * hitboxFitRatio;
        hitHeight = nativeSpriteHeight * scaleY * hitboxFitRatio;

        // BoxCollider2D도 동일하게 (디버그/시각 확인용)
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            box.size = new Vector2(nativeSpriteWidth, nativeSpriteHeight) * hitboxFitRatio;
        }
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        if (traveledDistance + step >= maxRange)
        {
            step = maxRange - traveledDistance;
        }

        if (step > 0f)
        {
            CheckHits(step);
            transform.position += (Vector3)(direction * step);
            traveledDistance += step;
        }

        if (traveledDistance >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    void CheckHits(float castDistance)
    {
        // ⭐ 박스 크기: 시각 스프라이트와 동일한 크기로 캐스트
        //    진행 방향(가로) = hitWidth, 수직(세로) = hitHeight
        //    이러면 검기 그림 안에 들어오는 모든 적이 정확히 판정됩니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(hitWidth, hitHeight),
            angle,
            direction,
            castDistance
        );

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (!hit.collider.CompareTag("Enemy")) continue;

            if (!canHitSameTargetTwice && hitColliders.Contains(hit.collider))
                continue;

            var enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitColliders.Add(hit.collider);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.8f, 0.6f);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Matrix4x4 rot = Matrix4x4.TRS(transform.position, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rot;
        float w = hitWidth > 0f ? hitWidth : nativeSpriteWidth * hitboxFitRatio;
        float h = hitHeight > 0f ? hitHeight : nativeSpriteHeight * hitboxFitRatio;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(w, h, 0.1f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
