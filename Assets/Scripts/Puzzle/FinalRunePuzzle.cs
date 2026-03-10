using UnityEngine;
using System.Collections;

public class FinalRunePuzzle : MonoBehaviour
{
    [Header("Puzzle Slots")]
    public Transform[] runeSlots;            // Empty objects for rune placement
    public int[] correctOrder;               // Expected runeID for each slot

    [Header("Door")]
    public DoorEscape door;                  // Door to open when puzzle is solved
    public float snapDistance = 1f;          // Distance to snap rune into slot

    [Header("Pop Out Settings")]
    public float popForce = 2f;             // Upward force for wrong rune pop
    public Vector3 popDirection = new Vector3(0, 1, -1); // direction to pop

    private GameObject[] placedRunes;       // Tracks runes currently in slots

    public GameObject Door;

    void Start()
    {
        placedRunes = new GameObject[runeSlots.Length];
    }

    /// <summary>
    /// Call this when a player drops a rune in the puzzle area
    /// </summary>
    public void TryPlaceRune(GameObject rune)
    {
        for (int i = 0; i < runeSlots.Length; i++)
        {
            if (placedRunes[i] != null) continue; // Slot already occupied

            float dist = Vector3.Distance(rune.transform.position, runeSlots[i].position);
            if (dist <= snapDistance)
            {
                // This is where you put the Updated Snap Logic
                rune.transform.SetParent(null);
                rune.transform.position = runeSlots[i].position;
                rune.transform.rotation = runeSlots[i].rotation;

                Rigidbody rb = rune.GetComponent<Rigidbody>();
                Collider col = rune.GetComponent<Collider>();

                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                }
                if (col != null)
                    col.enabled = false;

                placedRunes[i] = rune;

                if (!IsRuneCorrect(rune, i))
                {
                    Debug.Log($"Rune {rune.name} is wrong! Will pop out in 3 seconds...");
                    StartCoroutine(PopOutRuneDelayed(i, 3f));
                    return;
                }

                CheckPuzzleComplete();
                return;
            }
        }
    }

    //Delay for wrong rune pop out
    IEnumerator PopOutRuneDelayed(int slotIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        PopOutRune(slotIndex);
    }

    public void UpdateSlotHighlights(GameObject heldRune)
    {
        for (int i = 0; i < runeSlots.Length; i++)
        {
            SlotHighlight slotHighlight = runeSlots[i].GetComponent<SlotHighlight>();
            if (slotHighlight == null) continue;

            float dist = Vector3.Distance(heldRune.transform.position, runeSlots[i].position);
            if (dist <= snapDistance)
            {
                slotHighlight.HighlightSlot();
            }
            else
            {
                slotHighlight.ResetSlot();
            }
        }
    }

    bool IsRuneCorrect(GameObject rune, int slotIndex)
    {
        RuneObject runeScript = rune.GetComponent<RuneObject>();
        if (runeScript == null) return false;

        return runeScript.runeID == correctOrder[slotIndex];
    }

    void PopOutRune(int slotIndex)
    {
        GameObject wrongRune = placedRunes[slotIndex];
        if (wrongRune == null) return;

        placedRunes[slotIndex] = null;

        // Detach from slot
        wrongRune.transform.SetParent(null);

        Rigidbody rb = wrongRune.GetComponent<Rigidbody>();
        Collider col = wrongRune.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(popDirection.normalized * popForce, ForceMode.Impulse);
        }
        if (col != null)
            col.enabled = true;

    }

    void CheckPuzzleComplete()
    {
        for (int i = 0; i < correctOrder.Length; i++)
        {
            if (placedRunes[i] == null) return; // Slot empty puzzle incomplete

            RuneObject runeScript = placedRunes[i].GetComponent<RuneObject>();
            if (runeScript == null) return;

            if (runeScript.runeID != correctOrder[i])
                return; // Wrong rune puzzle incomplete
        }

        // Puzzle complete
        Debug.Log("Final Rune Puzzle Solved!");

        Door.SetActive(true);
    }
}
