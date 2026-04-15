using UnityEngine;

public class FireField : MonoBehaviour
{
    private float damagePerSecond; // 초당 데미지
    private float duration;        // 유지 시간
    private float timer = 0f;

    public void Setup(float duration, float damage)
    {
        this.duration = duration;
        this.damagePerSecond = damage;

        // 지정된 시간(duration) 후에 장판 자동 삭제
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 몬스터가 장판 위에 있을 때 실행
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth health = collision.GetComponent<EnemyHealth>();
            if (health != null)
            {
                // 프레임마다 데미지를 나눠서 줌 (1초에 총 damagePerSecond만큼 깎임)
                health.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }
}