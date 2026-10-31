using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    static SceneFader instance;
    Image image;
    bool busy;
    bool fadeInNext = true;   // true saat game pertama kali dibuka

    const float FadeInTime = 1.0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        GameObject go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();
        instance.Build();
    }

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject imgObj = new GameObject("Fade");
        imgObj.transform.SetParent(transform, false);
        image = imgObj.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 1f);
        image.raycastTarget = true;

        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        busy = false;

        if (fadeInNext)
        {
            fadeInNext = false;
            StartCoroutine(Fade(1f, 0f, FadeInTime));
        }
        else
        {
            // Reload biasa (mati / retry): tanpa efek fade
            SetAlpha(0f);
            image.raycastTarget = false;
        }
    }

    // sceneName kosong/null = scene berikutnya di Build Settings
    public static void Go(string sceneName, float fadeOut = 0.6f)
    {
        if (instance == null) Boot();
        if (instance.busy) return;
        instance.StartCoroutine(instance.GoRoutine(sceneName, fadeOut));
    }

    IEnumerator GoRoutine(string sceneName, float fadeOut)
    {
        busy = true;
        fadeInNext = true;
        image.raycastTarget = true;
        yield return Fade(0f, 1f, fadeOut);

        if (string.IsNullOrEmpty(sceneName)) SceneLoader.LoadNext();
        else SceneLoader.Load(sceneName);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
        image.raycastTarget = to > 0.5f;
    }

    void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }
}