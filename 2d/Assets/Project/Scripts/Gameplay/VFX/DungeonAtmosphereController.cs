using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 던전 층수에 따라 포스트프로세싱 효과를 보간해서 어두운 분위기를 만든다.
/// 1층 = 평범한 밝기, 10층 = 매우 어둡고 차가운 분위기.
/// 자동으로 Volume 컴포넌트와 프로파일을 생성하므로 GameObject에 이 스크립트만 붙이면 된다.
///
/// 적용되는 효과:
///   • ColorAdjustments: postExposure / saturation / contrast / colorFilter
///   • Vignette: intensity / color (가장자리 어둡게)
///   • Bloom: threshold / intensity (밝은 곳 빛나도록 — 대비 강화)
/// </summary>
[DefaultExecutionOrder(-50)]
public class DungeonAtmosphereController : MonoBehaviour
{
    [Header("층수별 보간 한계 (1층 → 10층)")]
    [Tooltip("post-exposure를 얼마나 어둡게 떨어뜨릴지 (EV stops). 10층 기준")]
    public float maxPostExposure = -0.7f; // ⭐ 어두움 약화
    [Tooltip("채도를 얼마나 떨어뜨릴지 (-100~100). 10층 기준")]
    public float maxSaturation = -18f;
    [Tooltip("대비를 얼마나 높일지 (-100~100). 10층 기준")]
    public float maxContrast = 20f;
    [Tooltip("10층 기준 색조 (푸르스름한 차가운 톤)")]
    public Color deepFloorTint = new Color(0.78f, 0.82f, 0.95f, 1f); // ⭐ 더 밝은 톤
    [Tooltip("1층 비네팅 강도")]
    public float minVignette = 0.15f;
    [Tooltip("10층 비네팅 강도")]
    public float maxVignette = 0.32f;
    [Tooltip("10층 비네팅 색조 (어두운 보라)")]
    public Color maxVignetteTint = new Color(0.06f, 0.02f, 0.12f, 1f);
    [Tooltip("1층 블룸 강도")]
    public float minBloom = 0.3f;
    [Tooltip("10층 블룸 강도 — 대비를 더 강조")]
    public float maxBloom = 0.8f;

    [Header("디버그")]
    [Tooltip("층 변경 시 콘솔에 로그 출력")]
    public bool logChanges = true;

    private Volume volume;
    private VolumeProfile profile;
    private ColorAdjustments colorAdjust;
    private Vignette vignette;
    private Bloom bloom;

    void Awake()
    {
        // Volume 컴포넌트 가져오거나 추가
        volume = GetComponent<Volume>();
        if (volume == null) volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0;
        volume.weight = 1f;

        // 프로파일을 동적으로 생성 (별도 자산 파일 불필요)
        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "DungeonAtmosphere_Runtime";
        volume.sharedProfile = profile;

        // 효과들 추가 + active=true
        colorAdjust = profile.Add<ColorAdjustments>(true);
        vignette = profile.Add<Vignette>(true);
        bloom = profile.Add<Bloom>(true);

        // 카메라의 PostProcessing 활성화 (Main Camera만)
        EnableCameraPostProcessing();
    }

    void OnEnable()
    {
        GameDataManager.OnFloorChanged += OnFloorChanged;
        // 첫 진입 시 현재 층 적용
        int floor = (GameDataManager.Instance != null) ? GameDataManager.Instance.CurrentFloor : 1;
        ApplyFloorEffects(floor);
    }

    void OnDisable()
    {
        GameDataManager.OnFloorChanged -= OnFloorChanged;
    }

    void OnFloorChanged(int floor)
    {
        ApplyFloorEffects(floor);
    }

    /// <summary>층수(1~10)에 따른 효과 보간 적용</summary>
    public void ApplyFloorEffects(int floor)
    {
        // t: 0 (1층) → 1 (10층)
        float t = Mathf.Clamp01((floor - 1) / 9f);

        // ColorAdjustments
        if (colorAdjust != null)
        {
            colorAdjust.postExposure.Override(Mathf.Lerp(0f, maxPostExposure, t));
            colorAdjust.saturation.Override(Mathf.Lerp(0f, maxSaturation, t));
            colorAdjust.contrast.Override(Mathf.Lerp(0f, maxContrast, t));
            colorAdjust.colorFilter.Override(Color.Lerp(Color.white, deepFloorTint, t));
        }

        // Vignette
        if (vignette != null)
        {
            vignette.intensity.Override(Mathf.Lerp(minVignette, maxVignette, t));
            vignette.smoothness.Override(0.6f);
            vignette.color.Override(Color.Lerp(Color.black, maxVignetteTint, t));
        }

        // Bloom
        if (bloom != null)
        {
            bloom.intensity.Override(Mathf.Lerp(minBloom, maxBloom, t));
            bloom.threshold.Override(Mathf.Lerp(1.2f, 0.7f, t));
        }

        if (logChanges)
        {
            Debug.Log($"<color=#aa88ff>[던전 분위기]</color> 층 {floor} (t={t:F2}) — exposure={Mathf.Lerp(0f, maxPostExposure, t):F2}, vignette={Mathf.Lerp(minVignette, maxVignette, t):F2}");
        }
    }

    /// <summary>Main Camera의 UniversalAdditionalCameraData.renderPostProcessing = true 활성화</summary>
    void EnableCameraPostProcessing()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            // 다른 카메라들도 시도
            foreach (var c in Camera.allCameras)
            {
                var data = c.GetComponent<UniversalAdditionalCameraData>();
                if (data != null) data.renderPostProcessing = true;
            }
            return;
        }

        var camData = cam.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null) camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;
    }
}
