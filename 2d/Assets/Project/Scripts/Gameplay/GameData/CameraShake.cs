using UnityEngine;

/// <summary>
/// 카메라 흔들림 매니저.
/// Main Camera에 붙여서 사용. 어디서든 CameraShake.Shake(...)로 호출 가능.
///
/// 사용 예시:
///   CameraShake.Shake(0.3f, 0.25f);                  // 표준 흔들림
///   CameraShake.Shake(0.5f, 0.4f, frequency: 25f);   // 강한 진동
///   CameraShake.ShakePreset(CameraShake.Preset.Heavy);// 프리셋 사용
/// </summary>
[DefaultExecutionOrder(100)] // CameraFollow보다 늦게 실행돼서 그 위에 흔들림을 얹음
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("디버그")]
    [Tooltip("현재 활성화된 흔들림의 남은 시간 (런타임 표시용)")]
    public float currentRemaining;

    [Tooltip("현재 흔들림의 강도 (런타임 표시용)")]
    public float currentMagnitude;

    // 활성 흔들림 파라미터
    private float duration;
    private float elapsed;
    private float magnitude;
    private float frequency;
    private float noiseSeedX;
    private float noiseSeedY;

    // 마지막에 계산한 흔들림 오프셋 (CameraFollow가 읽어감)
    private Vector3 currentOffset;

    public Vector3 CurrentOffset => currentOffset;

    public enum Preset
    {
        Light,   // 일반 타격
        Medium,  // 회전베기, 폭발 화살
        Heavy,   // 내려찍기, 얼음 폭발
        Epic     // 메테오, 폭발급
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void LateUpdate()
    {
        if (elapsed >= duration)
        {
            currentOffset = Vector3.zero;
            currentRemaining = 0f;
            currentMagnitude = 0f;
            return;
        }

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        // 감쇠 곡선: 시작에 최대, 끝에 0 (ease-out)
        float damper = 1f - t * t;

        // Perlin noise 기반 흔들림 (자연스러움)
        float time = Time.time * frequency;
        float nx = (Mathf.PerlinNoise(noiseSeedX, time) * 2f - 1f);
        float ny = (Mathf.PerlinNoise(noiseSeedY, time) * 2f - 1f);

        currentOffset = new Vector3(nx, ny, 0f) * (magnitude * damper);
        currentRemaining = duration - elapsed;
        currentMagnitude = magnitude * damper;
    }

    /// <summary>
    /// 카메라를 흔든다. 기존 흔들림과 비교하여 더 강한 쪽을 채택 (스택 안 함).
    /// </summary>
    public static void Shake(float magnitude, float duration, float frequency = 18f)
    {
        if (Instance == null) return;
        Instance.DoShake(magnitude, duration, frequency);
    }

    /// <summary>
    /// 프리셋 흔들림. 코드에서 일관된 강도로 쓰고 싶을 때 사용.
    /// </summary>
    public static void ShakePreset(Preset preset)
    {
        switch (preset)
        {
            case Preset.Light:  Shake(0.10f, 0.12f, 22f); break;
            case Preset.Medium: Shake(0.20f, 0.20f, 20f); break;
            case Preset.Heavy:  Shake(0.35f, 0.28f, 18f); break;
            case Preset.Epic:   Shake(0.55f, 0.45f, 16f); break;
        }
    }

    private void DoShake(float mag, float dur, float freq)
    {
        // 현재 진행 중인 흔들림보다 약하면 무시 (강한 흔들림 보존)
        float currentRemainingPower = (duration - elapsed) > 0 ? magnitude * (1f - elapsed / duration) : 0f;
        float newPower = mag;

        if (newPower < currentRemainingPower * 0.5f) return;

        magnitude = mag;
        duration = dur;
        frequency = freq;
        elapsed = 0f;
        // 새로운 시드로 매번 다른 패턴
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedY = Random.Range(0f, 1000f);
    }
}
