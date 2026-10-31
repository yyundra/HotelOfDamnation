using System.Collections;
using UnityEngine;

public class PopUpSpikes : MonoBehaviour
{
    [Tooltip("Object spike (child) yang punya SpriteRenderer, Collider2D, dan DamageOnTouch")]
    public GameObject spikes;

    public float startDelay = 0f;
    public float hiddenTime = 2f;     // spike tersembunyi
    public float warningTime = 0.5f;  // spike samar-samar (belum berbahaya)
    public float visibleTime = 1f;    // spike aktif / berbahaya

    SpriteRenderer sr;
    Collider2D[] cols;

    IEnumerator Start()
    {
        sr = spikes.GetComponentInChildren<SpriteRenderer>();
        cols = spikes.GetComponentsInChildren<Collider2D>();

        SetState(false, 0f);
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            SetState(false, 0f);
            yield return new WaitForSeconds(hiddenTime);

            SetState(false, 0.35f);
            yield return new WaitForSeconds(warningTime);

            SetState(true, 1f);
            yield return new WaitForSeconds(visibleTime);
        }
    }

    void SetState(bool dangerous, float alpha)
    {
        if (sr != null)
        {
            sr.enabled = alpha > 0f;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        foreach (var col in cols) col.enabled = dangerous;
    }
}