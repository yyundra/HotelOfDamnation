using UnityEngine;

// Trigger di pintu EXIT tiap level. B-1: isi nextSceneName = "Victory".
public class LevelExit : MonoBehaviour
{
    [Tooltip("Kosong = scene berikutnya di Build Settings")]
    public string nextSceneName;
    public float delay = 0.5f;

    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;

        var flash = other.GetComponentInParent<FlashlightSystem>();
        if (flash != null) flash.SaveForNextLevel();   // senter & baterai dibawa ke level berikutnya

        Invoke(nameof(Go), delay);
    }

    void Go()
    {
        if (string.IsNullOrEmpty(nextSceneName)) SceneLoader.LoadNext();
        else SceneLoader.Load(nextSceneName);
    }
}
