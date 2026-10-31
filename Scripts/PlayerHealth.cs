using System;
using System.Collections;
using UnityEngine;

// Taruh di Player. Semua hazard/enemy memanggil TakeDamage().
public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 3;
    public float invulnTime = 1f;
    public Vector2 knockbackForce = new Vector2(4f, 6f);

    public int CurrentHP { get; private set; }
    public event Action<int, int> OnHPChanged;   // (current, max) -> hubungkan ke UI HP

    private PlayerMovement move;
    private SpriteRenderer sprite;
    private float invulnUntil;

    void Awake()
    {
        move = GetComponent<PlayerMovement>();
        sprite = GetComponent<SpriteRenderer>();
        CurrentHP = maxHP;
    }

    void Start() { OnHPChanged?.Invoke(CurrentHP, maxHP); }

    public void TakeDamage(int amount, Vector2 sourcePosition)
    {
        if (move.IsDead || Time.time < invulnUntil) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        invulnUntil = Time.time + invulnTime;
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        float dir = transform.position.x >= sourcePosition.x ? 1f : -1f;
        move.Knockback(new Vector2(dir * knockbackForce.x, knockbackForce.y));
        StartCoroutine(Blink());

        if (CurrentHP <= 0) Kill();
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void ResetHealth()
    {
        CurrentHP = maxHP;
        invulnUntil = Time.time + 1f;   // kebal sebentar setelah respawn
        sprite.enabled = true;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    // Dipakai untuk instant fail (Capture, jatuh)
    public void Kill()
    {
        if (move.IsDead) return;
        move.Die();
        if (GameManager.Instance != null) GameManager.Instance.PlayerDied();
        else SceneLoader.Reload();
    }

    private IEnumerator Blink()
    {
        while (Time.time < invulnUntil)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        sprite.enabled = true;
    }
}
