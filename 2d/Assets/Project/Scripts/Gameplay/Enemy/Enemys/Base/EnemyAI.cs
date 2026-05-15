using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] protected EnemyData data;
    public PlayerData dataP;
    protected Rigidbody2D rb;
    protected EnemyHealth health; 
    private IObjectPool<EnemyAI> managedPool;
    public bool isDie = false;
    private float moveSpeed;
    protected float SkillDamage;
    private float ContactDamage;
    private float expAmount;
    private float DropWeapon;
    public bool usePooling = true; // 인스펙터에서 잡몹은 체크, 정예몹은 체크 해제

    //돌진 이벤트용
    private bool isRushMode = false;
    private Vector2 rushDir;
    private float rushLimitY;

    //슬로우 변수
    private float slowTimer = 0f;           // 슬로우 지속시간 타이머
    private bool isSlowed = false;          // 현재 슬로우 상태인지 체크
    private float originalSpeed = 5f;     // 원래 기본 속도 저장용

    //스턴 변수
    protected bool isStun = false; //스턴 
    private float stunTimer = 0f;   // 스턴 지속시간 타이머


    protected virtual void Awake()
    {
        rb =GetComponent<Rigidbody2D>();
        health=GetComponent<EnemyHealth>();
    }
   
    //처음 세팅할때 pool 참조 메니저에서 갖고옴 
    public void SetPool(IObjectPool<EnemyAI> pool)=> managedPool = pool;
    public int GetID() { return data.id; }

    //풀에서 꺼낼때 초기화 함수 Manager에서 호출
    public virtual void Init()
    {
        isDie = false;
        rb.linearVelocity = Vector2.zero; // 이전의 물리 속도 초기화
        isRushMode = false;
        int floor = GameDataManager.Instance.CurrentFloor;
        // int Startfloor = data.startfloor;
        // 테스트 위해서 시작층 모두 0으로 설정 
         int Startfloor = 0;
        health.init(
            (float)Mathf.RoundToInt(data.hp*(1.0f+(floor- Startfloor))*0.3f)
            ); //체력 초기화

        ContactDamage = (float)Mathf.RoundToInt(
            data.contactDamage * (1.0f+(floor- Startfloor) *0.15f)
            );

        SkillDamage = (float)Mathf.RoundToInt(
            data.skillDamage * (1.0f + (floor - Startfloor) * 0.15f)
            );


        expAmount = (float)Mathf.RoundToInt(data.DropExp * (1.0f + (Startfloor - 1.0f) * 0.4f));
        
        moveSpeed = data.moveSpeed * dataP.moveSpeed;
        DropWeapon = data.DropWeapon;
    }
    // 돌진 모드 셋팅 방향, 돌진속도배율/hp배율/어디까지갈건지 Y축
    public void SetRushMode(Vector2 dir, float speed, float hpMultiplier, float limitY)
    {
        isRushMode = true;
        rushDir = dir.normalized;
        rushLimitY = limitY+this.transform.position.y;
        //속도 hp 설정
        moveSpeed = speed * dataP.moveSpeed;
        health.Multiple(hpMultiplier);
    }
    public virtual void Die()
    {
        if (isDie) return;
        // 여기서 경험치 보석을 생성하거나 이펙트
        isDie = true;
        ExpManager.Instance.DropExp(this.transform.position, expAmount);        
        ItemManager.Instance.DropItem(this.transform.position, DropWeapon,false);

        EnemyManager.Instance.EnqueueToRelease(this);
    }
    public virtual void OnUpdate(Vector2 playerPos)
    {
        HandleSlowTimer();
        HandleStunTimer();
        if (isStun) return;
        if (isDie) return;
    }
    public virtual void MoveTaget(Vector2 targetPos)
    {
        if (data == null) return;
        if (isStun) return;
        if (isDie) return;
        if (isRushMode) {
            rb.linearVelocity = rushDir * moveSpeed;
            if (transform.position.y < rushLimitY) {
                isDie = true;
                EnemyManager.Instance.EnqueueToRelease(this); }
        }
        else
        {
            float distance = Vector2.Distance(transform.position, targetPos);
            // 타겟과 너무 가까우면 멈춤 (예: 0.1 유닛 거리)
            if (distance < 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
            HandleSpriteFlip(dir.x);
        }
        
    }

    protected void HandleSpriteFlip(float horizontalDir)
    {
        // 0.1f는 미세한 움직임으로 인한 덜덜거림 방지
        if (horizontalDir < 0.1f) // 오른쪽 이동
        {
            // 원래 크기 유지
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalDir > -0.1f) // 왼쪽 이동
        {
            // X값만 마이너스로
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(ContactDamage==0) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.TakeDamage(ContactDamage);
                // 시각적 확인을 위한 로그
                Debug.Log($"{collision.gameObject.name}에게 {ContactDamage}의 데미지를 입혔습니다.");
            }
        }
    }


    // 외부에서 호출할 슬로우 함수   ( 배수 , 지속시간 ) 중첩 실행시 시간만 갱신 
    public void ApplySlow(float slowMultiplier, float duration)
    {
        // 이미 슬로우 중이라면 시간만 초기화(갱신)
        if (!isSlowed)
        {
            originalSpeed = moveSpeed; // 슬로우 전 속도 저장
            moveSpeed *= slowMultiplier; // 속도 감소 
            isSlowed = true;
        }

        slowTimer = duration; // 지속 시간 설정 (중첩 호출 시 시간 갱신)
        Debug.Log("슬로우 적용");
    }
    //외부에서 호출할 스턴 함수 (지속시간) 중복시 시간 갱신
    public void ApplyStun(float duration)
    {
        isStun = true;
        stunTimer = duration; // 지속 시간 설정 (중첩 호출 시 시간 갱신)
        rb.linearVelocity = Vector2.zero; // 즉시 정지
        Debug.Log("슬로우 적용");
    }

    
    private void HandleSlowTimer()
    {
        if (isSlowed)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0)
            {
                StopSlow();
            }
        }
    }
    private void StopSlow()
    {
        isSlowed = false;
        moveSpeed = originalSpeed; // 원래 속도로 복구
        slowTimer = 0f;
        Debug.Log("슬로우 종료, 속도 복구");
    }
    private void HandleStunTimer()
    {
        if (isStun)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
            {
                StopStun();
            }
        }
    }
    private void StopStun()
    {
        isStun = false;
        stunTimer = 0f;
        Debug.Log("스턴 종료, 스턴 복구");
    }
}
