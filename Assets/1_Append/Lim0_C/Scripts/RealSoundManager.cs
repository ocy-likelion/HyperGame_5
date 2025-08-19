using System;
using Unity.VisualScripting;
using UnityEngine;

public class RealSoundManager : MonoBehaviour
{
    [SerializeField]private AudioSource bgmAudioSource;
    [SerializeField]private AudioSource sfxAudioSource;
    
    [SerializeField]private AudioClip bgmClip;
    [SerializeField]private AudioClip[] sfxClips;

    public static RealSoundManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = 0.6f;
        sfxAudioSource.loop = false;
        bgmAudioSource.Play();
    }

    public void PlayOneShot(Enums.SfxClips sfxClip)
    {
        sfxAudioSource.PlayOneShot(sfxClips[(int)sfxClip]);
    }

    public void OnClickButton()
    {
        sfxAudioSource.PlayOneShot(sfxClips[(int)Enums.SfxClips.Click]);
    }
}
