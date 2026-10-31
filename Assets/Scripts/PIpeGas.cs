using System.Collections;
using UnityEngine;

public class PipeGas : MonoBehaviour
{
    public GameObject gas;
    public float damage = 0.5f;
    public float startDelay = 0f;
    public float offTime = 2f;
    public float warningTime = 0.7f;
    public float onTime = 1.5f;
    public bool debugLog = false;

    [Header("Audio")]
    public AudioClip roarSfx;
    [Range(0f, 1f)] public float roarVolume = 1f;
    public float hearDistance = 12f;
    public bool roarAtWarning = false;

    SpriteRenderer sr;
    Collider2D col;
    bool dangerous;
    float onAlpha = 0.8f;

    IEnumerator Start()
    {
        sr = gas.GetComponent<SpriteRenderer>();
        col = gas.GetComponent<Collider2D>();
        if (sr != null && sr.color.a > 0.05f) onAlpha = sr.color.a;

        SetState(false, 0f);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            SetState(false, 0f);
            yield return new WaitForSeconds(offTime);

            SetState(false, onAlpha * 0.3f);
            if (roarAtWarning) PlayRoar();
            yield return new WaitForSeconds(warningTime);

            SetState(true, onAlpha);
            if (!roarAtWarning) PlayRoar();
            yield return new WaitForSeconds(onTime);
        }
    }

    void PlayRoar()
    {
        Sfx.PlayNear(roarSfx, gas.transform.position, roarVolume, hearDistance);
    }

    void SetState(bool isDangerous, float alpha)
    {
        dangerous = isDangerous;
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    Bounds GetArea()
    {
        if (col != null && col.enabled) return col.bounds;
        if (sr != null) return sr.bounds;
        return new Bounds(gas.transform.position, Vector3.one);
    }

    void Update()
    {
        if (!dangerous) return;

        Bounds b = GetArea();
        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f, Physics2D.AllLayers);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                if (debugLog) Debug.Log("PipeGas kena Player", this);
                health.TakeDamage(damage, gas.transform.position);
                return;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (gas == null) return;
        Bounds b = GetArea();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}