using UnityEngine;

public class XRAMaterialSelectionUI : MonoBehaviour
{
    [SerializeField] private XRAAdvancedUIInteraction modelController;
    [SerializeField] private Material[] availableMaterials;
    [SerializeField] private Renderer materialPreviewRenderer;

    public void SelectMaterial(int index)
    {
        if (!IsValidMaterialIndex(index)) return;
        Material material = availableMaterials[index];
        ApplyToPreview(material);
        if (modelController != null)
        {
            modelController.SetActiveMaterial(material);
        }
    }

    private void ApplyToPreview(Material material)
    {
        if (materialPreviewRenderer == null || material == null) return;
        Material[] materials = materialPreviewRenderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = material;
        }
        materialPreviewRenderer.materials = materials;
    }

    private bool IsValidMaterialIndex(int index)
    {
        return availableMaterials != null && index >= 0 && index < availableMaterials.Length && availableMaterials[index] != null;
    }
}
