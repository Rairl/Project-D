using UnityEngine;

public class RuneObject : MonoBehaviour, IInteractable
{
    public string interactName = "Pick up Rune";
    public int runeID; // 0, 1, 2 for different runes

    public void Interact()
    {
        FPSController player = FindFirstObjectByType<FPSController>();

        if (player.heldObject != null) return;

        player.heldObject = gameObject;

        // STORE ROTATION OFFSET
        player.heldRotationOffset =
            Quaternion.Inverse(player.holdPoint.rotation) * transform.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    public Camera GetInteractionCamera()
    {
        return null; // optional: you can assign a special camera if desired
    }
}
