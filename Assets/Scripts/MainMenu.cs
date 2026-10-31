using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public string firstScene = "Basement1";

    public void Play()
    {
        TutorialManager.seen = false;
        SceneFader.Go(firstScene);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}