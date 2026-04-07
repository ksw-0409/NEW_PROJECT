using UnityEngine;
using System.Collections;

public class RotatingSlashSkill : SkillBase
{
    public GameObject effectPrefab;
    private RotatingSlashData rotData;

    void Awake()
    {
        //rotData = (RotatingSlashData)data;
    }
    public void Init(RotatingSlashData data, SkillInstance instance)
    {
        this.data = data;
        this.instance = instance;
        this.rotData = data; // ⭐ 여기서 넣어야 함

        StartCoroutine(AutoCast());
    }
    protected override void Execute()
    {
        float range = instance.GetCurrentLevelData().range; // ⭐ 여기
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
        float range = instance.GetCurrentLevelData().range; // ⭐ 이걸로 통일

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(GetDamage());
            }
        }

        SpawnEffect(range); // ⭐ 이것도 같이 넘김
    }

    void SpawnEffect(float range)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab);

        effect.transform.SetParent(transform);
        effect.transform.localPosition = Vector3.zero;

        // ⭐ 범위 기반 스케일
        float scale = range * 1.5f;
        effect.transform.localScale = new Vector3(scale, scale, 1);

        Destroy(effect, 0.2f);
    }

    void OnDrawGizmosSelected()
    {
        if (instance == null) return;

        float range = instance.GetCurrentLevelData().range;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        // ⭐ 방향 표시 (부채꼴)
        Gizmos.color = Color.yellow;

        float angle = rotData != null ? rotData.angle : 360f;
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