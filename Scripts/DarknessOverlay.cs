using UnityEngine;

// Vignette fullscreen yang berpusat di player. Lubang terang menyempit seiring baterai habis.
// Pasang di object Vignette (UI Image, sprite Vignette.png, size 6000x6000, + CanvasGroup).
public class DarknessOverlay : MonoBehaviour
{
    public FlashlightSystem flashlight;
    public Transform player;
    public Camera cam;
    public RectTransform vignette;
    public CanvasGroup group;

    [Header("Skala vignette")]
    public float darkScale = 0.6f;    // paling gelap (lubang kecil di sekitar player)
    public float clearScale = 8f;     // terang normal (vignette tidak terlihat)
    public float smooth = 3f;

    private float current;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        current = clearScale;
    }

    void LateUpdate()
    {
        if (flashlight == null || player == null || vignette == null) return;

        // interpolasi logaritmik supaya perubahan terasa merata
        float target = darkScale * Mathf.Pow(clearScale / darkScale, flashlight.LightLevel);
        current = Mathf.Lerp(current, target, smooth * Time.deltaTime);

        vignette.localScale = Vector3.one * current;
        vignette.position = cam.WorldToScreenPoint(player.position);   // Canvas = Screen Space Overlay

        if (group != null) group.alpha = Mathf.InverseLerp(clearScale * 0.95f, clearScale * 0.6f, current);
    }
}
