using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    public string menuScene = "MainMenu";
    public float inputDelay = 2f;   // tombol baru aktif setelah 2 detik

    float timer;
    bool leaving;

    void Update()
    {
        if (leaving) return;
        timer += Time.deltaTime;

        if (timer >= inputDelay && Input.anyKeyDown)
        {
            leaving = true;
            SceneFader.Go(menuScene);
        }
    }
}