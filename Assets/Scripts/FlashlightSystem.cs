using UnityEngine;

// Taruh di Player.
// Konsep: baterai terus berkurang. Makin sedikit baterai, layar makin gelap (diatur DarknessOverlay).
// Ambil battery = terang lagi.
public class FlashlightSystem : MonoBehaviour
{
    [Header("State")]
    public bool hasFlashlight = false;
    [Tooltip("Centang HANYA di B-3 (level pertama): hapus data senter bawaan.")]
    public bool isFirstLevel = false;

    [Header("Awal Game")]
    [Tooltip("Centang kalau player sudah pegang senter sejak spawn (baterai langsung = Start Battery).")]
    public bool startWithFlashlight = false;
    [Tooltip("Baterai awal (saat spawn kalau Start With Flashlight, atau saat ambil senter).")]
    public float startBattery = 80f;

    [Header("Baterai")]
    public float maxBattery = 100f;
    [Tooltip("Makin kecil makin lambat. 0.5 = habis total ~160 detik dari 80%.")]
    public float drainPerSecond = 0.5f;
    public bool infiniteBattery = false;   // debug/QA

    [Header("Kegelapan")]
    [Range(0f, 1f)] public float darkenBelow = 0.5f;        // mulai menggelap saat baterai < 50%
    [Range(0f, 1f)] public float noFlashlightLevel = 0.3f;  // tingkat terang di DarkZone saat belum punya senter

    public float Battery { get; private set; }
    public float Percent => maxBattery > 0f ? Battery / maxBattery : 0f;
    public bool InDarkZone { get; set; }

    // 0 = paling gelap, 1 = terang normal
    public float LightLevel
    {
        get
        {
            if (!hasFlashlight) return InDarkZone ? noFlashlightLevel : 1f;
            return Mathf.Clamp01(Percent / darkenBelow);
        }
    }

    // Data yang dibawa antar level
    private static bool carryHas;
    private static float carryBattery;

    void Awake()
    {
        if (isFirstLevel) { carryHas = false; carryBattery = 0f; }

        if (!isFirstLevel && carryHas)
        {
            hasFlashlight = true;
            Battery = carryBattery;
        }
        else if (startWithFlashlight)
        {
            hasFlashlight = true;
            Battery = Mathf.Min(startBattery, maxBattery);
        }
    }

    void Update()
    {
        if (!hasFlashlight || infiniteBattery) return;
        Battery = Mathf.Max(0f, Battery - drainPerSecond * Time.deltaTime);
    }

    public void PickUp()
    {
        hasFlashlight = true;
        Battery = Mathf.Min(startBattery, maxBattery);
    }

    public void AddBattery(float amount) { Battery = Mathf.Min(maxBattery, Battery + amount); }

    public void Restore(bool has, float battery) { hasFlashlight = has; Battery = battery; }

    // Dipanggil LevelExit sebelum pindah scene
    public void SaveForNextLevel() { carryHas = hasFlashlight; carryBattery = Battery; }
}