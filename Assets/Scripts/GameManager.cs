using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 1 object kosong "GameManager" per scene. Checkpoint, respawn, dan Game Over.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float respawnDelay = 1.5f;
    public float chaserGapOnRespawn = 8f;

    [Header("Game Over")]
    public float gameOverDelay = 1.2f;        // jeda biar animasi death kelihatan
    public string menuSceneName = "MainMenu";

    private PlayerMovement player;
    private PlayerHealth health;
    private FlashlightSystem flash;
    private RisingFlesh chaser;
    private VerticalCameraFollow cam;

    private Vector3 checkpointPos;
    private float checkpointBattery;
    private bool checkpointHasFlash;
    private bool dying;

    public Vector3 CheckpointPosition => checkpointPos;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();
        health = player.GetComponent<PlayerHealth>();
        flash = player.GetComponent<FlashlightSystem>();
        chaser = FindFirstObjectByType<RisingFlesh>();
        cam = FindFirstObjectByType<VerticalCameraFollow>();

        checkpointPos = player.transform.position;
        SaveFlashState();
    }

    public void SetCheckpoint(Vector3 pos)
    {
        checkpointPos = pos;
        SaveFlashState();
    }

    private void SaveFlashState()
    {
        if (flash == null) return;
        checkpointBattery = flash.Battery;
        checkpointHasFlash = flash.hasFlashlight;
    }

    // Dipanggil PlayerHealth setelah player mati
    public void PlayerDied()
    {
        if (dying) return;
        dying = true;
        StartCoroutine(health.CurrentHP <= 0f ? GameOverRoutine() : RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        player.transform.position = checkpointPos;
        player.Revive();
        health.OnRespawn();                       // HP tidak di-reset, cuma kebal sebentar
        if (flash != null) flash.Restore(checkpointHasFlash, checkpointBattery);
        if (chaser != null) chaser.ResetTo(checkpointPos.y - chaserGapOnRespawn);
        if (cam != null) cam.SnapToTarget();

        dying = false;
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);
        if (GameUI.Instance != null) GameUI.Instance.ShowGameOver();
        MusicPlayer.PauseMusic();                 // lagu memudar lalu berhenti
        Time.timeScale = 0f;                      // freeze
    }

    // ---- Hubungkan ke OnClick tombol ----
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneLoader.Reload();
    }

    // Pakai ini untuk tombol "Exit" (Application.Quit tidak jalan di WebGL/itch.io)
    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    void Update()
    {
        // Debug: K = bunuh diri (tes respawn), H = damage 0.5 (tes setengah heart), N = scene berikutnya
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.K)) health.Kill();
            if (Input.GetKeyDown(KeyCode.N)) SceneLoader.LoadNext();
            if (Input.GetKeyDown(KeyCode.H)) health.TakeDamage(0.5f, (Vector2)player.transform.position + Vector2.left);   // tes setengah heart
        }
    }
}