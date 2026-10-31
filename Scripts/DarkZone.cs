using UnityEngine;

// OPSIONAL (B-3): area gelap sebelum player punya senter. Collider2D (Is Trigger).
// Selama player di dalam & belum punya senter, layar menggelap sebagian (noFlashlightLevel).
public class DarkZone : MonoBehaviour
{
    private FlashlightSystem flash;

    void Start() { flash = FindFirstObjectByType<FlashlightSystem>(); }

    void OnTriggerEnter2D(Collider2D other) { if (flash != null && other.CompareTag("Player")) flash.InDarkZone = true; }
    void OnTriggerExit2D(Collider2D other) { if (flash != null && other.CompareTag("Player")) flash.InDarkZone = false; }
}
