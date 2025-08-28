using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// LEGACY
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer mixer;                  // (�ɼ�) Master/BGM/SFX �׷� ���
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Music Sources")]
    public AudioSource bgmMain;               // loop ON
    public AudioSource bgmTimer;              // loop ON, ���� ���� 0
    [Range(0f, 1f)] public float bgmMainDefaultVol = 0.7f;
    [Range(0f, 1f)] public float bgmTimerDefaultVol = 0.7f;

    [Header("SFX Sources")]
    public AudioSource sfx;                   // 2D, PlayOneShot ��
    public AudioSource uiSfx;                 // ��ư Ŭ�� �� UI ����

    [Header("Win/Lose SFX (��ŷ)")]
    public AudioClip winClip;
    public AudioClip loseClip;
    public float duckTo = 0.25f;              // ��ŷ�� BGM ����
    public float duckAttack = 0.12f;
    public float duckRelease = 0.35f;

    [Header("Audio Clips")]
    public AudioClip bgmClip;
    public AudioClip[] sfxClip;
    public AudioClip[] uiClip;
    
    [Header("Timer Layer Fade")]
    [Range(0f, 1f)] public float timerFadeStart = 0.35f; // �������� �� ����
    public float timerFadeTime = 0.5f;

    bool musicMuted = false, sfxMuted = false;
    float _bgmMainVol, _bgmTimerVol;
    bool _paused = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ����� & �ʱ�ȭ
        foreach (var a in new[] { bgmMain, bgmTimer })
        {
            a.outputAudioMixerGroup = musicGroup; a.spatialBlend = 0f; a.loop = true;
        }
        foreach (var a in new[] { sfx, uiSfx })
        {
            a.outputAudioMixerGroup = sfxGroup; a.spatialBlend = 0f;
        }
        _bgmMainVol = bgmMainDefaultVol;
        _bgmTimerVol = 0f;                    // Ÿ�̸� ���̾�� ���� 0
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        bgmMain.volume = musicMuted ? 0f : _bgmMainVol;
        bgmTimer.volume = musicMuted ? 0f : _bgmTimerVol;
        sfx.mute = sfxMuted; uiSfx.mute = sfxMuted;
    }

    void Start()
    {
        bgmMain.clip = bgmClip; bgmMain.Play();
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

    // ���� �ð� ����(1��0)�� �ܺο��� ��� �Ѱ���
    public void UpdateTimerNormalized(float norm)
    {
        if (!bgmTimer.clip) return;
        if (!bgmTimer.isPlaying) bgmTimer.Play();

        // pause �߿� Ÿ�̸� ���̾�� �� �鸮��(�䱸����)
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
        // Ÿ�̸Ӹ� ���߱�, �������� �״��
        if (paused && bgmTimer.isPlaying) bgmTimer.Pause();
        if (!paused && bgmTimer.clip) bgmTimer.UnPause();
    }

    public void ResetAll()
    {                   // ��ó������ �ǵ����⡱
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
        // Attack: BGM ���� ������ (unscaled)
        yield return FadeTwo(bgmMain, bgmTimer, duckTo, duckAttack);
        sfx.PlayOneShot(clip);
        // Ŭ�� ���̸�ŭ ���(Ÿ�ӽ����ϰ� ����)
        float t = 0f; while (t < clip.length) { t += Time.unscaledDeltaTime; yield return null; }
        // Release: ����
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
    void Update() { ApplyVolumes(); } // �ܺ� ���� �ݿ�
}
