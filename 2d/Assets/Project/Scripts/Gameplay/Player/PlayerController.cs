using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SkillData startSkill;

    private Vector2     moveInput;
    private Rigidbody2D rb;
    private PlayerStats stats;
    private SpriteRenderer sprite;
    private ShieldSkill shield;

    [HideInInspector] public bool isMoving;

    [SerializeField] private float invincibleTime = 0.5f;
    private bool isInvincible = false;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        stats  = GetComponent<PlayerStats>();
        sprite = GetComponent<SpriteRenderer>();
        shield = GetComponent<ShieldSkill>();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMoving  = moveInput.sqrMagnitude > 0;
    }

    /// <summary>외부(Enemy 등)에서 플레이어에게 피해를 줄 때 호출</summary>
    public void TakeDamage(float dmg)
    {
        // 1) ShieldSkill (검 마법 트리에서 활성화한 자동 발동 쉴드)
        if (shield != null && shield.OnHit(dmg))
        {
            StartCoroutine(HitEffect());
            return;
        }

        if (isInvincible) return;

        // 2) PlayerStats.TakeDamage 안에서 currentShield (방어 패시브) 자동 처리
        //    - currentShield > 0 이면 자동 흡수 후 return
        //    - 없으면 패시브 방어 % 감소 적용 후 HP 차감
        stats.TakeDamage(dmg);
        StartCoroutine(HitEffect());
    }

    IEnumerator HitEffect()
    {
        isInvincible  = true;
        sprite.color  = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color  = Color.white;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible  = false;
    }

    void FixedUpdate()
    {
        if (GameDataManager.Instance.isPachinkoActive)
        {
            // 1. 속도를 완전히 0으로 초기화
            rb.linearVelocity = Vector2.zero;
            return;
        }
        rb.linearVelocity = moveInput * stats.MoveSpeed;
    }
}
