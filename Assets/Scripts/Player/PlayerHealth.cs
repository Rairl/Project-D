using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 2;
    public int currentHP;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float invincibleTime = 1f; // prevent instant death after teleport
    private bool invincible = false;

    [Header("LosePanel")]
    public AudioSource diedSFX;
    public CanvasGroup losePanel;   // UI panel with CanvasGroup for fading
    public float fadeDuration = 1f;
    public GameObject enemy;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (invincible) return;

        currentHP -= damage;
        Debug.Log("Player HP: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // Teleport player to respawn
            Respawn();
        }

        StartCoroutine(ResetInvincibility());
    }

    void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            Debug.Log("Player teleported to respawn point!");
        }
    }

    IEnumerator ResetInvincibility()
    {
        invincible = true;
        yield return new WaitForSeconds(invincibleTime);
        invincible = false;
    }

    void Die()
    {
        Debug.Log("Player Died!");
        // You can add Game Over panel or reload scene here
        StartCoroutine(LoseSequence());
        Destroy(enemy);
    }

    IEnumerator LoseSequence()
    {
        // Play running SFX
        if (diedSFX != null)
            diedSFX.Play();

        // Wait 3 seconds
        yield return new WaitForSeconds(1f);

        // Fade in win panel
        yield return StartCoroutine(FadeInPanel());

        // Wait another 3 seconds
        yield return new WaitForSeconds(2f);

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator FadeInPanel()
    {
        float time = 0f;
        losePanel.gameObject.SetActive(true);

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            losePanel.alpha = time / fadeDuration;
            yield return null;
        }
    }
}
