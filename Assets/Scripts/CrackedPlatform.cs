using System.Collections;
using UnityEngine;

// Platform kayu lapuk. Cuma pakai 1 sprite (utuh), tanpa Animator.
// Stable -> diinjak -> Cracking (getar, sprite tetap utuh) -> Broken (jatuh & hilang) -> Respawn
public class CrackedPlatform : MonoBehaviour
{
    public enum State { Stable, Cracking, Broken }

    [Header("Sprite")]
    public SpriteRenderer spriteRenderer;   // kosong = cari otomatis

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

    public State CurrentState { get; private set; } = State.Stable;

    private Collider2D col;
    private Vector3 startPos;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null) { Debug.LogError("CrackedPlatform: SpriteRenderer tidak ditemukan di " + name, this); enabled = false; return; }
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
        // CRACKING: sprite TETAP utuh, hanya getar
        CurrentState = State.Cracking;
        Play(crackClip);
        if (debris) debris.Play();

        float t = 0f;
        while (t < breakDelay)
        {
            t += Time.deltaTime;
            spriteRenderer.transform.localPosition = startPos + (Vector3)(Random.insideUnitCircle * shakeAmount);
            yield return null;
        }
        spriteRenderer.transform.localPosition = startPos;

        // BROKEN: jatuh + fade (masih sprite utuh)
        CurrentState = State.Broken;
        col.enabled = false;
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
        SetAlpha(1f);
        spriteRenderer.enabled = true;
        col.enabled = true;
        CurrentState = State.Stable;
    }

    private void Play(AudioClip c) { if (audioSource != null && c != null) audioSource.PlayOneShot(c); }
    private void SetAlpha(float a) { var c = spriteRenderer.color; c.a = a; spriteRenderer.color = c; }
}