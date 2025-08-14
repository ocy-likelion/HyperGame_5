using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer mixer;                  // (옵션) Master/BGM/SFX 그룹 사용
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Music Sources")]
    public AudioSource bgmMain;               // loop ON
    public AudioSource bgmTimer;              // loop ON, 시작 볼륨 0
    [Range(0f, 1f)] public float bgmMainDefaultVol = 0.7f;
    [Range(0f, 1f)] public float bgmTimerDefaultVol = 0.7f;

    [Header("SFX Sources")]
    public AudioSource sfx;                   // 2D, PlayOneShot 용
    public AudioSource uiSfx;                 // 버튼 클릭 등 UI 전용

    [Header("Win/Lose SFX (듀킹)")]
    public AudioClip winClip;
    public AudioClip loseClip;
    public float duckTo = 0.25f;              // 듀킹시 BGM 비율
    public float duckAttack = 0.12f;
    public float duckRelease = 0.35f;

    [Header("Timer Layer Fade")]
    [Range(0f, 1f)] public float timerFadeStart = 0.35f; // 남은비율 ≤ 시작
    public float timerFadeTime = 0.5f;

    bool musicMuted = false, sfxMuted = false;
    float _bgmMainVol, _bgmTimerVol;
    bool _paused = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 라우팅 & 초기화
        foreach (var a in new[] { bgmMain, bgmTimer })
        {
            a.outputAudioMixerGroup = musicGroup; a.spatialBlend = 0f; a.loop = true;
        }
        foreach (var a in new[] { sfx, uiSfx })
        {
            a.outputAudioMixerGroup = sfxGroup; a.spatialBlend = 0f;
        }
        _bgmMainVol = bgmMainDefaultVol;
        _bgmTimerVol = 0f;                    // 타이머 레이어는 시작 0
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        bgmMain.volume = musicMuted ? 0f : _bgmMainVol;
        bgmTimer.volume = musicMuted ? 0f : _bgmTimerVol;
        sfx.mute = sfxMuted; uiSfx.mute = sfxMuted;
    }

    // ====== Public API ======
    public void PlayBgm(AudioClip clip, float fade = 0.3f)
    {
        if (bgmMain.clip == clip && bgmMain.isPlaying) return;
        StartCoroutine(FadeSwap(bgmMain, clip, bgmMainDefaultVol, fade));
    }

    public void SetTimerLayerClip(AudioClip clip) { bgmTimer.clip = clip; }
    public void StartTimerLayer() { bgmTimer.Play(); }
    public void StopTimerLayer(float fade = 0.25f) { StartCoroutine(Fade(bgmTimer, 0f, fade)); }

    // 남은 시간 비율(1→0)을 외부에서 계속 넘겨줘
    public void UpdateTimerNormalized(float norm)
    {
        if (!bgmTimer.clip) return;
        if (!bgmTimer.isPlaying) bgmTimer.Play();

        // pause 중엔 타이머 레이어는 안 들리게(요구사항)
        if (_paused) { if (bgmTimer.volume > 0f) bgmTimer.Pause(); return; }
        else if (bgmTimer.clip && !bgmTimer.isPlaying) bgmTimer.UnPause();

        float t = Mathf.InverseLerp(timerFadeStart, 0f, Mathf.Clamp01(norm)); // 0~1
        float target = Mathf.Lerp(0f, bgmTimerDefaultVol, t);
        _bgmTimerVol = Mathf.MoveTowards(_bgmTimerVol, target, Time.unscaledDeltaTime / Mathf.Max(0.01f, timerFadeTime));
        ApplyVolumes();
    }

    public void PlaySfx(AudioClip clip, float vol = 1f) { if (clip) sfx.PlayOneShot(clip, sfxMuted ? 0 : vol); }
    public void PlayUiClick(AudioClip clip, float vol = 1f) { if (clip) uiSfx.PlayOneShot(clip, sfxMuted ? 0 : vol); }

    public void PlayWin() { StartCoroutine(DuckAndPlay(winClip)); }
    public void PlayLose() { StartCoroutine(DuckAndPlay(loseClip)); }

    public void SetMusicMute(bool v) { musicMuted = v; ApplyVolumes(); }
    public void SetSfxMute(bool v) { sfxMuted = v; ApplyVolumes(); }
    public void MuteAll(bool v) { musicMuted = v; sfxMuted = v; ApplyVolumes(); }

    public void OnGamePaused(bool paused)
    {
        _paused = paused;
        // 타이머만 멈추기, 나머지는 그대로
        if (paused && bgmTimer.isPlaying) bgmTimer.Pause();
        if (!paused && bgmTimer.clip) bgmTimer.UnPause();
    }

    public void ResetAll()
    {                   // “처음으로 되돌리기”
        StopAllCoroutines();
        _bgmMainVol = bgmMainDefaultVol;
        _bgmTimerVol = 0f;
        musicMuted = sfxMuted = false;
        if (bgmMain.clip) { bgmMain.time = 0f; bgmMain.Play(); }
        bgmTimer.Stop(); bgmTimer.time = 0f;
        ApplyVolumes();
    }

    // ====== Helpers ======
    IEnumerator DuckAndPlay(AudioClip clip)
    {
        if (!clip) yield break;
        // Attack: BGM 볼륨 내리기 (unscaled)
        yield return FadeTwo(bgmMain, bgmTimer, duckTo, duckAttack);
        sfx.PlayOneShot(clip);
        // 클립 길이만큼 대기(타임스케일과 무관)
        float t = 0f; while (t < clip.length) { t += Time.unscaledDeltaTime; yield return null; }
        // Release: 원복
        yield return FadeTwo(bgmMain, bgmTimer, 1f, duckRelease, true);
    }

    IEnumerator Fade(AudioSource src, float target, float time)
    {
        float start = src.volume, t = 0f;
        while (t < time) { t += Time.unscaledDeltaTime; src.volume = Mathf.Lerp(start, target, t / time); yield return null; }
        src.volume = target;
    }
    IEnumerator FadeTwo(AudioSource a, AudioSource b, float factor, float time, bool restore = false)
    {
        float aStart = a.volume, bStart = b.volume;
        float aEnd = restore ? _bgmMainVol : _bgmMainVol * factor;
        float bEnd = restore ? _bgmTimerVol : _bgmTimerVol * factor;
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            a.volume = Mathf.Lerp(aStart, aEnd, t / time);
            b.volume = Mathf.Lerp(bStart, bEnd, t / time);
            yield return null;
        }
        a.volume = aEnd; b.volume = bEnd;
    }
    IEnumerator FadeSwap(AudioSource src, AudioClip next, float targetVol, float time)
    {
        if (src.isPlaying) yield return Fade(src, 0f, time);
        src.clip = next; src.Play();
        _bgmMainVol = targetVol;
        yield return Fade(src, musicMuted ? 0f : targetVol, time);
    }
    void Update() { ApplyVolumes(); } // 외부 변경 반영
}
