using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RealSoundManager : MonoBehaviour
{
    // private 필드(인스펙터 노출)
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip[] sfxClips;

    // private 필드
    private AudioSource _bgmAudioSource;
    private AudioSource _sfxAudioSource;
    private bool _isMuted = false;

    // 싱글턴
    public static RealSoundManager Instance { get; private set; }

    // 유니티 콜백
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    // 초기화
    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Init();
    }
    private void Init()
    {
        _bgmAudioSource = FindObjectsByType<AudioSource>(FindObjectsSortMode.None)[0];
        _sfxAudioSource = FindObjectsByType<AudioSource>(FindObjectsSortMode.None)[1];
        _bgmAudioSource.clip = bgmClip;
        _bgmAudioSource.loop = true;
        _bgmAudioSource.volume = 0.6f;
        _sfxAudioSource.loop = false;
        _bgmAudioSource.Play();
        UpdateMute();
    }

    // 재생
    public void PlayOneShot(Enums.SfxClips sfxClip)
    {
        _sfxAudioSource.PlayOneShot(sfxClips[(int)sfxClip]);
    }
    public void GameEndFade()
    {
        StartCoroutine(FadeAudio(_bgmAudioSource.volume, 0.3f, 0.5f));
    }
    private IEnumerator FadeAudio(float startVolume, float targetVolume, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            _bgmAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        _bgmAudioSource.volume = targetVolume;
    }
    public void OnClickButton()
    {
        _sfxAudioSource.PlayOneShot(sfxClips[(int)Enums.SfxClips.Click]);
    }

    // 뮤트
    private void UpdateMute()
    {
        _bgmAudioSource.mute = _isMuted;
        _sfxAudioSource.mute = _isMuted;
    }
    public void OnClickMute()
    {
        _isMuted = !_isMuted;
        UpdateMute();
    }
}
