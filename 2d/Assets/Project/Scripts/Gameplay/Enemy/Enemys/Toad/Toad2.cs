using UnityEngine;

public class Toad2 : EnemyAI
{
    [Header("두꺼비 특화 설정")]
    public int a;
    public override void Init()
    {
        base.Init(); // 부모의 초기화
    }

    // 매니저의 FixedUpdate에서 매 프레임 호출됨
    public override void MoveTaget(Vector2 targetPos)
    {
        base.MoveTaget(targetPos);
    }

    public override void Die()
    {
        // 두꺼비 로직추가
        base.Die(); // 부모의 드랍 및 풀 반납 로직 실행
    }
}