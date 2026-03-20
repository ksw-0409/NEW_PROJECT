using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyData data;
    private Rigidbody2D rb;

    void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
    }

    public virtual void MoveTaget(Vector2 targetPos)
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * data.moveSpeed;
    }
}
