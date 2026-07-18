using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        if (backgroundMusic != null)
            musicSource.clip = backgroundMusic;
    }

    private void Start()
    {
        if (musicSource.clip != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
    }

    public float GetVolume()
    {
        return musicSource.volume;
    }

    public void SetMuted(bool muted)
    {
        musicSource.mute = muted;
    }

    public void Mute()
    {
        SetMuted(true);
    }

    public void Unmute()
    {
        SetMuted(false);
    }

    // For future UI transitions; it does not change normal playback behaviour.
    public void FadeTo(float targetVolume, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(Mathf.Clamp01(targetVolume), Mathf.Max(0f, duration)));
    }

    private IEnumerator FadeRoutine(float targetVolume, float duration)
    {
        float initialVolume = musicSource.volume;

        if (duration <= 0f)
        {
            musicSource.volume = targetVolume;
            fadeCoroutine = null;
            yield break;
        }

        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(initialVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        fadeCoroutine = null;
    }
}
