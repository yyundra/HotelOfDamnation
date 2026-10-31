using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 3 heart + persentase baterai di bawahnya.
public class HUD : MonoBehaviour
{
    public PlayerHealth health;
    public FlashlightSystem flashlight;

    [Header("Hearts (urut kiri ke kanan)")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;                 // boleh kosong -> heart hanya diredupkan
    public Color emptyColor = new Color(1, 1, 1, 0.25f);

    [Header("Baterai")]
    public TMP_Text batteryText;
    public Color lowColor = Color.red;
    public float lowThreshold = 0.2f;

    void Start()
    {
        if (health != null) { health.OnHPChanged += UpdateHearts; UpdateHearts(health.CurrentHP, health.maxHP); }
    }

    void OnDestroy() { if (health != null) health.OnHPChanged -= UpdateHearts; }

    void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool full = i < current;
            if (emptyHeart != null) { hearts[i].sprite = full ? fullHeart : emptyHeart; hearts[i].color = Color.white; }
            else hearts[i].color = full ? Color.white : emptyColor;
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
