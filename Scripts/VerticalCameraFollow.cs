using UnityEngine;

// Script kamu + SnapToTarget() supaya kamera ikut turun saat respawn di checkpoint.
public class VerticalCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public float verticalOffset = 2f;
    public float smoothSpeed = 3f;

    [Header("Camera Position")]
    public bool followUpOnly = true;

    private float fixedX;
    private float velocityY;

    void Start() { fixedX = transform.position.x; }

    void LateUpdate()
    {
        if (player == null) return;

        float targetY = player.position.y + verticalOffset;
        float newY;

        if (followUpOnly)
        {
            if (targetY > transform.position.y)
                newY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, 1f / smoothSpeed);
            else { newY = transform.position.y; velocityY = 0f; }
        }
        else newY = Mathf.SmoothDamp(transform.position.y, targetY, ref velocityY, 1f / smoothSpeed);

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
