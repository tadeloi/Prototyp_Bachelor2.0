using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum SoundType
{
    MUSIC,
    BACKGROUND,
    SOUNDS,
    WALKING,
    SPRINTING,
    MENU
}

public enum MenuSoundType
{
    MAINMENU,
    STARTGAME,
}
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;
    //private AudioSource musicAudioSource;

    public AudioSource AudioSourceTemplate;
    [SerializeField] private AudioSource[] audioSources;

    [Header("Footsteps (Walking / Sprinting)")]
    [Tooltip("Zwei dedizierte AudioSources, zwischen denen beim Wechsel Walking/Sprinting sanft übergeblendet wird. Bitte im Inspector zuweisen.")]
    [SerializeField] private AudioSource footstepSourceA;
    [SerializeField] private AudioSource footstepSourceB;

    private AudioSource activeFootstepSource;
    private SoundType? currentFootstepType = null;
    private readonly Dictionary<AudioSource, Coroutine> footstepFadeRoutines = new Dictionary<AudioSource, Coroutine>();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Should not have more than 1 AudioManager.");
    }

    public static void PlaySound(Categories parameter, SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip parameterClip = clips[(int)parameter];
        instance.audioSource.PlayOneShot(parameterClip, volume);
    }

    public static void PlayMenuSound(MenuSoundType soundType, float volume = 1, bool loop = false)
    {
        AudioClip[] clips = instance.soundList[(int)SoundType.MENU].Sounds;
        AudioClip parameterClip;
        switch (soundType)
        {
            case MenuSoundType.MAINMENU:
                parameterClip = clips[0];
                break;
            case MenuSoundType.STARTGAME:
                parameterClip = clips[1];
                break;
            default:
                Debug.LogWarning("Unknown menu sound type: " + soundType);
                return;
        }

        AudioSource menuAudioSource = null;

        foreach (AudioSource source in instance.audioSources)
        {
            if (!source.isPlaying)
            {
                menuAudioSource = source;
                break;
            }
        }
        if (menuAudioSource == null)
            menuAudioSource = instance.audioSources[0]; // Fallback: Verwende die erste AudioSource, wenn alle belegt sind

        menuAudioSource.clip = parameterClip;
        menuAudioSource.volume = volume;
        menuAudioSource.loop = loop;
        menuAudioSource.Play();
    }

    public static void PlayVFXSound(int position, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)SoundType.SOUNDS].Sounds;
        AudioClip parameterClip = clips[position];
        instance.audioSource.PlayOneShot(parameterClip, volume);
    }

    public static void PlaySoundLooped(Categories parameter, SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip parameterClip = clips[(int)parameter];
        AudioSource musicAudioSource = null;

        foreach (AudioSource source in instance.audioSources)
        {
            if (!source.isPlaying)
            {
                musicAudioSource = source;
                break;
            }
        }

        if (musicAudioSource == null)
            return;

        musicAudioSource.clip = parameterClip;
        musicAudioSource.volume = volume;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public static void Stop()
    {
        instance.audioSource.Stop();
        instance.audioSource.loop = false;
    }

    public static void StopAllAudioSources()
    {
        foreach (AudioSource source in instance.audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
                source.loop = false;
            }
        }
    }
    public static void FadeOutAndStop(float fadeDuration = 1f)
    {
        foreach (AudioSource source in instance.audioSources)
        {
            if (source != null)
            {
                Debug.Log("Trying to stop all running music");
                Debug.Log("Currently at sound: " + source.ToString());
                if (source.isPlaying)
                {
                    instance.StartCoroutine(instance.FadeOutCoroutine(source, fadeDuration));
                }
            }

        }
    }

    // ---------------------------------------------------------------
    // Footstep sound handling (Walking / Sprinting) mit Crossfade
    // ---------------------------------------------------------------

    /// <summary>
    /// Spielt den Walking- oder Sprinting-Sound für die übergebene Category.
    /// Läuft bereits der passende Sound, passiert nichts. Läuft ein anderer
    /// Footstep-Sound, wird sanft zum neuen übergeblendet.
    /// </summary>
    public static void PlayFootstepSound(Categories category, SoundType soundType, float volume = 1f, float fadeDuration = 0.25f)
    {
        instance.PlayFootstepSoundInternal(category, soundType, volume, fadeDuration);
    }

    /// <summary>
    /// Blendet den aktuell laufenden Footstep-Sound aus und stoppt ihn danach.
    /// </summary>
    public static void StopFootstepSound(float fadeDuration = 0.25f)
    {
        instance.StopFootstepSoundInternal(fadeDuration);
    }

    private void PlayFootstepSoundInternal(Categories category, SoundType soundType, float volume, float fadeDuration)
    {
        // Bereits der gewünschte Sound aktiv? Dann nichts tun.
        if (currentFootstepType == soundType && activeFootstepSource != null && activeFootstepSource.isPlaying)
            return;

        AudioClip[] clips = soundList[(int)soundType].Sounds;
        AudioClip clip = clips[(int)category];

        AudioSource oldSource = activeFootstepSource;
        AudioSource newSource = (activeFootstepSource == footstepSourceA) ? footstepSourceB : footstepSourceA;

        newSource.clip = clip;
        newSource.loop = true;
        newSource.volume = 0f;
        newSource.Play();

        StartFootstepFade(newSource, volume, fadeDuration);

        if (oldSource != null && oldSource.isPlaying)
            StartFootstepFade(oldSource, 0f, fadeDuration);

        activeFootstepSource = newSource;
        currentFootstepType = soundType;
    }

    private void StopFootstepSoundInternal(float fadeDuration)
    {
        if (activeFootstepSource != null)
            StartFootstepFade(activeFootstepSource, 0f, fadeDuration);

        activeFootstepSource = null;
        currentFootstepType = null;
    }

    private void StartFootstepFade(AudioSource source, float targetVolume, float duration)
    {
        if (footstepFadeRoutines.TryGetValue(source, out Coroutine running) && running != null)
            StopCoroutine(running);

        footstepFadeRoutines[source] = StartCoroutine(FadeFootstepVolumeCoroutine(source, targetVolume, duration));
    }

    private IEnumerator FadeFootstepVolumeCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, duration > 0f ? elapsed / duration : 1f);
            yield return null;
        }

        source.volume = targetVolume;

        if (targetVolume <= 0f)
            source.Stop();
    }

    // ---------------------------------------------------------------

    private IEnumerator FadeOutCoroutine(float fadeDuration = 1f)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.volume = startVolume; // Lautstärke zurücksetzen für spätere Sounds
    }

    private IEnumerator FadeOutCoroutine(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        source.Stop();
        source.loop = false;
        source.volume = startVolume; // Lautstärke zurücksetzen für spätere Sounds
    }



