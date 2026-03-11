using UnityEngine;
using System.Collections;

public class GeneratorPuzzle : MonoBehaviour
{
    [Header("Fuel & Slot")]
    public GameObject requiredFuel;       // assign the correct fuel prefab
    public Transform fuelSlot;            // empty object where fuel sits
    public float snapSpeed = 2f;          // speed to move fuel into slot
    public float hoverAmount = 0.05f;     // hover height for effect
    public float hoverSpeed = 5f;         // hover speed

    [Header("Rune & Wall Pattern")]
    public GameObject runeToSpawn;        // pickup rune prefab
    public Transform runeSpawnPoint;      // where rune appears
    public GameObject wallPattern;        // wall pattern object (set inactive in scene)
    public float waitBeforeRune = 3f;     // wait before spawning rune

    [Header("Puzzle")]
    public bool activated = false;

    [Header("Audio")]
    public AudioSource runeSFX;
    public AudioSource generatorSFX;

    void OnCollisionEnter(Collision collision)
    {
        if (activated) return;

        // Check if collided object is the correct fuel
        if (collision.gameObject == requiredFuel)
        {
            StartCoroutine(ActivateGenerator(collision.gameObject));
        }
    }

    IEnumerator ActivateGenerator(GameObject fuel)
    {
        activated = true;

        Rigidbody rb = fuel.GetComponent<Rigidbody>();
        Collider col = fuel.GetComponent<Collider>();

        // Disable physics and collider
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        if (col != null)
            col.enabled = false;

        // Smoothly move fuel into slot
        float t = 0f;
        Vector3 startPos = fuel.transform.position;
        Quaternion startRot = fuel.transform.rotation;

        generatorSFX.Play();

        while (t < 1f)
        {
            t += Time.deltaTime * snapSpeed;
            fuel.transform.position = Vector3.Lerp(startPos, fuelSlot.position, t);
            fuel.transform.rotation = Quaternion.Lerp(startRot, fuelSlot.rotation, t);

            // Optional hover effect
            fuel.transform.position += Vector3.up * Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;

            yield return null;
        }

        // Wait before spawning rune
        yield return new WaitForSeconds(waitBeforeRune);

        // Spawn rune
        if (runeToSpawn != null && runeSpawnPoint != null)
            Instantiate(runeToSpawn, runeSpawnPoint.position, Quaternion.identity);

        runeSFX.Play();

        // Show wall pattern
        if (wallPattern != null)
            wallPattern.SetActive(true);

        // Notify puzzle manager
        RunePuzzleManager.Instance.GeneratorCompleted();

        // Destroy fuel
        Destroy(fuel);
    }
}
