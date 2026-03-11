using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenemanager : MonoBehaviour
{
    public void onStartPressed()
    {
        // Load main scene
        SceneManager.LoadScene("MainScene");
    }

    public void onQuit()
    {
        Application.Quit();
    }
}
