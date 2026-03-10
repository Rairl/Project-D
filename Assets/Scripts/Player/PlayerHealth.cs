using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 2;
    public int currentHP;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float invincibleTime = 1f; // prevent instant death after teleport
    private bool invincible = false;

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
        // For example:
        // SceneManager.LoadScene("MainMenu");
    }
}
