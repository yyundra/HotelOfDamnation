using UnityEngine;

// Kamera vertikal. Player berada di TENGAH layar (verticalOffset = 0).
// Dead zone: selama player masih di dalam zona kecil di tengah, kamera diam,
// jadi lompatan tidak membuat layar goyang naik-turun.
public class VerticalCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public float verticalOffset = 0f;     // 0 = player persis di tengah layar
    public float deadZone = 1.2f;         // setengah tinggi zona diam (unit)
    public float smoothSpeed = 5f;

    [Header("Camera Position")]
    public bool followUpOnly = false;     // false = ikut naik DAN turun

    private float fixedX;
    private float velocityY;

    void Start() { fixedX = transform.position.x; }

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = player.position.y + verticalOffset;
        float diff = targetY - transform.position.y;

        // Kamera hanya bergerak kalau player keluar dari dead zone
        float desiredY = Mathf.Abs(diff) <= deadZone
            ? transform.position.y
            : targetY - Mathf.Sign(diff) * deadZone;

        if (followUpOnly && desiredY < transform.position.y)
        {
            velocityY = 0f;
            desiredY = transform.position.y;
        }

        float newY = Mathf.SmoothDamp(transform.position.y, desiredY, ref velocityY, 1f / smoothSpeed);
        transform.position = new Vector3(fixedX, newY, transform.position.z);
    }

    // Langsung loncat ke posisi player (dipakai saat respawn)
    public void SnapToTarget()
    {
        if (player == null) return;
        velocityY = 0f;
        transform.position = new Vector3(fixedX, player.position.y + verticalOffset, transform.position.z);
    }
}