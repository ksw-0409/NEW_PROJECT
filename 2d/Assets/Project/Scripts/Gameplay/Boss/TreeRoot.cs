using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 특수공격: 땅에서 솟아오르는 나무 뿌리
/// 일정 시간 후 데미지 발생, 그 후 사라짐
/// </summary>
public class TreeRoot : MonoBehaviour
{
    private float damage;
    private float damageDelay;
    private float damageRadius = 0.7f;
    private float lifetime = 1.5f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    public void Setup(float damage, float damageDelay)
    {
        this.damage = damage;
        this.damageDelay = damageDelay;
        StartCoroutine(RootSequence());
    }

    private IEnumerator RootSequence()
    {
        // 등장 모션: 작게 시작해서 위로 솟구
        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x, 0.05f, 1f);
        Color baseColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);

        // 위험지대 가시화 (데미지 들어가기 전 사전 경고)
        {
            SkillRangeIndicator.Spawn(
                transform.position,
                damageRadius,
                new Color(0.55f, 0.3f, 0.1f, 0.9f),
                damageDelay + 0.1f,
                SkillRangeIndicator.Shape.Circle
            );
        }

        // 자라는 모션
        float growT = 0f;
        float growDur = damageDelay;
        while (growT < growDur)
        {
            growT += Time.deltaTime;
            float p = growT / growDur;
            transform.localScale = new Vector3(originalScale.x, Mathf.Lerp(0.05f, originalScale.y, p), 1f);
            if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(0.6f, 1f, p));
            yield return null;
        }

        // 데미지 발생 시점!
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var ph = hit.GetComponent<PlayerController>();
                if (ph == null) ph = hit.GetComponentInParent<PlayerController>();
                if (ph != null) ph.TakeDamage(damage);
                break;
            }
        }

        // 잠시 유지 후 페이드아웃
        float persist = lifetime - damageDelay;
        yield return new WaitForSeconds(Mathf.Max(0.1f, persist * 0.6f));

        float fadeT = 0f;
        float fadeDur = persist * 0.4f;
        while (fadeT < fadeDur)
        {
            fadeT += Time.deltaTime;
            float p = 1f - (fadeT / fadeDur);
            if (sr != null) sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, p);
            yield return null;
        }

        Destroy(gameObject);
    }
}
