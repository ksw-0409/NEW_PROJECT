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

        SpawnEffect(range);
    }

    void SpawnEffect(float range)
    {
        if (effectPrefab == null) return;
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);

        // [수정] 2.0f가 너무 크다면 0.5f~0.8f 정도로 낮추세요.
        // 이 값이 낮아질수록 이미지의 끝이 검은 원 안으로 들어옵니다.
        float multiplier = 0.2f;
        float finalScale = range * multiplier;

        effect.transform.localScale = new Vector3(finalScale, finalScale, 1);
        Destroy(effect, 0.2f);
    }

    // 기즈모 색상을 검은색으로 변경
    void OnDrawGizmos()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        // 전체 범위 원을 검은색으로 표시
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, range);

        // 부채꼴 가이드라인도 검은색으로 통일
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