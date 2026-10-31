using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    [Tooltip("Kosong = scene berikutnya di Build Settings")]
    public string nextSceneName;
    public float fadeOutTime = 1.2f;

    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;
        used = true;

        var flash = other.GetComponentInParent<FlashlightSystem>();
        if (flash != null) flash.SaveForNextLevel();   // senter & baterai dibawa

        // Player berhenti di pintu selama layar menggelap
        var move = other.GetComponentInParent<PlayerMovement>();
        if (move != null)
        {
            var rb = move.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            move.enabled = false;
        }

        SceneFader.Go(nextSceneName, fadeOutTime);
    }
}