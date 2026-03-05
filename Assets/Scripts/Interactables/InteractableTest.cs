using UnityEngine;
using System.Collections;

public class InteractableTest : MonoBehaviour, IInteractable
{
    [Header("UI Name")]
    public string interactName = "Pump"; // This is the name shown on the UI

    [Header("Camera")]
    public Camera taskCamera;

    [Header("PlayerSetup")]
    public FPSController playerController;
    public Transform playerViewPoint; // empty object marking where player stands for this task

    [Header("Smooth Movement")]
    public float moveDuration = 0.3f;      // duration of smooth move to task

    public void Interact()
    {
        Debug.Log("Interacting with " + interactName);

        playerController.StartInteraction();

        if (taskCamera != null)
            playerController.SwitchCamera(taskCamera);

        if (playerViewPoint != null)
        {
            playerController.transform.position = playerViewPoint.position;
            playerController.transform.rotation = playerViewPoint.rotation;
        }

    }

    public Camera GetInteractionCamera()
    {
        return taskCamera;
    }

    // Smooth player movement coroutine
    IEnumerator MovePlayerSmooth(Transform player, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            player.position = Vector3.Lerp(startPos, targetPos, t);
            player.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        player.position = targetPos;
        player.rotation = targetRot;
    }
}
