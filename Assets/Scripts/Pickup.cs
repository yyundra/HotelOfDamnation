using UnityEngine;

// 1 script untuk semua item. Collider2D = Is Trigger.
// Flashlight & Battery juga menjadi CHECKPOINT (setsCheckpoint).
public class Pickup : MonoBehaviour
{
    public enum Kind { Flashlight, Battery }

    public Kind kind;

    [Header("Battery")]
    public float batteryAmount = 30f;       // +30%

    [Header("Checkpoint")]
    public bool setsCheckpoint = true;
    [Tooltip("Opsional: titik respawn. Kosong = posisi pickup + offset.")]
    public Transform respawnPoint;
    public Vector3 respawnOffset = new Vector3(0f, 0.6f, 0f);

    public AudioClip pickupSfx;

    private bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || !other.CompareTag("Player")) return;

        var flash = other.GetComponentInParent<FlashlightSystem>();

        switch (kind)
        {
            case Kind.Flashlight: if (flash != null) flash.PickUp(); break;
            case Kind.Battery: if (flash != null) flash.AddBattery(batteryAmount); break;
        }

        // Checkpoint disimpan SETELAH baterai ditambah, jadi respawn membawa baterai terbaru
        if (setsCheckpoint && GameManager.Instance != null)
        {
            Vector3 pos = respawnPoint != null ? respawnPoint.position : transform.position + respawnOffset;
            GameManager.Instance.SetCheckpoint(pos);
        }

        used = true;
        if (pickupSfx != null) AudioSource.PlayClipAtPoint(pickupSfx, transform.position);
        Destroy(gameObject);
    }
}