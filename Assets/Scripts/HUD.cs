using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Heart (mendukung setengah heart) + persentase baterai.
// Setiap Image heart: Image Type = Filled, Fill Method = Horizontal, Fill Origin = Left.
public class HUD : MonoBehaviour
{
    public PlayerHealth health;
    public FlashlightSystem flashlight;

    [Header("Hearts (urut kiri ke kanan)")]
    public Image[] hearts;

    [Header("Baterai")]
    public TMP_Text batteryText;
    public Color lowColor = Color.red;
    public float lowThreshold = 0.2f;

    void Start()
    {
        // Otomatis cari kalau kosong (biar aman dipakai sebagai prefab)
        if (health == null) health = FindFirstObjectByType<PlayerHealth>();
        if (flashlight == null) flashlight = FindFirstObjectByType<FlashlightSystem>();

        if (health != null) { health.OnHPChanged += UpdateHearts; UpdateHearts(health.CurrentHP, health.maxHP); }
    }

    void OnDestroy() { if (health != null) health.OnHPChanged -= UpdateHearts; }

    void UpdateHearts(float current, float max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // heart ke-i terisi sebanyak (current - i), dibatasi 0..1  (2.5 -> 1, 1, 0.5)
            hearts[i].fillAmount = Mathf.Clamp01(current - i);
        }
    }

    void Update()
    {
        if (batteryText == null || flashlight == null) return;
        if (!flashlight.hasFlashlight) { batteryText.text = ""; return; }

        batteryText.text = Mathf.CeilToInt(flashlight.Percent * 100f) + "%";
        batteryText.color = flashlight.Percent <= lowThreshold ? lowColor : Color.white;
    }
}