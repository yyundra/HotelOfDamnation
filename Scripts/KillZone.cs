using UnityEngine;

// Jatuh keluar layar = mati. Collider2D (Is Trigger) lebar, taruh di bawah level.
// Kalau followTarget diisi (Main Camera), zona ikut kamera di bawah layar.
public class KillZone : MonoBehaviour
{
    public Transform followTarget;
    public float offsetBelow = 8f;

    void LateUpdate()
    {
        if (followTarget == null) return;
        var p = transform.position;
        p.y = followTarget.position.y - offsetBelow;
        transform.position = p;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.Kill();
    }
}
