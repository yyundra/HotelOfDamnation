using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;
    public GameObject gameOverPanel;     

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void ShowGameOver() { if (gameOverPanel != null) gameOverPanel.SetActive(true); }

    public void Retry() { if (GameManager.Instance != null) GameManager.Instance.Retry(); }
    public void ExitToMenu() { if (GameManager.Instance != null) GameManager.Instance.ExitToMenu(); }
}