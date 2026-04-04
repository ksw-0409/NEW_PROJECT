using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private PlayerStats stats;

    [HideInInspector] public bool isMoving;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
    }
    public SlashSkillData slashData;
    public GameObject slashEffectPrefab;

    void Start()
    {
        GetComponent<PlayerSkillController>()
            .AddSlashSkill(slashData, 6, slashEffectPrefab);
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
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * stats.MoveSpeed;
    }
}
