using System;
using System.Collections;
using UnityEngine;

// Taruh di Player. Semua hazard/enemy memanggil TakeDamage() atau Kill().
// HP dihitung dalam satuan HEART: maxHP = 3 -> 3 heart, damage 0.5 -> setengah heart.
public class PlayerHealth : MonoBehaviour
{
    public float maxHP = 3f;
    public float invulnTime = 1.5f;
    public float blinkInterval = 0.08f;
    public Vector2 knockbackForce = new Vector2(4f, 6f);

    public float CurrentHP { get; private set; }
    public event Action<float, float> OnHPChanged;   // (current, max) -> HUD

    private PlayerMovement move;
    private SpriteRenderer sprite;
    private float invulnUntil;
    private Coroutine blinkRoutine;

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        sprite = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
    }

    void Start() { OnHPChanged?.Invoke(CurrentHP, maxHP); }

    // Damage biasa dari trap / kelelawar / spike (mis. 0.5f). Bisa diblok invincibility.
    public void TakeDamage(float amount, Vector2 sourcePosition)
    {
        if (move.IsDead || Time.time < invulnUntil) return;

        CurrentHP = Mathf.Max(0f, CurrentHP - amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0f) { Die(); return; }

        float dir = transform.position.x >= sourcePosition.x ? 1f : -1f;
        move.Knockback(new Vector2(dir * knockbackForce.x, knockbackForce.y));
        StartInvuln(invulnTime);
    }

    // Instant fail (monster utama / jatuh): hilang 1 heart penuh, lalu respawn.
    // Kalau heart habis -> Game Over. Tidak bisa diblok invincibility.
    public void Kill()
    {
        if (move.IsDead) return;
        CurrentHP = Mathf.Max(0f, CurrentHP - 1f);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        Die();
    }

    private void Die()
    {
        StopBlink();
        sprite.enabled = true;
        move.Die();
        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        else SceneLoader.Reload();
    }

    // Dipanggil GameManager setelah respawn (HP TIDAK di-reset, cuma kebal sebentar)
    public void OnRespawn() { StartInvuln(1.5f); }

    private void StartInvuln(float duration)
    {
        invulnUntil = Time.time + duration;
        StopBlink();
        blinkRoutine = StartCoroutine(Blink());
    }

    private void StopBlink()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = null;
    }

    private IEnumerator Blink()
    {
        while (Time.time < invulnUntil)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        sprite.enabled = true;
        blinkRoutine = null;
    }
}