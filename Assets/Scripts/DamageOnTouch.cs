using UnityEngine;

// Pasang di spike, kelelawar, uap pipa, dll. Perlu Collider2D (spike/uap: Is Trigger dicentang).
// Invincibility diurus PlayerHealth, jadi aman dipanggil berkali-kali.
public class DamageOnTouch : MonoBehaviour
{
    public float damage = 0.5f;   // 0.5 = setengah heart
    [Tooltip("Centang untuk melihat di Console apa yang tersentuh (matikan setelah selesai tes).")]
    public bool debugLog = false;

    // Enter DAN Stay: Stay saja bisa terlewat kalau Rigidbody player sedang "tidur" (diam)
    void OnTriggerEnter2D(Collider2D c) { Hit(c); }
    void OnTriggerStay2D(Collider2D c) { Hit(c); }
    void OnCollisionEnter2D(Collision2D c) { Hit(c.collider); }
    void OnCollisionStay2D(Collision2D c) { Hit(c.collider); }

    private void Hit(Collider2D c)
    {
        var h = c.GetComponentInParent<PlayerHealth>();
        if (debugLog) Debug.Log("DamageOnTouch '" + name + "' menyentuh '" + c.name + "' | PlayerHealth ketemu: " + (h != null), this);
        if (h != null) h.TakeDamage(damage, transform.position);
    }
}