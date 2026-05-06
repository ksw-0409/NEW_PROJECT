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
    protected bool isDie = false;
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
        if (usePooling&& managedPool!=null)
        {
            managedPool.Release(this);
        }
        else
        {
            EnemyManager.Instance.RemoveActiveEnemy(this);
            Destroy(gameObject);
        }
    }
    public virtual void OnUpdate(Vector2 playerPos)
    {
        // 기본 로직 
    }
    public virtual void MoveTaget(Vector2 targetPos)
    {
        if (data == null) return;
        if (isRushMode) {
            rb.linearVelocity = rushDir * moveSpeed;
            if(transform.position.y< rushLimitY) managedPool.Release(this);
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

}
