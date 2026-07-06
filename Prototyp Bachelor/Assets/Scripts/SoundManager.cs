using UnityEngine;
using System;
using System.Collections;

public enum SoundType
{
    MUSIC,
    BACKGROUND,
    SOUNDS
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
}