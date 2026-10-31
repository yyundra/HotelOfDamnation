using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static bool seen;   // true = tutorial sudah selesai, tidak diulang saat Retry

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI hoverText;

    [Header("Scene")]
    [Tooltip("Object baterai di Hierarchy (yang punya Pickup)")]
    public GameObject battery;

    [Header("Pengaturan")]
    public Vector3 hoverOffset = new Vector3(0f, 1.8f, 0f);
    public float typeSpeed = 0.03f;

    PlayerMovement move;
    Transform player;

    void Start()
    {
        dialoguePanel.SetActive(false);
        hoverText.gameObject.SetActive(false);

        if (seen)
        {
            enabled = false;
            return;
        }

        move = FindFirstObjectByType<PlayerMovement>();
        if (move != null) player = move.transform;

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSecondsRealtime(1f);

        yield return Talk(new string[]
        {
            "There's a monster breakout going on!",
            "I can feel him under my feet.",
            "We need to move forward."
        });

        yield return Hint("Press D to move to the right", KeyCode.D);
        yield return Hint("Press A to move to the left", KeyCode.A);

        yield return Talk(new string[]
        {
            "It's pitch black in here, I have my flashlight but it has no battery.",
            "What is that up there?"
        });

        yield return Hint("Press Space to Jump", KeyCode.Space);

        // Tunggu baterai diambil (Pickup melakukan Destroy)
        ShowHover("Try to get the Battery Up There On Your Left.");
        while (battery != null) yield return null;
        hoverText.gameObject.SetActive(false);

        yield return Flash("Watch your flashlight's battery percentage", 3f);
        yield return Flash("And JUMP UPWARD NOW!!", 3f);

        seen = true;
    }

    // ---------- Text box (game freeze) ----------
    IEnumerator Talk(string[] lines)
    {
        hoverText.gameObject.SetActive(false);
        Time.timeScale = 0f;
        if (move != null) move.enabled = false;
        dialoguePanel.SetActive(true);

        foreach (string line in lines)
        {
            dialogueText.text = line;
            dialogueText.maxVisibleCharacters = 0;
            int shown = 0;
            float timer = 0f;

            // Teks muncul huruf per huruf. Enter/klik = langsung tampil semua
            while (shown < line.Length)
            {
                if (Advance()) break;
                timer += Time.unscaledDeltaTime;
                while (timer >= typeSpeed && shown < line.Length)
                {
                    timer -= typeSpeed;
                    shown++;
                }
                dialogueText.maxVisibleCharacters = shown;
                yield return null;
            }

            dialogueText.maxVisibleCharacters = line.Length;
            yield return null;
            while (!Advance()) yield return null;   // tunggu Enter/klik untuk baris berikutnya
        }

        dialoguePanel.SetActive(false);
        if (move != null) move.enabled = true;
        Time.timeScale = 1f;
    }

    // ---------- Teks melayang di atas player ----------
    IEnumerator Hint(string message, KeyCode key)
    {
        ShowHover(message);
        while (!Input.GetKeyDown(key)) yield return null;
        yield return new WaitForSeconds(0.5f);
        hoverText.gameObject.SetActive(false);
    }

    IEnumerator Flash(string message, float seconds)
    {
        ShowHover(message);
        yield return new WaitForSeconds(seconds);
        hoverText.gameObject.SetActive(false);
    }

    void ShowHover(string message)
    {
        hoverText.text = message;
        hoverText.maxVisibleCharacters = 9999;
        hoverText.gameObject.SetActive(true);
        Follow();
    }

    bool Advance()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);
    }

    void Follow()
    {
        if (player == null || Camera.main == null) return;
        hoverText.transform.position = Camera.main.WorldToScreenPoint(player.position + hoverOffset);
    }

    void LateUpdate()
    {
        if (hoverText != null && hoverText.gameObject.activeSelf) Follow();
    }
}