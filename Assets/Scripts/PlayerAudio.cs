using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Jalan")]
    public AudioClip walkSfx;
    [Range(0f, 1f)] public float walkVolume = 0.6f;
    [Tooltip("Centang = klip Walk diputar berulang selama berjalan. Hapus centang = diputar satu kali tiap Step Interval.")]
    public bool walkAsLoop = true;
    public float stepInterval = 0.35f;

    [Header("Lompat dan Mendarat")]
    public AudioClip jumpSfx;
    [Range(0f, 1f)] public float jumpVolume = 0.7f;
    public AudioClip landSfx;
    [Range(0f, 1f)] public float landVolume = 0.7f;
    [Tooltip("Kecepatan jatuh minimal supaya bunyi mendarat. Naikkan kalau terlalu sering bunyi.")]
    public float landMinSpeed = 3f;

    [Header("Terluka")]
    public AudioClip hurtSfx;
    [Range(0f, 1f)] public float hurtVolume = 1f;
    [Tooltip("Lewati bagian hening di awal klip (detik). Isi kalau Ouch! telat bunyi.")]
    public float hurtStartTime = 0f;

    Rigidbody2D rb;
    PlayerMovement move;
    PlayerHealth health;
    AudioSource walkSource;
    float nextStep;
    float lastHP;
    float prevVy;
    bool jumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        move = GetComponent<PlayerMovement>();
        health = GetComponent<PlayerHealth>();

        if (walkSfx != null && walkAsLoop)
        {
            walkSource = gameObject.AddComponent<AudioSource>();
            walkSource.clip = walkSfx;
            walkSource.loop = true;
            walkSource.volume = walkVolume;
            walkSource.spatialBlend = 0f;
            walkSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (health != null)
        {
            lastHP = health.CurrentHP;
            health.OnHPChanged += OnHPChanged;
        }
    }

    void OnDestroy()
    {
        if (health != null) health.OnHPChanged -= OnHPChanged;
    }

    void OnHPChanged(float current, float max)
    {
        if (current < lastHP) Sfx.Play2D(hurtSfx, hurtVolume, hurtStartTime);
        lastHP = current;
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump")) jumpPressed = true;

        bool canMove = move != null && !move.IsDead && Time.timeScale > 0f;
        bool walking = canMove
            && Mathf.Abs(rb.linearVelocity.x) > 0.5f
            && Mathf.Abs(rb.linearVelocity.y) < 0.1f;

        if (walkSfx == null) return;

        if (walkAsLoop)
        {
            if (walkSource == null) return;
            if (walking && !walkSource.isPlaying) walkSource.Play();
            else if (!walking && walkSource.isPlaying) walkSource.Stop();
        }
        else if (walking && Time.time >= nextStep)
        {
            nextStep = Time.time + stepInterval;
            Sfx.Play2D(walkSfx, walkVolume);
        }
    }

    void LateUpdate()
    {
        float vy = rb.linearVelocity.y;
        bool alive = move != null && !move.IsDead && Time.timeScale > 0f;

        if (jumpPressed)
        {
            jumpPressed = false;
            // Lompat = kecepatan naik tiba-tiba melonjak
            if (alive && vy > 1f && vy > prevVy + 1f) Sfx.Play2D(jumpSfx, jumpVolume);
        }

        // Mendarat = tadi jatuh cepat, sekarang berhenti
        if (alive && prevVy < -landMinSpeed && vy > -0.5f) Sfx.Play2D(landSfx, landVolume);

        prevVy = vy;
    }
}