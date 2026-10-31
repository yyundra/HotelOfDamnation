using UnityEngine;

// EN00 Chaser: ancaman yang naik dari bawah. State: Far (audio saja) / Visible / Attack / Capture.
// Attack = damage + knockback. Capture = instant fail (player berada di bawah permukaan).
// Taruh di sprite besar daging di bawah level. Permukaan atas = transform.y + surfaceOffset.
public class RisingFlesh : MonoBehaviour
{
    public enum State { Far, Visible, Attack, Capture }

    [Header("Target")]
    public Transform player;

    [Header("Kecepatan (tuning per floor)")]
    public float riseSpeed = 0.3f;          // B-3 ~0.1, B-2 ~0.3, B-1 ~0.6 (TBC, tes dulu)
    public float startDelay = 3f;
    public float respawnGap = 10f;          // jarak chaser di bawah checkpoint saat respawn
    public float maxDistanceBelow = 12f;    // rubber-band: tidak boleh terlalu jauh dari player
    [HideInInspector] public float RageMultiplier = 1f;   // diatur RageMode

    [Header("Jarak")]
    public float surfaceOffset = 0f;
    public float visibleDistance = 6f;
    public float attackDistance = 1.5f;

    [Header("Toggle (debug/QA)")]
    public bool captureEnabled = true;      // B-3: matikan supaya tidak ada unfair catch
    public bool attackEnabled = true;
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;

    [Header("Audio")]
    public AudioSource rumble;              // loop; volume naik saat dekat

    public State CurrentState { get; private set; } = State.Far;
    public float SurfaceY => transform.position.y + surfaceOffset;

    private PlayerHealth hp;
    private PlayerMovement move;
    private float nextAttackAt;
    private float delayUntil;

    void Start()
    {
        delayUntil = Time.time + startDelay;
        if (RunState.HasCheckpointHere)
        {
            var sp = transform.position;
            sp.y = RunState.CpPos.y - respawnGap - surfaceOffset;
            transform.position = sp;
        }
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player != null) { hp = player.GetComponent<PlayerHealth>(); move = player.GetComponent<PlayerMovement>(); }
    }

    void Update()
    {
        if (player == null || move.IsDead) return;

        if (Time.time >= delayUntil)
        {
            float step = riseSpeed * RageMultiplier * Time.deltaTime;
            transform.position += Vector3.up * step;

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

    // Dipanggil GameManager saat respawn di checkpoint
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