#if UNITY_EDITOR
    public void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}
[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}

/* Saving old Code
using UnityEngine;
using System;
using System.Collections;

public enum SoundType
{
    MUSIC,
    BACKGROUND,
    SOUNDS,
    WALKING,
    SPRINTING
}
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;
    //private AudioSource musicAudioSource;

    public AudioSource AudioSourceTemplate;
    [SerializeField] private AudioSource[] audioSources;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Should not have more than 1 AudioManager.");
    }

    public static void PlaySound(Categories parameter, SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip parameterClip = clips[(int)parameter];
        instance.audioSource.PlayOneShot(parameterClip, volume);
    }

    public static void PlayVFXSound(int position, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)SoundType.SOUNDS].Sounds;
        AudioClip parameterClip = clips[position];
        instance.audioSource.PlayOneShot(parameterClip, volume);
    }

    public static void PlaySoundLooped(Categories parameter, SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip parameterClip = clips[(int)parameter];
        AudioSource musicAudioSource = null;

        foreach (AudioSource source in instance.audioSources)
        {
            if (!source.isPlaying)
            {
                musicAudioSource = source;
                break;
            }
        }

        if (musicAudioSource == null)
            return;

        musicAudioSource.clip = parameterClip;
        musicAudioSource.volume = volume;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public static void Stop()
    {
        instance.audioSource.Stop();
        instance.audioSource.loop = false;
    }
    public static void FadeOutAndStop(float fadeDuration = 1f)
    {
        foreach (AudioSource source in instance.audioSources)
        {
            if (source != null)
            {
                Debug.Log("Trying to stop all running music");
                Debug.Log("Currently at sound: " + source.ToString());
                if (source.isPlaying)
                {
                    instance.StartCoroutine(instance.FadeOutCoroutine(source, fadeDuration));
                }
            }

        }
    }

    private IEnumerator FadeOutCoroutine(float fadeDuration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.volume = startVolume; // Lautst�rke zur�cksetzen f�r sp�tere Sounds
    }

    private IEnumerator FadeOutCoroutine(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        source.Stop();
        source.loop = false;
        source.volume = startVolume; // Lautst�rke zur�cksetzen f�r sp�tere Sounds
    }



#if UNITY_EDITOR
    public void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}
[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}*/