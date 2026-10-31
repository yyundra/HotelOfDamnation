using UnityEngine;

// HZ-Steam. Taruh di object dengan Collider2D (Is Trigger) = area semburan.
// Siklus total 3 detik: Inactive -> Warning -> Active. Deterministik (pakai Time.time), jadi bisa dipelajari player.
public class SteamPipe : MonoBehaviour
{
    public enum Phase { Inactive, Warning, Active }

    [Header("Timing (total = 3 detik sesuai LDD)")]
    public float cycleTotal = 3f;
    public float warningTime = 0.7f;
    public float activeTime = 1f;
    public float startOffset = 0f;   // geser siklus kalau ada banyak pipa

    [Header("Damage")]
    public int damage = 1;

    [Header("Visual / Audio")]
    public GameObject warningVisual;  // mis. partikel desis kecil
    public GameObject activeVisual;   // semburan penuh
    public AudioSource audioSource;
    public AudioClip hissClip;
    public AudioClip burstClip;

    public Phase CurrentPhase { get; private set; } = Phase.Inactive;

    private Collider2D hitbox;
    private float InactiveTime => Mathf.Max(0.1f, cycleTotal - warningTime - activeTime);

    void Awake() { hitbox = GetComponent<Collider2D>(); }

    void Start() { SetPhase(Phase.Inactive, true); }

    void Update()
    {
        float t = (Time.time + startOffset) % cycleTotal;
        Phase p = t < InactiveTime ? Phase.Inactive
                : t < InactiveTime + warningTime ? Phase.Warning
                : Phase.Active;
        if (p != CurrentPhase) SetPhase(p, false);
    }

    private void SetPhase(Phase p, bool force)
    {
        CurrentPhase = p;
        if (warningVisual) warningVisual.SetActive(p == Phase.Warning);
        if (activeVisual) activeVisual.SetActive(p == Phase.Active);
        hitbox.enabled = (p == Phase.Active);   // damage HANYA saat Active

        if (force || audioSource == null) return;
        if (p == Phase.Warning && hissClip) audioSource.PlayOneShot(hissClip);
        if (p == Phase.Active && burstClip) audioSource.PlayOneShot(burstClip);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (CurrentPhase != Phase.Active) return;
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage, transform.position);
    }
}
