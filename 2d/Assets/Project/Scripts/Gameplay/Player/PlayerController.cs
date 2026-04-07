using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SkillData startSkill;

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private PlayerStats stats;

    //쉴드 무적
    private bool isInvincible = false;
    [SerializeField] private float invincibleTime = 0.5f;

    private SpriteRenderer sprite;
    private ShieldSkill shield;
    [HideInInspector] public bool isMoving;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();

        sprite = GetComponent<SpriteRenderer>();
        shield = GetComponent<ShieldSkill>();
    }
    public SlashData slashData;
    public RotatingSlashData rotatingslashData;
    public FireballData fireballData;

    public GameObject slashEffectPrefab;
    public GameObject RotatingslashEffectPrefab;

    void Start()
    {

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMoving = moveInput.sqrMagnitude > 0;
    }
    public void TakeDamage(float dmg)
    {
        // ⭐ 방어막 먼저
        if (shield != null && shield.UseShield())
            return;

        if (isInvincible) return;

        Debug.Log("플레이어 피격: " + dmg);

        StartCoroutine(HitEffect());
    }
    IEnumerator HitEffect()
    {
        isInvincible = true;

        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * stats.CurrentMoveSpeed;
    }
}
