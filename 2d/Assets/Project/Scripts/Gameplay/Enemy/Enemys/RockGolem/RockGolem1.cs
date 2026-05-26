using UnityEngine;
using System.Collections;

public class RockGolem1 : EnemyAI
{
    [Header("암석 골렘 특화 설정")]
    public float rockCount = 4f;
    public float DieDealy = 1.0f;
    public float rockSpeed = 10f;
    public GameObject rockPrefab;
    private bool RockDie = false;


    public Animator animator;

    public override void Init()
    {
        base.Init();
        RockDie = false;
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        if (RockDie) return;
        base.MoveTaget(targetPos);
    }
    public override void Die()
    {
        rb.linearVelocity = Vector2.zero;
        if (RockDie) return;
        StartCoroutine(DieRoutine());
    }
    private void SpawnDeathRocks()
    {
        float angleStep = 360f / rockCount;
        float randomAngle = Random.Range(0, 360f);
        for (int i = 0; i < rockCount; i++)
        {
            float targetAngle = i * angleStep + randomAngle;
            // 각도를 방향 벡터로 변환
            float radian = targetAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
            // 암석 생성
            GameObject rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);
            Rock rockScript = rock.GetComponent<Rock>();
            if (rockScript != null)
            {
                rockScript.Setup(SkillDamage, rockSpeed, dir);
            }
        }
    }

    private IEnumerator DieRoutine()
    {
        RockDie = true;
        yield return new WaitForSeconds(DieDealy);
        animator.SetTrigger("is_boom");
       /*
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        Color flashColor = Color.red; // 번쩍일 색상 (흰색 원하면 Color.white)

      
        float elapsed = 0f;
        float flashInterval = 0.1f; // 깜빡이는 속도 (낮을수록 빠름)

        // 1. DieDealy 시간 동안 반복해서 번쩍임
        while (elapsed < DieDealy)
        {
            // 색상 교체 (깜빡임)
            sprite.color = (sprite.color == originalColor) ? flashColor : originalColor;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }*/
        SpawnDeathRocks();
        base.Die();
    }
}