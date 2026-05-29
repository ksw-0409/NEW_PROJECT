using UnityEngine;
using System.Collections;

public class ArrowRainArea : MonoBehaviour
{
    [Header("Settings")]
    public GameObject fallingArrowPrefab; // ���� �������� �ִ� ȭ�� ��ü
    public float areaRadius = 3f;         // ȭ���� �������� ����

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
        Destroy(gameObject, duration + 1f); // ��� ȭ���� ������ ������ ��� �� �ı�
    }

    IEnumerator RainRoutine()
    {
        float elapsed = 0;
        float spawnInterval = isConcentrated ? 0.08f : 0.25f;
        // ⭐ 전설 어빌리티 3 (페일노트) 장착 시: 화살비 밀도 2배 (간격 절반)
        if (PlayerStats.Instance != null && PlayerStats.Instance.HasAbility(3))
            spawnInterval *= 0.5f;

        float actualRadius = GetActualWorldRadius();

        while (elapsed < duration)
        {
            // 3. ���� ���� ������ �ȿ����� ���� ��ǥ ����
            Vector2 randomPos = (Vector2)transform.position + (Random.insideUnitCircle * actualRadius);

            Vector3 spawnPos = new Vector3(randomPos.x, randomPos.y + 10f, 0);

            GameObject arrow = Instantiate(fallingArrowPrefab, spawnPos, Quaternion.Euler(0, 0, -90));
            // ���� ���� ũ�⸦ ������ ����(0.5) Ȥ�� �� �۰�(0.3) ����
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