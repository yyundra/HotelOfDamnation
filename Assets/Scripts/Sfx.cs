using UnityEngine;

public static class Sfx
{
    public static void Play2D(AudioClip clip, float volume = 1f, float startTime = 0f)
    {
        if (clip == null) return;
        startTime = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.05f));

        var go = new GameObject("Sfx_" + clip.name);
        var a = go.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume;
        a.spatialBlend = 0f;
        a.time = startTime;
        a.Play();
        Object.Destroy(go, clip.length - startTime + 0.1f);
    }

    public static void PlayNear(AudioClip clip, Vector3 position, float volume = 1f, float hearDistance = 12f)
    {
        if (clip == null) return;

        float camDist = 10f;
        if (Camera.main != null)
            camDist = Mathf.Max(1f, Mathf.Abs(Camera.main.transform.position.z - position.z));

        var go = new GameObject("Sfx_" + clip.name);
        go.transform.position = position;
        var a = go.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume;
        a.spatialBlend = 1f;
        a.rolloffMode = AudioRolloffMode.Linear;
        a.minDistance = camDist;
        a.maxDistance = camDist + hearDistance;
        a.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }
}