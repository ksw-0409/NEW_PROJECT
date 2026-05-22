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
    public bool usePooling = true; // �ν����Ϳ��� ����� üũ, �������� üũ ����

    //���� �̺�Ʈ��
    private bool isRushMode = false;
    private Vector2 rushDir;
    private float rushLimitY;

    //���ο� ����
    private float slowTimer = 0f;           // ���ο� ���ӽð� Ÿ�̸�
    private bool isSlowed = false;          // ���� ���ο� �������� üũ
    private float originalSpeed = 5f;     // ���� �⺻ �ӵ� �����

    //���� ����
    protected bool isStun = false; //���� 
    private float stunTimer = 0f;   // ���� ���ӽð� Ÿ�̸�

    protected bool isFlip=false;

    protected virtual void Awake()
    {
        rb =GetComponent<Rigidbody2D>();
        health=GetComponent<EnemyHealth>();
    }
   
    //ó�� �����Ҷ� pool ���� �޴������� ����� 
    public void SetPool(IObjectPool<EnemyAI> pool)=> managedPool = pool;
    public int GetID() { return data.id; }

    //Ǯ���� ������ �ʱ�ȭ �Լ� Manager���� ȣ��
    public virtual void Init()
    {
        isDie = false;
        rb.linearVelocity = Vector2.zero; // ������ ���� �ӵ� �ʱ�ȭ
        isRushMode = false;
        int floor = GameDataManager.Instance.CurrentFloor;
        // int Startfloor = data.startfloor;
        // �׽�Ʈ ���ؼ� ������ ��� 0���� ���� 
         int Startfloor = 0;
        health.init(
            (float)Mathf.RoundToInt(data.hp*(1.0f+(floor- Startfloor))*0.3f)
            ); //ü�� �ʱ�ȭ

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
    // ���� ��� ���� ����, �����ӵ�����/hp����/������������ Y��
    public void SetRushMode(Vector2 dir, float speed, float hpMultiplier, float limitY)
    {
        isRushMode = true;
        rushDir = dir.normalized;
        rushLimitY = limitY+this.transform.position.y;
        //�ӵ� hp ����
        moveSpeed = speed * dataP.moveSpeed;
        health.Multiple(hpMultiplier);
    }
    public virtual void Die()
    {
        if (isDie) return;
        isDie = true;

        // ✨ 적 처치 이펙트 — 보스면 elite 폭발
        bool isElite = gameObject.name.Contains("Boss") || gameObject.name.Contains("Elite");
        VFXManager.SpawnDeathBurst(transform.position, isElite);

        ExpManager.Instance.DropExp(this.transform.position, expAmount);
        ItemManager.Instance.DropItem(this.transform.position, DropWeapon, false);

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
            // Ÿ�ٰ� �ʹ� ������ ���� (��: 0.1 ���� �Ÿ�)
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
        // 0.1f�� �̼��� ���������� ���� �����Ÿ� ����
        if (horizontalDir < 0.1f) // ������ �̵�
        {
            isFlip = false;
            // ���� ũ�� ����
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalDir > -0.1f) // ���� �̵�
        {
            isFlip = true;
            // X���� ���̳ʽ���
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
                // �ð��� Ȯ���� ���� �α�
                Debug.Log($"{collision.gameObject.name}���� {ContactDamage}�� �������� �������ϴ�.");
            }
        }
    }


    // �ܺο��� ȣ���� ���ο� �Լ�   ( ��� , ���ӽð� ) ��ø ����� �ð��� ���� 
    public void ApplySlow(float slowMultiplier, float duration)
    {
        // �̹� ���ο� ���̶�� �ð��� �ʱ�ȭ(����)
        if (!isSlowed)
        {
            originalSpeed = moveSpeed; // ���ο� �� �ӵ� ����
            moveSpeed *= slowMultiplier; // �ӵ� ���� 
            isSlowed = true;
        }

        slowTimer = duration; // ���� �ð� ���� (��ø ȣ�� �� �ð� ����)
        Debug.Log("���ο� ����");
    }
    //�ܺο��� ȣ���� ���� �Լ� (���ӽð�) �ߺ��� �ð� ����
    public void ApplyStun(float duration)
    {
        isStun = true;
        stunTimer = duration; // ���� �ð� ���� (��ø ȣ�� �� �ð� ����)
        rb.linearVelocity = Vector2.zero; // ��� ����
        Debug.Log("���ο� ����");
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
        moveSpeed = originalSpeed; // ���� �ӵ��� ����
        slowTimer = 0f;
        Debug.Log("���ο� ����, �ӵ� ����");
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
        Debug.Log("���� ����, ���� ����");
    }
}
