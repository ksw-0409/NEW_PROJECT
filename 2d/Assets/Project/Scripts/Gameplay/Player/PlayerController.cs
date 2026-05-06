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
        // 방어막 흡수 (마나 반사 포함)
        if (shield != null && shield.OnHit(dmg))
            return;

        if (isInvincible) return;

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
        rb.linearVelocity = moveInput * stats.MoveSpeed;
    }
}
