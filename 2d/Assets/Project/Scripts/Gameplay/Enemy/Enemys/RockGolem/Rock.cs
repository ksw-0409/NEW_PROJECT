using UnityEngine;

public class Rock : MonoBehaviour
{
    private float damage;
    private float speed;
    private Vector2 direction;
    private Vector2 startPos;
    private float maxRange = 15f; // 최대 사거리 15m
    private bool isInitialized = false;
    // 데이터 주입
    public void Setup(float dmg, float spd, Vector2 dir)
    {
        damage = dmg;
        speed = spd;
        startPos = transform.position;
        direction = dir.normalized; // 방향은 항상 정규화
        // 방향에 맞춰 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        if (Vector2.Distance(startPos, transform.position) >= maxRange)
        {
            DestroyRock();
        }
    }

    // 데미지 입히는 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 태그 확인
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.TakeDamage(damage);
            Debug.Log($"플레이어 적중! 데미지: {damage}");
            DestroyRock();
        }
    }

    private void DestroyRock()
    {
        Destroy(gameObject);
    }
}
