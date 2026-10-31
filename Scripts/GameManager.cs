using System.Collections;
using UnityEngine;

// 1 object kosong "GameManager" per scene. Mengatur checkpoint & respawn (tanpa reload scene).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float respawnDelay = 1.5f;
    public float chaserGapOnRespawn = 8f;   // jarak chaser di bawah checkpoint setelah respawn

    private PlayerMovement player;
    private PlayerHealth health;
    private FlashlightSystem flash;
    private RisingFlesh chaser;
    private VerticalCameraFollow cam;

    private Vector3 checkpointPos;
    private float checkpointBattery;
    private bool checkpointHasFlash;
    private bool dying;

    void Awake() { Instance = this; }

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();
        health = player.GetComponent<PlayerHealth>();
        flash = player.GetComponent<FlashlightSystem>();
        chaser = FindFirstObjectByType<RisingFlesh>();
        cam = FindFirstObjectByType<VerticalCameraFollow>();

        checkpointPos = player.transform.position;   // checkpoint awal = posisi spawn
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

    public void PlayerDied()
    {
        if (dying) return;
        dying = true;
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        player.transform.position = checkpointPos;
        player.Revive();
        health.ResetHealth();
        if (flash != null) flash.Restore(checkpointHasFlash, checkpointBattery);
        if (chaser != null) chaser.ResetTo(checkpointPos.y - chaserGapOnRespawn);
        if (cam != null) cam.SnapToTarget();

        dying = false;
    }

    void Update()
    {
        // Debug: K = bunuh diri (tes respawn), N = scene berikutnya
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.K)) health.Kill();
            if (Input.GetKeyDown(KeyCode.N)) SceneLoader.LoadNext();
        }
    }
}
