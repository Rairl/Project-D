using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorEscape : MonoBehaviour
{
    public AudioSource runningSFX;
    public CanvasGroup winPanel;   // UI panel with CanvasGroup for fading
    public float fadeDuration = 1f;

    bool triggered = false;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !triggered)
        {
            Debug.Log("Win");
            triggered = true;
            StartCoroutine(WinSequence());
        }
    }

    IEnumerator WinSequence()
    {
        // Play running SFX
        if (runningSFX != null)
            runningSFX.Play();

        // Wait 3 seconds
        yield return new WaitForSeconds(1f);

        // Fade in win panel
        yield return StartCoroutine(FadeInPanel());

        // Wait another 3 seconds
        yield return new WaitForSeconds(2f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator FadeInPanel()
    {
        float time = 0f;
        winPanel.gameObject.SetActive(true);

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            winPanel.alpha = time / fadeDuration;
            yield return null;
        }

        winPanel.alpha = 1;
    }
}
