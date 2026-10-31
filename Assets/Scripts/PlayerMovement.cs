using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// Taruh di Player (sama object dengan Rigidbody2D, CapsuleCollider2D, SpriteRenderer, Animator).
// Animator TIDAK butuh parameter / transition: script langsung memutar state lewat nama.

public class PlayerMovement : MonoBehaviour
{
    [Header("Gerak")]
    [SerializeField] private float moveSpeed = 5f;
    [FormerlySerializedAs("jumpHeight")]
    [SerializeField] private float jumpSpeed = 10f;      // kecepatan awal lompat (bukan tinggi)
    [SerializeField] private LayerMask jumpableGround;   // WAJIB diisi (layer Ground)

    [Header("Jump feel")]
    [Tooltip("Masih boleh lompat sesaat setelah jalan melewati tepi platform.")]
    [SerializeField] private float coyoteTime = 0.1f;
    [Tooltip("Lepas tombol lompat lebih awal = lompatan lebih pendek.")]
    [SerializeField] private bool variableJump = false;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Fisika")]
    [SerializeField] private float groundCheckDistance = 0.08f;
    [Tooltip("Mencegah player 'nempel' di sisi platform saat menekan arah ke tembok.")]
    [SerializeField] private bool zeroFriction = true;

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

    private float moveInput;
    private int jumpCount;
    private float coyoteTimer;
    private bool grounded;
    private bool isDead;
    private float knockbackTimer;
    private Coroutine boostRoutine;

    // ---------- Animasi ----------
    // Nama state di Animator = nama enum ini (otomatis, tidak perlu daftar nama terpisah)
    private enum AnimState { idle, Jump, DoubleJump, Fall, Death }
    private static readonly string[] stateNames = System.Enum.GetNames(typeof(AnimState));
    private int[] stateHash;
    private bool[] stateExists;
    private AnimState currentAnim = (AnimState)(-1);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        coll = GetComponent<CapsuleCollider2D>();

        if (jumpableGround.value == 0)
            Debug.LogWarning("PlayerMovement: Jumpable Ground masih kosong! Player tidak akan pernah dianggap menapak tanah.", this);

        if (zeroFriction)
        {
            var mat = new PhysicsMaterial2D("PlayerNoFriction") { friction = 0f, bounciness = 0f };
            rb.sharedMaterial = mat;
            coll.sharedMaterial = mat;
        }

        SetupAnimator();
    }

    private void SetupAnimator()
    {
        stateHash = new int[stateNames.Length];
        stateExists = new bool[stateNames.Length];

        if (anim == null) { Debug.LogWarning("PlayerMovement: tidak ada komponen Animator di Player.", this); return; }
        if (anim.runtimeAnimatorController == null) { Debug.LogWarning("PlayerMovement: Animator belum diberi Controller.", this); return; }

        for (int i = 0; i < stateNames.Length; i++)
        {
            stateHash[i] = Animator.StringToHash(stateNames[i]);
            stateExists[i] = anim.HasState(0, stateHash[i]);
        }

        for (int i = 0; i < stateNames.Length; i++)
            if (!stateExists[i])
                Debug.LogWarning("PlayerMovement: state '" + stateNames[i] + "' tidak ada di Animator Controller (nama harus persis, huruf besar/kecil berpengaruh).", this);
    }

    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        grounded = CheckGrounded();

        // Reset lompatan saat menapak tanah; beri coyote time saat baru meninggalkan tanah
        if (grounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpCount = 0;
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            // Jatuh dari tepi tanpa lompat = lompatan pertama dianggap terpakai (sisa 1 double jump)
            if (coyoteTimer <= 0f && jumpCount == 0) jumpCount = 1;
        }

        if (Input.GetButtonDown("Jump") && jumpCount < 2)
        {
            jumpCount++;
            coyoteTimer = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed * JumpMultiplier);
        }

        if (variableJump && Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        FlipSprite();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;   // saat knockback, input horizontal diabaikan sebentar
            return;
        }
        rb.linearVelocity = new Vector2(moveInput * moveSpeed * SpeedMultiplier, rb.linearVelocity.y);
    }

    private void FlipSprite()
    {
        if (moveInput > 0) { sprite.flipX = false; Facing = 1; }
        else if (moveInput < 0) { sprite.flipX = true; Facing = -1; }
    }

    private void UpdateAnimation()
    {
        AnimState s;
        float vy = rb.linearVelocity.y;

        if (!grounded || vy > 0.1f)
        {
            if (vy > 0.1f) s = jumpCount >= 2 ? AnimState.DoubleJump : AnimState.Jump;
            else s = AnimState.Fall;
        }
        else s = AnimState.idle;

        SetAnim(s);
    }

    // Hanya memutar animasi kalau state BERUBAH (jadi clip tidak restart tiap frame)
    private void SetAnim(AnimState s)
    {
        if (s == currentAnim) return;
        currentAnim = s;

        if (anim == null || stateExists == null || !stateExists[(int)s]) return;
        anim.Play(stateHash[(int)s], 0, 0f);
    }

    private bool CheckGrounded()
    {
        Bounds b = coll.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - groundCheckDistance * 0.5f);
        Vector2 size = new Vector2(b.size.x * 0.9f, groundCheckDistance);
        return Physics2D.OverlapBox(center, size, 0f, jumpableGround) != null;
    }

    void OnDrawGizmosSelected()
    {
        var c = GetComponent<CapsuleCollider2D>();
        if (c == null) return;
        Bounds b = c.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(b.center.x, b.min.y - groundCheckDistance * 0.5f, 0f),
                            new Vector3(b.size.x * 0.9f, groundCheckDistance, 0f));
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
        moveInput = 0f;
        rb.linearVelocity = Vector2.zero;
        jumpCount = 0;
        SetAnim(AnimState.idle);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        SetAnim(AnimState.Death);
    }
}