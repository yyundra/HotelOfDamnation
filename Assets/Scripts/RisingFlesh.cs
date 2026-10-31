using UnityEngine;

public class RisingFlesh : MonoBehaviour
{
    public enum State { Far, Visible, Attack, Capture }

    [Header("Target")]
    public Transform player;

    [Header("Kecepatan")]
    public float riseSpeed = 0.15f;
    public float activateHeight = 4f;
    public float maxDistanceBelow = 5.5f;

    [Header("Titik Mematikan")]
    public bool autoSurface = true;
    public float surfaceInset = 0.3f;
    public float surfaceOffset = 0f;

    [Header("Jarak")]
    public float visibleDistance = 6f;
    public float attackDistance = 1.5f;

    [Header("Toggle")]
    public bool captureEnabled = true;
    public bool attackEnabled = false;
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;

    [Header("Audio (opsional)")]
    public AudioSource rumble;

    public State CurrentState { get; private set; } = State.Far;
    public bool Activated => activated;
    public float SurfaceY => transform.position.y + surfaceOffset;

    private PlayerHealth hp;
    private PlayerMovement move;
    private float startY;
    private bool activated;
    private float delayUntil;
    private float nextAttackAt;

    void Start()
    {
        if (autoSurface)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                surfaceOffset = sr.bounds.max.y - surfaceInset - transform.position.y;
                Debug.Log("RisingFlesh: Surface Offset otomatis = " + surfaceOffset, this);
            }
            else Debug.LogWarning("RisingFlesh: tidak ada SpriteRenderer, pakai Surface Offset manual.", this);
        }

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player != null)
        {
            hp = player.GetComponent<PlayerHealth>();
            move = player.GetComponent<PlayerMovement>();
            startY = player.position.y;
        }
    }

    void Update()
    {
        if (player == null || move.IsDead) return;

        if (!activated && player.position.y - startY >= activateHeight) activated = true;

        if (activated && Time.time >= delayUntil)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            float minSurface = player.position.y - maxDistanceBelow;
            if (SurfaceY < minSurface) transform.position += Vector3.up * (minSurface - SurfaceY);
        }

        float gap = player.position.y - SurfaceY;

        if (rumble != null) rumble.volume = Mathf.Clamp01(1f - gap / (visibleDistance * 2f));

        if (gap <= 0f && captureEnabled)
        {
            CurrentState = State.Capture;
            hp.Kill();
        }
        else if (gap < attackDistance && attackEnabled)
        {
            CurrentState = State.Attack;
            if (Time.time >= nextAttackAt)
            {
                nextAttackAt = Time.time + attackCooldown;
                hp.TakeDamage(attackDamage, new Vector2(player.position.x, SurfaceY));
            }
        }
        else CurrentState = gap < visibleDistance ? State.Visible : State.Far;
    }

    public void ResetTo(float surfaceY, float grace = 2f)
    {
        var p = transform.position;
        p.y = surfaceY - surfaceOffset;
        transform.position = p;
        delayUntil = Time.time + grace;
        nextAttackAt = Time.time + 1f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-20, SurfaceY, 0), new Vector3(20, SurfaceY, 0));
    }
}