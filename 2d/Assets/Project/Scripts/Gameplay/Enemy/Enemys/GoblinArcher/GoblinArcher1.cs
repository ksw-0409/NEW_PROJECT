using UnityEngine;
using System.Collections;

public class GoblinArcher1 : EnemyAI
{
    [Header("궁수 설정")]
    public GameObject arrowPrefab;
    public float detectRange = 6.0f;     // 공격 인지 범위
    public float chargeTime = 1.5f;     // 차징 시간
    public float arrowSpeed = 20f;      // 화살 속도
    public float attackCoolDown = 3.0f; // 공격 쿨타임
    public float attackAfterDelay = 0.3f; //공격 후딜

    private Animator anim;
    private bool isActionRunning = false;
    private bool canAttack = true;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
    }
    public override void Init()
    {
        base.Init();
        isActionRunning = false;
        canAttack = true;
    }
    public override void OnUpdate(Vector2 playerPos)
    {
        base.OnUpdate(playerPos);   
        if (isDie || isActionRunning) return;

        // 성능을 위해 거리의 제곱으로 비교 
        float sqrDist = (playerPos - (Vector2)transform.position).sqrMagnitude;
        bool ismoving = rb.linearVelocity.magnitude > 0.1f;
        anim.SetBool("isWalking", ismoving);
        if (canAttack && sqrDist <= detectRange * detectRange)
        {
            // 공격 시퀀스 체이닝 시작
            StartCoroutine(AttackSequence(playerPos));
        }
    }
    public override void MoveTaget(Vector2 targetPos)
    {
        // 액션 중(차징/돌진/후딜)에는 이동 로직 중단
        if (isActionRunning) return;
        base.MoveTaget(targetPos);
    }

    // 메서드 체이닝: 전체 공격 흐름 제어
    private IEnumerator AttackSequence(Vector2 playerPos)
    {
        isActionRunning = true;
        canAttack = false;
        anim.SetBool("isWalking", false); // 공격 중엔 걷지 않음
        // 차징 (준비)
        yield return StartCoroutine(ChargePhase());

        // 발사 (데이터 주입 포함)
        yield return StartCoroutine(ShootPhase(playerPos));

        // 3단계: 후딜레이 및 상태 복구
        yield return StartCoroutine(PostAttackPhase());

        // 4단계: 쿨타임 (별도 루틴으로 실행하여 이동 가능하게 함)
        StartCoroutine(CoolDownPhase());
    }

    private IEnumerator ChargePhase()
    {
        rb.linearVelocity = Vector2.zero; // 차징 중 정지
        Debug.Log("궁수: 조준 중...");
        yield return new WaitForSeconds(chargeTime-0.35f);
        // 애니메이션 시작 트리거 발동
        anim.SetTrigger("doAttack");
        yield return new WaitForSeconds(0.35f);
    }

    private IEnumerator ShootPhase(Vector2 playerPos)
    {
        Vector2 currentTargetPos = EnemyManager.Instance.player.position;
        Vector2 dir = (currentTargetPos - (Vector2)transform.position).normalized;

        // 화살 생성
        GameObject arrowObj = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        Archer arrowScript = arrowObj.GetComponent<Archer>();
        // 생성과 동시에 데미지,  방향 주입
        arrowScript.Setup(SkillDamage, arrowSpeed, dir);
        Debug.Log("궁수: 발사!");
        yield return null;
    }

    private IEnumerator PostAttackPhase()
    {
        yield return new WaitForSeconds(attackAfterDelay);
        isActionRunning = false; // 이제 다시 MoveTaget이 작동 가능함
    }

    private IEnumerator CoolDownPhase()
    {
        yield return new WaitForSeconds(attackCoolDown);
        canAttack = true;
    }
}