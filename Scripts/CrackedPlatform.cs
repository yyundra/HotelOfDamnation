using System.Collections;
using UnityEngine;

// Platform kayu lapuk, cukup 2 sprite (utuh + retak):
// Stable (sprite utuh) -> diinjak -> Cracking (sprite retak + getar) -> Broken (jatuh & hilang) -> Respawn (sprite utuh)
public class CrackedPlatform : MonoBehaviour
{
    public enum State { Stable, Cracking, Broken }

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;   // kosong = cari otomatis
    public Sprite intactSprite;             // kosong = pakai sprite awal
    public Sprite crackedSprite;

    [Header("Timing")]
    public float breakDelay = 1.5f;         // diinjak -> runtuh
    public float fallTime = 0.4f;
    public bool respawn = true;
    public float respawnDelay = 4f;

    [Header("Efek")]
    public float shakeAmount = 0.04f;
    public float fallDistance = 1.5f;
    public ParticleSystem debris;           // opsional
    public AudioSource audioSource;         // opsional
    public AudioClip crackClip;
    public AudioClip breakClip;

    [Header("Animator (opsional, kalau nanti sudah ada animasi: trigger Crack, Break, Reset)")]
    public Animator animator;

    public State CurrentState { get; private set; } = State.Stable;

    private Collider2D col;
    private Vector3 startPos;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (intactSprite == null && spriteRenderer != null) intactSprite = spriteRenderer.sprite;
        startPos = spriteRenderer.transform.localPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (CurrentState != State.Stable) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // hanya kalau player mendarat dari atas
        if (collision.collider.bounds.min.y < col.bounds.max.y - 0.15f) return;

        StartCoroutine(BreakRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        // CRACKING
        CurrentState = State.Cracking;
        if (crackedSprite != null) spriteRenderer.sprite = crackedSprite;
        Trigger("Crack");
        Play(crackClip);
        if (debris) debris.Play();

        float t = 0f;
        while (t < breakDelay)
        {
            t += Time.deltaTime;
            if (animator == null)
                spriteRenderer.transform.localPosition = startPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            yield return null;
        }
        spriteRenderer.transform.localPosition = startPos;

        // BROKEN
        CurrentState = State.Broken;
        col.enabled = false;
        Trigger("Break");
        Play(breakClip);
        if (debris) debris.Play();

        t = 0f;
        while (t < fallTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fallTime);
            spriteRenderer.transform.localPosition = startPos + Vector3.down * fallDistance * k * k;
            SetAlpha(1f - k);
            yield return null;
        }
        spriteRenderer.enabled = false;

        if (!respawn) yield break;

        // RESPAWN
        yield return new WaitForSeconds(respawnDelay);
        spriteRenderer.transform.localPosition = startPos;
        spriteRenderer.sprite = intactSprite;
        SetAlpha(1f);
        spriteRenderer.enabled = true;
        col.enabled = true;
        Trigger("Reset");
        CurrentState = State.Stable;
    }

    private void Trigger(string n) { if (animator != null) animator.SetTrigger(n); }
    private void Play(AudioClip c) { if (audioSource != null && c != null) audioSource.PlayOneShot(c); }
    private void SetAlpha(float a) { var c = spriteRenderer.color; c.a = a; spriteRenderer.color = c; }
}
