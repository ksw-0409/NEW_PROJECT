using UnityEngine;

public class Slime1 : EnemyAI
{
    [Header("슬라임 특화 설정")]
    [SerializeField] private float jumpForce = 5f;    // 점프(돌진) 힘
    [SerializeField] private float jumpInterval = 2f; // 점프 간격
    private float timer = 0f;
    public override void Init()
    {
        base.Init(); // 부모의 초기화
        timer = 0;
    }

    // 매니저의 FixedUpdate에서 매 프레임 호출됨
    public override void MoveTaget(Vector2 targetPos)
    {
        if (isDie) return;
        //추가 로직 가능
        timer += Time.fixedDeltaTime;
        if (timer >= jumpInterval)
        {
            // 플레이어 방향으로 점프
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.AddForce(dir * jumpForce, ForceMode2D.Impulse);
            // 타이머 리셋
            timer = 0f;
        }
    }

    public override void Die()
    {
        // 슬라임 전용 이펙트 로직 추가 가능
        base.Die(); // 부모의 드랍 및 풀 반납 로직 실행
    }
}