using UnityEngine;
using System.Collections;

public class EnemySlow : MonoBehaviour
{
    private EnemyAI ai;
    private EnemyData data;
    private Coroutine slowRoutine;

    void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    public void ApplySlow(float percent, float duration)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(percent, duration));
    }

    IEnumerator SlowRoutine(float percent, float duration)
    {
        if (data == null) yield break; // 안전장치: data 미할당 시 NRE 방지
        float originalSpeed = data.moveSpeed;

        data.moveSpeed *= (1f - percent);

        yield return new WaitForSeconds(duration);

        data.moveSpeed = originalSpeed;
    }
}