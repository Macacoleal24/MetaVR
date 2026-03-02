using System.Collections.Generic;
using UnityEngine;

public class XRAAdvancedUIInteractionDemo : MonoBehaviour
{
    [SerializeField] private GameObject[] modelTemplates;
    [SerializeField] private Material[] availableMaterials;
    [SerializeField] private Renderer materialPreviewRenderer;

    [SerializeField] private Vector3 spawnPosition = new Vector3(1, 1.5f, 2);
    [SerializeField] private Vector3 spawnRotation = new Vector3(45, 45, 0);
    [SerializeField] private Vector3 spawnScale = new Vector3(0.35f, 0.35f, 0.35f);

    private int activeModelIndex = -1;
    private int activeMaterialIndex = -1;
    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    public void SelectModel(int index)
    {
        if (!IsValidModelIndex(index)) return;
        if (!HasCompatibleRenderer(modelTemplates[index])) return;
        activeModelIndex = index;
    }

    public void SelectModelAndGenerate(int index)
    {
        SelectModel(index);
        GenerateActiveModel();
    }

    public void GenerateActiveModel()
    {
        if (!IsValidModelIndex(activeModelIndex)) return;
        GameObject template = modelTemplates[activeModelIndex];
        if (template == null) return;

        GameObject instance = Instantiate(template);
        instance.transform.position = spawnPosition;
        instance.transform.rotation = Quaternion.Euler(spawnRotation);
        instance.transform.localScale = spawnScale;
        spawnedObjects.Add(instance);

        if (IsValidMaterialIndex(activeMaterialIndex))
        {
            ApplyMaterial(instance, availableMaterials[activeMaterialIndex]);
        }
    }

    public void SelectMaterial(int index)
    {
        if (!IsValidMaterialIndex(index)) return;
        Material material = availableMaterials[index];
        activeMaterialIndex = index;

        if (materialPreviewRenderer != null)
        {
            ApplyMaterial(materialPreviewRenderer.gameObject, material);
        }

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            ApplyMaterial(spawnedObjects[i], material);
        }
    }

    private bool IsValidModelIndex(int index)
    {
        return modelTemplates != null && index >= 0 && index < modelTemplates.Length && modelTemplates[index] != null;
    }

    private bool IsValidMaterialIndex(int index)
    {
        return availableMaterials != null && index >= 0 && index < availableMaterials.Length && availableMaterials[index] != null;
    }

    private bool HasCompatibleRenderer(GameObject template)
    {
        return template != null && template.GetComponentInChildren<Renderer>() != null;
    }

    private void ApplyMaterial(GameObject target, Material material)
    {
        if (target == null || material == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material[] materials = r.materials;
            for (int m = 0; m < materials.Length; m++)
            {
                materials[m] = material;
            }
            r.materials = materials;
        }
    }
}
