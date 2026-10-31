using UnityEngine;
using UnityEngine.SceneManagement;

public static class RunState
{
    // Checkpoint (bertahan saat scene di-reload, dihapus saat pindah level)
    public static bool HasCheckpoint;
    public static Vector2 CpPos;
    public static string CheckpointScene;

    // True hanya kalau checkpoint tersimpan di scene yang sedang aktif
    public static bool HasCheckpointHere =>
        HasCheckpoint && CheckpointScene == SceneManager.GetActiveScene().name;

    // Status yang dibawa antar level
    public static bool HasFlashlight;
    public static float BatteryPercent = 100f;

    public static void SetCheckpoint(Vector2 position, float battery, bool hasFlashlight)
    {
        HasCheckpoint = true;
        CpPos = position;
        CheckpointScene = SceneManager.GetActiveScene().name;
        BatteryPercent = battery;
        HasFlashlight = hasFlashlight;
    }

    public static void ClearCheckpoint()
    {
        HasCheckpoint = false;
        CpPos = Vector2.zero;
        CheckpointScene = null;
        // Senter dan baterai sengaja tidak direset (dibawa antar level)
    }

    public static void ResetAll()
    {
        ClearCheckpoint();
        HasFlashlight = false;
        BatteryPercent = 100f;
    }
}