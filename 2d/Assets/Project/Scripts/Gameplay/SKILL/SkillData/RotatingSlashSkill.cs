using UnityEngine;
using System.Collections;

public class RotatingSlashSkill : SkillBase
{
    public GameObject effectPrefab;
    private RotatingSlashData rotData;

    public void Init(RotatingSlashData data, SkillInstance instance)
    {
        this.data = data;
        this.instance = instance;
        this.rotData = data;
        StartCoroutine(AutoCast());
    }

    protected override void Execute(Transform player)
    {
        StartCoroutine(SlashRoutine());
    }

    IEnumerator SlashRoutine()
    {
        for (int i = 0; i < GetCount(); i++)
        {
            Attack();
            yield return new WaitForSeconds(rotData.hitInterval);
        }
    }

    void Attack()
    {
        // 레벨업된 현재 범위를 가져옴
        float range = instance.GetCurrentLevelData().range;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyHealth>()?.TakeDamage(GetDamage());
            }
        }

        // ⭐ 피격범위 가시화: 회전 베기는 360도 원형
        SkillRangeIndicator.Spawn(
            transform.position,
            range,
            new Color(0.3f, 0.85f, 1f, 0.95f),
            0.35f,
            SkillRangeIndicator.Shape.Circle
        );

        SpawnEffect(range);
    }

    void SpawnEffect(float range)
    {
        if (effectPrefab == null) return;
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
        const float spriteNative = 0.96f;
        const float activeRatio = 0.30f;
        // 이펙트 크기 = 데미지 판정 영역(지름 = range*2)과 일치
        float finalScale = (range * 2f) / spriteNative; // active ratio 제거
        effect.transform.localScale = new Vector3(finalScale, finalScale, 1f);
        Destroy(effect, 0.3f);
    }

    // 기즈모 색상을 검은색으로 변경
    // ⭐ 기즈모: 실제 피격판정(OverlapCircle range)과 완벽 일치
    void OnDrawGizmos()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, range);

        float angle = (rotData != null) ? rotData.angle : 360f;
        if (angle < 360f)
        {
            Vector3 forward = transform.right;
            int step = 20;
            for (int i = 0; i <= step; i++)
            {
                float currentAngle = -angle / 2 + (angle / step) * i;
                Vector3 dir = Quaternion.Euler(0, 0, currentAngle) * forward;
                Gizmos.DrawLine(transform.position, transform.position + dir * range);
            }
        }
    }
}