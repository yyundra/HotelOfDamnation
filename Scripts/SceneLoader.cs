using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // Pindah level: checkpoint level lama dihapus
    public static void Load(string sceneName)
    {
        RunState.ClearCheckpoint();
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadNext()
    {
        RunState.ClearCheckpoint();
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0;
        SceneManager.LoadScene(next);
    }

    // Mati / restart: checkpoint DIPERTAHANKAN
    public static void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
