using UnityEngine;
using System.Collections;

public class ArrowRainArea : MonoBehaviour
{
    [Header("Settings")]
    public GameObject fallingArrowPrefab; // 실제 데미지를 주는 화살 객체
    public float areaRadius = 3f;         // 화살이 떨어지는 범위

    private float damage;
    private float duration;
    private bool isExecution;
    private bool isConcentrated;
    private CircleCollider2D areaCollider;
    private Vector3 baseScale;
    private float baseColliderRadius = 1f;

    private void Awake()
    {
        areaCollider = GetComponent<CircleCollider2D>();
        baseScale = transform.localScale;
        if (areaCollider != null)
            baseColliderRadius = Mathf.Max(0.01f, areaCollider.radius);
    }

    public void Setup(float dmg, float mult, float dur, bool exec, bool conc, float radius)
    {
        this.damage = dmg * mult;
        this.duration = dur;
        this.isExecution = exec;
        this.isConcentrated = conc;
        this.areaRadius = Mathf.Max(0.1f, radius * 2f);
        ApplyAreaScale(this.areaRadius);

        StartCoroutine(RainRoutine());
        Destroy(gameObject, duration + 1f); // 모든 화살이 떨어질 때까지 대기 후 파괴
    }

    IEnumerator RainRoutine()
    {
        float elapsed = 0;
        float spawnInterval = isConcentrated ? 0.08f : 0.25f;

        float actualRadius = GetActualWorldRadius();

        while (elapsed < duration)
        {
            // 3. 계산된 실제 반지름 안에서만 랜덤 좌표 생성
            Vector2 randomPos = (Vector2)transform.position + (Random.insideUnitCircle * actualRadius);

            Vector3 spawnPos = new Vector3(randomPos.x, randomPos.y + 10f, 0);

            GameObject arrow = Instantiate(fallingArrowPrefab, spawnPos, Quaternion.Euler(0, 0, -90));
            // 생성 직후 크기를 현재의 절반(0.5) 혹은 더 작게(0.3) 조절
            arrow.transform.localScale = Vector3.one * 0.3f;
            var arrowScript = arrow.GetComponent<FallingArrow>();
            if (arrowScript != null)
            {
                arrowScript.Initialize(damage, isExecution, isConcentrated, randomPos, (Vector2)transform.position, actualRadius);
            }

            yield return new WaitForSeconds(spawnInterval);
            elapsed += spawnInterval;
        }
    }

    private void ApplyAreaScale(float targetWorldRadius)
    {
        if (areaCollider == null)
            return;

        float currentBaseWorldRadius = baseColliderRadius * Mathf.Abs(baseScale.x);
        currentBaseWorldRadius = Mathf.Max(0.01f, currentBaseWorldRadius);
        float scaleFactor = targetWorldRadius / currentBaseWorldRadius;
        transform.localScale = baseScale * scaleFactor;
    }

    private float GetActualWorldRadius()
    {
        if (areaCollider == null)
            return areaRadius;

        return areaCollider.radius * Mathf.Abs(transform.localScale.x);
    }
}