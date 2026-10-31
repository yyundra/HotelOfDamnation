using UnityEngine;

// Taruh di scene "Victory". Hubungkan ke OnClick Button.
public class VictoryScreen : MonoBehaviour
{
    public string firstLevelScene = "B3";
    public string mainMenuScene = "";   // kosong = tombol menu tidak dipakai

    public void PlayAgain() { SceneLoader.Load(firstLevelScene); }

    public void MainMenu() { if (!string.IsNullOrEmpty(mainMenuScene)) SceneLoader.Load(mainMenuScene); }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
