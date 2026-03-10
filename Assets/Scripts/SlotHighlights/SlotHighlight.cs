using UnityEngine;

public class SlotHighlight : MonoBehaviour
{
    public Renderer slotRenderer;
    public Material defaultMaterial;
    public Material highlightMaterial;
    [HideInInspector] public bool isHighlighted = false;

    void Start()
    {
        if (slotRenderer == null)
            slotRenderer = GetComponent<Renderer>();

        if (slotRenderer != null && defaultMaterial != null)
            slotRenderer.material = defaultMaterial;
    }

    public void HighlightSlot()
    {
        if (!isHighlighted && slotRenderer != null && highlightMaterial != null)
        {
            slotRenderer.material = highlightMaterial;
            isHighlighted = true;
        }
    }

    public void ResetSlot()
    {
        if (isHighlighted && slotRenderer != null && defaultMaterial != null)
        {
            slotRenderer.material = defaultMaterial;
            isHighlighted = false;
        }
    }
}
