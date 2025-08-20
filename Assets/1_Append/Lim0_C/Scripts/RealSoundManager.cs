using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RealSoundManager : MonoBehaviour
{
    private AudioSource _bgmAudioSource;
    private AudioSource _sfxAudioSource;
    
    private bool _isMuted = false;
    
    [SerializeField]private AudioClip bgmClip;
    [SerializeField]private AudioClip[] sfxClips;

    public static RealSoundManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Init();
    }

    public void OnClickMute()
    {
        _isMuted = !_isMuted;
        UpdateMute();
    }

    void UpdateMute()
    {
        _bgmAudioSource.mute = _isMuted;
        _sfxAudioSource.mute = _isMuted;
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

    public void PlayOneShot(Enums.SfxClips sfxClip)
    {
        _sfxAudioSource.PlayOneShot(sfxClips[(int)sfxClip]);
    }

    public void OnClickButton()
    {
        _sfxAudioSource.PlayOneShot(sfxClips[(int)Enums.SfxClips.Click]);
    }
}
