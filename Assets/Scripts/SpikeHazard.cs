using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    public float damage = 0.5f;
    public bool debugLog = false;

    [Header("Audio")]
    public AudioClip hitSfx;
    [Range(0f, 1f)] public float hitVolume = 1f;

    Collider2D col;
    SpriteRenderer sr;
    float nextLog;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    Bounds GetArea()
    {
        if (col != null && col.enabled) return col.bounds;
        if (sr != null) return sr.bounds;
        return new Bounds(transform.position, Vector3.one);
    }

    void Update()
    {
        Bounds b = GetArea();
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f, Physics2D.AllLayers);

        if (debugLog && Time.time >= nextLog)
        {
            nextLog = Time.time + 1f;
            string names = "";
            foreach (var h in hits) names += h.name + ", ";
            Debug.Log("SpikeHazard '" + name + "' ukuran area " + b.size + " | menyentuh: " + names, this);
        }

        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                float before = health.CurrentHP;
                health.TakeDamage(damage, transform.position);
                if (health.CurrentHP < before) Sfx.Play2D(hitSfx, hitVolume);
                return;
            }
        }
    }

    void OnDrawGizmos()
    {
        Bounds b = GetArea();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}