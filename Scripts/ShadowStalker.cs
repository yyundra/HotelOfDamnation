using UnityEngine;

// EN01. Musuh statis di platform wajib. Collider2D = Is Trigger (hitbox sesuai silhouette sprite).
public class ShadowStalker : MonoBehaviour
{
    public int damage = 1;

    void OnTriggerStay2D(Collider2D other)
    {
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage, transform.position);
    }
}
