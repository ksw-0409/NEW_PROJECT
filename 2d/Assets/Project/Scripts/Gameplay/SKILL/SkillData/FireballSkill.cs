using UnityEngine;
using UnityEngine.InputSystem;

public class FireballSkill : SkillBase
{
    private FireballData fireData;
    //public GameObject effectPrefab;
    public void Init(FireballData data, SkillInstance instance)
    {
        base.Init(data, instance);
        this.fireData = data;
    }

    protected override void Execute()
    {
        if (fireData == null)
        {
            Debug.LogError("fireData NULL");
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        for (int i = 0; i < GetCount(); i++)
        {
            Shoot(dir);
        }
    }

    void Shoot(Vector2 dir)
    {
        if (fireData == null)
        {
            Debug.LogError("fireData NULL");
            return;
        }

        if (fireData.projectilePrefab == null)
        {
            Debug.LogError("projectilePrefab NULL");
            return;
        }

        GameObject obj = Instantiate(
            fireData.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 없음");
            return;
        }

        rb.linearVelocity = dir * fireData.projectileSpeed;

        FireballProjectile proj = obj.GetComponent<FireballProjectile>();
        if (proj == null)
        {
            Debug.LogError("FireballProjectile 없음");
            return;
        }

        proj.Init(GetDamage(), fireData.explosionRadius, fireData.effectPrefab);
    }
}