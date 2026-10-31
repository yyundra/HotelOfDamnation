using UnityEngine;

// TR-IDLE5: player diam > 5 detik -> tangan chaser naik dari bawah layar (tidak langsung bunuh).
// Jadikan "hand" CHILD dari Main Camera, posisi awal di luar layar bawah.
public class IdleThreat : MonoBehaviour
{
    public PlayerMovement player;
    public Transform hand;
    public float idleThreshold = 5f;
    public float hiddenLocalY = -7f;   // di luar layar
    public float shownLocalY = -4.5f;  // masuk layar bawah
    public float moveSpeed = 4f;
    public AudioSource audioSource;
    public AudioClip warningClip;

    private float idleTimer;
    private bool warned;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (player == null || hand == null) return;

        if (player.IsIdle) idleTimer += Time.deltaTime;
        else { idleTimer = 0f; warned = false; }

        bool show = idleTimer >= idleThreshold;
        if (show && !warned)
        {
            warned = true;
            if (audioSource && warningClip) audioSource.PlayOneShot(warningClip);
        }

        Vector3 p = hand.localPosition;
        p.y = Mathf.MoveTowards(p.y, show ? shownLocalY : hiddenLocalY, moveSpeed * Time.deltaTime);
        hand.localPosition = p;
    }
}
