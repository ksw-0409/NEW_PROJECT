using UnityEngine;

/// <summary>
/// 채무자의 낙인이 활성화된 동안 Player 주변에 보라색 디버프 오라를 표시.
/// 일반 재화가 음수일 때 자동으로 보이고, 0 이상이 되면 자동으로 숨김.
/// </summary>
public class PlayerDebtMarkVisual : MonoBehaviour
{
    private GameObject auraGo;
    private float pulseTimer = 0f;
    private LineRenderer line;

    void Update()
    {
        if (PassiveSystem.Instance == null) return;
        bool active = PassiveSystem.Instance.IsDebtMarkActive;

        if (active && auraGo == null)
        {
            auraGo = VFXManager.CreateDebtAura(transform);
            line = auraGo.GetComponent<LineRenderer>();
        }
        else if (!active && auraGo != null)
        {
            Destroy(auraGo);
            auraGo = null;
            line = null;
        }

        // 펄스 효과
        if (line != null)
        {
            pulseTimer += Time.deltaTime * 2.5f;
            float t = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
            Color c = Color.Lerp(
                new Color(0.45f, 0.12f, 0.75f, 0.6f),
                new Color(0.7f, 0.25f, 0.95f, 0.95f),
                t);
            line.startColor = c;
            line.endColor = c;
        }
    }
}
