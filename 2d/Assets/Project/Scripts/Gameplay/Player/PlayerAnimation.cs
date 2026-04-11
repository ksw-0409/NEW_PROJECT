using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    // 문자열 오타 방지를 위해 미리 Hash로 변환 
    private readonly int hashMoving = Animator.StringToHash("isMoving");
    private readonly int hashDie = Animator.StringToHash("doDie");
    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // 이동 애니메이션 제어
        // 물리적인 속도가 0.1보다 크면 걷는 것으로 판단
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;
        anim.SetBool(hashMoving, isMoving);

        // 좌우 방향 전환 (Flip)
        // x축 속도가 양수면 오른쪽(1), 음수면 왼쪽(-1)
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            float direction = (rb.linearVelocity.x > 0 ? 1f : -1f)*GetComponent<Transform>().localScale.y;
            transform.localScale = new Vector3(direction, GetComponent<Transform>().localScale.y, GetComponent<Transform>().localScale.z);
        }
    }

    // 외부(이동/전투 스크립트)에서 호출할 죽음 함수
    public void PlayDie()
    {
        anim.SetTrigger(hashDie);
    }
}
