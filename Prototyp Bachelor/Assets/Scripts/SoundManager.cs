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

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        instance = this;
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

        instance.audioSource.clip = parameterClip;
        instance.audioSource.volume = volume;
        instance.audioSource.loop = true;
        instance.audioSource.Play();
    }

    public static void Stop()
    {
        instance.audioSource.Stop();
        instance.audioSource.loop = false;
    }
    public static void FadeOutAndStop(float fadeDuration = 1f)
    {
        instance.StartCoroutine(instance.FadeOutCoroutine(fadeDuration));
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
        audioSource.volume = startVolume; // Lautstärke zurücksetzen für spätere Sounds
    }



#if UNITY_EDITOR
    public void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for(int i = 0; i < soundList.Length; i++)
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