using UnityEngine;

public class MeteorVisual : MonoBehaviour
{
    public GameObject fireFieldPrefab;
    private float duration;
    private float dotDamage;
    private float radius;

    public void Setup(float dur, float dmg, float rad)
    {
        duration = dur;
        dotDamage = dmg;
        radius = rad;

        // 💡 보험용: 2초 동안 아무데도 안 부딪히면 공중에서라도 터지게 함
        Invoke("Explode", 2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 💡 태그가 정의되어 있지 않아도 에러가 나지 않게 "Enemy" 체크만 우선 수행
        if (collision.CompareTag("Enemy"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // 중복 실행 방지
        CancelInvoke("Explode");

        if (fireFieldPrefab != null)
        {
            GameObject fieldGo = Instantiate(fireFieldPrefab, transform.position, Quaternion.identity);
            fieldGo.transform.localScale = Vector3.one * radius;

            FireField field = fieldGo.GetComponent<FireField>();
            if (field != null) field.Setup(duration, dotDamage);
        }

        Destroy(gameObject);
    }
}