using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip music;
    [Range(0f, 1f)] public float volume = 0.5f;

    static MusicPlayer current;
    AudioSource src;
    bool paused;
    Coroutine fade;

    void Awake()
    {
        if (current != null)
        {
            current.Apply(music, volume);
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        src = gameObject.AddComponent<AudioSource>();
        src.loop = true;
        src.spatialBlend = 0f;
        src.playOnAwake = false;
        Apply(music, volume);
    }

    // Dipanggil dari script lain (GameManager saat Game Over)
    public static void PauseMusic(float fadeTime = 0.6f)
    {
        if (current != null) current.DoPause(fadeTime);
    }

    void DoPause(float fadeTime)
    {
        if (paused || src == null || !src.isPlaying) return;
        paused = true;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeThenPause(fadeTime));
    }

    IEnumerator FadeThenPause(float fadeTime)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }
        src.Pause();
        fade = null;
    }

    void Apply(AudioClip clip, float vol)
    {
        if (fade != null) { StopCoroutine(fade); fade = null; }
        src.volume = vol;

        if (src.clip == clip)
        {
            if (paused) { paused = false; src.UnPause(); }
            else if (clip != null && !src.isPlaying) src.Play();
            return;
        }

        paused = false;
        src.clip = clip;
        if (clip == null) src.Stop();
        else src.Play();
    }
}