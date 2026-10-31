using System.Collections;
using UnityEngine;

// VERSI UPDATE dari PlayerController.cs kamu.
// PENTING: nama file HARUS sama dengan nama class (PlayerMovement.cs), kalau tidak Unity tidak bisa attach script-nya.
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private LayerMask jumpableGround;

    // --- API untuk script lain ---
    public float SpeedMultiplier { get; set; } = 1f;   // dipakai Adrenaline
    public float JumpMultiplier { get; set; } = 1f;
    public int Facing { get; private set; } = 1;       // 1 = kanan, -1 = kiri (dipakai senter)
    public bool IsDead => isDead;
    public bool IsIdle => !isDead && Mathf.Abs(rb.linearVelocity.x) < 0.1f && Mathf.Abs(rb.linearVelocity.y) < 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private CapsuleCollider2D coll;

    private float moveInput = 0;
    private int jumpCount = 0;
    private bool isDead = false;
    private float knockbackTimer = 0f;
    private bool hasStateParam = false;
    private Coroutine boostRoutine;

    private enum playerState { idle, jump, doubleJump, fall, death }
    private playerState state = playerState.idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();

        // Cek sekali: kalau Animator belum punya parameter "state", game tetap jalan tanpa spam error
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (var p in anim.parameters)
                if (p.name == "state") hasStateParam = true;
        }
    }

    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (knockbackTimer > 0f)
            knockbackTimer -= Time.deltaTime;   // saat knockback, input horizontal diabaikan sebentar
        else
            rb.linearVelocity = new Vector2(moveInput * moveSpeed * SpeedMultiplier, rb.linearVelocity.y);

        if (IsGrounded() && rb.linearVelocity.y <= 0.1f)
            jumpCount = 0;

        if (Input.GetButtonDown("Jump") && jumpCount < 2)
        {
            jumpCount++;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpHeight * JumpMultiplier);
        }

        FlipSprite();
        UpdateAnimation();
    }

    private void FlipSprite()
    {
        if (moveInput > 0) { sprite.flipX = false; Facing = 1; }
        else if (moveInput < 0) { sprite.flipX = true; Facing = -1; }
    }

    private void UpdateAnimation()
    {
        if (isDead) state = playerState.death;
        else if (!IsGrounded() || rb.linearVelocity.y > 0.1f)
        {
            if (rb.linearVelocity.y > 0.1f)
                state = jumpCount >= 2 ? playerState.doubleJump : playerState.jump;
            else
                state = playerState.fall;
        }
        else state = playerState.idle;

        if (hasStateParam) anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(
            coll.bounds.center,
            new Vector2(coll.bounds.size.x * 0.9f, coll.bounds.size.y),
            0f, Vector2.down, 0.15f, jumpableGround);
    }

    public void Knockback(Vector2 velocity, float duration = 0.25f)
    {
        if (isDead) return;
        rb.linearVelocity = velocity;
        knockbackTimer = duration;
    }

    public void Boost(float speedMult, float jumpMult, float duration)
    {
        if (boostRoutine != null) StopCoroutine(boostRoutine);
        boostRoutine = StartCoroutine(BoostRoutine(speedMult, jumpMult, duration));
    }

    private IEnumerator BoostRoutine(float speedMult, float jumpMult, float duration)
    {
        SpeedMultiplier = speedMult;
        JumpMultiplier = jumpMult;
        yield return new WaitForSeconds(duration);
        SpeedMultiplier = 1f;
        JumpMultiplier = 1f;
    }

    public void Revive()
    {
        isDead = false;
        knockbackTimer = 0f;
        rb.linearVelocity = Vector2.zero;
        jumpCount = 0;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        state = playerState.death;
        if (hasStateParam) anim.SetInteger("state", (int)state);
    }
}
