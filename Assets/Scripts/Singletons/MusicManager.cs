using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    public static MusicManager Instance;

    public AudioClip NightLullaby;
    public AudioClip NightBaseMusic;
    public AudioClip MainMusic;
    public AudioClip DistortedMusic;
    public AudioClip CreditMusic;

    private AudioSource musicMain;
    private AudioSource musicStandBy;
    private DistortionController distortion;

    private Coroutine fadeInCoroutine;
    private Coroutine fadeOutCoroutine;

    public void PlayCreditMusic() {
        CrossFadeTo(CreditMusic);    
    }
    
    public void PlayNightMusic() {
        CrossFadeTo(NightBaseMusic);
    }
    
    public void PlayMainMusic() {
        CrossFadeTo(MainMusic);
    }

    public void StartDistortion() {
        CrossFadeTo(DistortedMusic, 2f, true);
    }

    public void StopDistortion() {
        CrossFadeTo(MainMusic, 2f, true);
    }
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogWarning(name + "destroyed");
            Destroy(gameObject);
        }
    }

    private void Start() {
        musicMain = gameObject.AddComponent<AudioSource>();
        musicMain.playOnAwake = false;
        musicStandBy = gameObject.AddComponent<AudioSource>();
        musicStandBy.playOnAwake = false;

        distortion = GetComponent<DistortionController>();
        distortion.Initialize();
        FadeIn(musicMain, NightLullaby);
    }

    private void CrossFadeTo(AudioClip audioClip, float duration = 2f, bool matchTime = false, float startTime = 0f) {
        AudioSource oldSource;
        AudioSource newSource;

        if (musicMain.isPlaying) {
            oldSource = musicMain;
            newSource = musicStandBy;
        } else {
            oldSource = musicStandBy;
            newSource = musicMain;
        }

        if (matchTime) {
            FadeIn(newSource, audioClip, duration, oldSource.time / oldSource.clip.length * audioClip.length);
        } else {
            FadeIn(newSource, audioClip, duration, startTime);
        }

        FadeOut(oldSource, duration);
    }

    private void FadeIn(AudioSource audioSource, AudioClip clip, float duration = 2f, float startTime = 0f, float targetVolume = 1f,
        bool loop = true) {
        if (fadeInCoroutine != null) {
            StopCoroutine(fadeInCoroutine);
        }

        fadeInCoroutine = StartCoroutine(FadeInCoroutine(audioSource, clip, duration, startTime, targetVolume, loop));
    }
    
    private void FadeOut(AudioSource audioSource, float duration = 2f) {
        if (audioSource.isPlaying) {
            if (fadeOutCoroutine != null) {
                StopCoroutine(fadeOutCoroutine);
            }

            fadeOutCoroutine = StartCoroutine(FadeOutCoroutine(audioSource, duration));
        }
    }

    private IEnumerator FadeInCoroutine(AudioSource audioSource, AudioClip clip, float duration, float time, float targetVolume, bool loop) {
        float startTime = Time.time;
        float stepTime = Time.deltaTime;
        float elapsedTime = 0f;

        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.loop = loop;
        audioSource.time = time;
        audioSource.Play();

        while (elapsedTime < duration) {
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTime / duration);
            yield return new WaitForSeconds(stepTime);
            elapsedTime = Time.time - startTime;
        }
    }

    private IEnumerator FadeOutCoroutine(AudioSource audioSource, float duration) {
        float startTime = Time.time;
        float stepTime = Time.deltaTime;
        float elapsedTime = 0f;
        float originalVolume = audioSource.volume;

        while (elapsedTime < duration) {
            audioSource.volume = Mathf.Lerp(originalVolume, 0f, elapsedTime / duration);
            yield return new WaitForSeconds(stepTime);
            elapsedTime = Time.time - startTime;
        }

        audioSource.Stop();
    }
}