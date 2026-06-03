using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager _instance;
    public static BGMManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<BGMManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("BGMManager");
                    _instance = go.AddComponent<BGMManager>();
                }
            }
            return _instance;
        }
    }

    // 1. 유니티 인스펙터에서 관리할 BGM 배열
    [SerializeField] private AudioClip[] bgmTracks;
    [SerializeField] private float defaultFadeDuration = 1.0f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // 2. 씬이 이동해도 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusic(int trackIndex, bool fade = true)
    {
        // 예외 처리: 배열 범위를 벗어나면 리턴
        if (bgmTracks == null || trackIndex < 0 || trackIndex >= bgmTracks.Length)
        {
            Debug.LogWarning($"[BGMManager] {trackIndex}번 트랙이 배열에 없습니다.");
            return;
        }

        AudioClip newTrack = bgmTracks[trackIndex];
        if (audioSource.clip == newTrack) return; // 이미 재생 중이면 무시

        if (fade)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTrackRoutine(newTrack));
        }
        else
        {
            audioSource.clip = newTrack;
            audioSource.volume = 1f;
            if (newTrack != null) audioSource.Play();
        }
    }

    private IEnumerator FadeTrackRoutine(AudioClip newTrack)
    {
        float timer = 0f;
        float startVolume = audioSource.volume;

        if (audioSource.isPlaying)
        {
            while (timer < defaultFadeDuration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / defaultFadeDuration);
                yield return null;
            }
        }

        audioSource.clip = newTrack;

        if (newTrack != null)
        {
            audioSource.Play();
            timer = 0f;
            while (timer < defaultFadeDuration)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0f, 1f, timer / defaultFadeDuration);
                yield return null;
            }
            audioSource.volume = 1f;
        }
        else
        {
            audioSource.Stop();
        }
    }
}