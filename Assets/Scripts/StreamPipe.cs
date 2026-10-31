using System.Collections;
using UnityEngine;

// Pipa uap: siklus mati -> peringatan (uap tipis berkedip, belum bahaya) -> menyembur (damage).
// Total siklus default = 1.5 + 1.5 = 3 detik.
// Setup: object pipa punya child "Steam" berisi SpriteRenderer + BoxCollider2D (Is Trigger) + DamageOnTouch.
public class SteamPipe : MonoBehaviour
{
    public GameObject steam;          // child uap
    public float offTime = 1.5f;      // total waktu mati (termasuk peringatan)
    public float warnTime = 0.5f;     // bagian akhir offTime: uap tipis berkedip
    public float onTime = 1.5f;       // lama menyembur
    public float startDelay = 0f;     // geser ritme antar pipa
    [Range(0f, 1f)] public float warnAlpha = 0.3f;

    private SpriteRenderer sr;
    private Collider2D hitbox;

    IEnumerator Start()
    {
        if (steam == null) yield break;
        sr = steam.GetComponentInChildren<SpriteRenderer>();
        hitbox = steam.GetComponentInChildren<Collider2D>();
        warnTime = Mathf.Min(warnTime, offTime);

        SetSteam(0f, false);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            SetSteam(0f, false);
            yield return new WaitForSeconds(offTime - warnTime);

            // peringatan: berkedip tipis, belum melukai
            float t = 0f;
            while (t < warnTime)
            {
                SetSteam(Mathf.PingPong(t * 12f, 1f) * warnAlpha, false);
                t += Time.deltaTime;
                yield return null;
            }

            SetSteam(1f, true);
            yield return new WaitForSeconds(onTime);
        }
    }

    private void SetSteam(float alpha, bool dangerous)
    {
        if (sr != null) { var c = sr.color; c.a = alpha; sr.color = c; }
        if (hitbox != null) hitbox.enabled = dangerous;
    }
}