using UnityEngine;

public class BatPatrol : MonoBehaviour
{
    [Header("Gerak")]
    public float speed = 2f;
    public float range = 3f;
    public float bobHeight = 0.3f;
    public float bobSpeed = 3f;
    public bool spriteFacesRight = true;

    [Header("Animasi (2 frame)")]
    public Sprite frameA;
    public Sprite frameB;
    public float frameInterval = 0.15f;

    [Header("Damage")]
    public float damage = 0.5f;

    [Header("Audio")]
    public AudioClip flySfx;
    [Range(0f, 1f)] public float flyVolume = 0.4f;
    public float hearDistance = 12f;
    public AudioClip attackSfx;
    [Range(0f, 1f)] public float attackVolume = 1f;

    Vector3 start;
    int direction = 1;
    float phase;
    float animTimer;
    bool showB;
    Collider2D col;
    SpriteRenderer sr;
    AudioSource flySource;

    void Start()
    {
        start = transform.position;
        phase = Random.value * 6.28f;
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        if (flySfx != null)
        {
            float camDist = 10f;
            if (Camera.main != null)
                camDist = Mathf.Max(1f, Mathf.Abs(Camera.main.transform.position.z - transform.position.z));

            flySource = gameObject.AddComponent<AudioSource>();
            flySource.clip = flySfx;
            flySource.loop = true;
            flySource.volume = flyVolume;
            flySource.spatialBlend = 1f;
            flySource.rolloffMode = AudioRolloffMode.Linear;
            flySource.minDistance = camDist;
            flySource.maxDistance = camDist + hearDistance;
            flySource.Play();
        }
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos.x += direction * speed * Time.deltaTime;

        if (pos.x > start.x + range)
        {
            pos.x = start.x + range;
            direction = -1;
        }
        else if (pos.x < start.x - range)
        {
            pos.x = start.x - range;
            direction = 1;
        }

        pos.y = start.y + Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight;
        transform.position = pos;

        Vector3 scale = transform.localScale;
        float face = spriteFacesRight ? direction : -direction;
        scale.x = Mathf.Abs(scale.x) * face;
        transform.localScale = scale;

        Animate();
        CheckHit();
    }

    void Animate()
    {
        if (sr == null || frameA == null || frameB == null) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameInterval)
        {
            animTimer = 0f;
            showB = !showB;
        }
        sr.sprite = showB ? frameB : frameA;
    }

    void CheckHit()
    {
        Bounds b;
        if (col != null && col.enabled) b = col.bounds;
        else if (sr != null) b = sr.bounds;
        else b = new Bounds(transform.position, Vector3.one * 0.5f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(b.center, b.size, 0f);
        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                float before = health.CurrentHP;
                health.TakeDamage(damage, transform.position);
                if (health.CurrentHP < before) Sfx.Play2D(attackSfx, attackVolume);
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 c = Application.isPlaying ? start : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(c + Vector3.left * range, c + Vector3.right * range);
    }
}