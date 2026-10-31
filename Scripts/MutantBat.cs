using UnityEngine;

// EN02. Bat bertengger. Kalau player DIAM > idleTime di dalam zona (platform yang di-tag) -> Warning -> Dive -> Return.
// Taruh di object bat dengan Collider2D (Is Trigger) + Rigidbody2D (Kinematic).
public class MutantBat : MonoBehaviour
{
    private enum State { Perched, Warning, Dive, Return }

    [Header("Anti-idle zone (relatif posisi bertengger bat)")]
    public Vector2 zoneOffset = new Vector2(0f, -1.5f);
    public Vector2 zoneSize = new Vector2(4f, 2f);

    [Header("Timing")]
    public float idleTime = 3f;
    public float warningTime = 0.7f;
    public float diveSpeed = 12f;
    public float returnSpeed = 4f;
    public float cooldown = 2f;
    public int damage = 1;

    [Header("Visual")]
    public SpriteRenderer sprite;
    public AudioSource audioSource;
    public AudioClip warningClip;

    private State state = State.Perched;
    private Vector3 perchPos;
    private Vector3 targetPos;
    private float idleTimer, warnTimer, readyAt;
    private Transform player;
    private PlayerMovement playerMove;

    void Start()
    {
        perchPos = transform.position;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; playerMove = p.GetComponent<PlayerMovement>(); }
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (player == null || playerMove.IsDead) return;

        switch (state)
        {
            case State.Perched:
                bool inZone = ZoneBounds().Contains(player.position);
                if (inZone && playerMove.IsIdle && Time.time >= readyAt) idleTimer += Time.deltaTime;
                else idleTimer = 0f;

                if (idleTimer >= idleTime)
                {
                    state = State.Warning; warnTimer = 0f; idleTimer = 0f;
                    if (audioSource && warningClip) audioSource.PlayOneShot(warningClip);
                }
                break;

            case State.Warning:
                warnTimer += Time.deltaTime;
                if (sprite) sprite.color = (Mathf.FloorToInt(warnTimer * 12f) % 2 == 0) ? Color.red : Color.white; // telegraph
                if (warnTimer >= warningTime)
                {
                    targetPos = player.position;
                    if (sprite) sprite.color = Color.white;
                    state = State.Dive;
                }
                break;

            case State.Dive:
                Face(targetPos);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, diveSpeed * Time.deltaTime);
                if ((transform.position - targetPos).sqrMagnitude < 0.01f) state = State.Return;
                break;

            case State.Return:
                Face(perchPos);
                transform.position = Vector3.MoveTowards(transform.position, perchPos, returnSpeed * Time.deltaTime);
                if ((transform.position - perchPos).sqrMagnitude < 0.01f)
                {
                    state = State.Perched;
                    readyAt = Time.time + cooldown;
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (state != State.Dive) return;
        var hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) return;
        hp.TakeDamage(damage, transform.position);
        state = State.Return;
    }

    private void Face(Vector3 target)
    {
        if (sprite) sprite.flipX = target.x < transform.position.x;
    }

    private Bounds ZoneBounds()
    {
        Vector3 basePos = Application.isPlaying ? perchPos : transform.position;
        return new Bounds(basePos + (Vector3)zoneOffset, new Vector3(zoneSize.x, zoneSize.y, 100f));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Vector3 c = (Application.isPlaying ? perchPos : transform.position) + (Vector3)zoneOffset;
        Gizmos.DrawCube(c, new Vector3(zoneSize.x, zoneSize.y, 0.1f));
    }
}
