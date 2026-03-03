using System.Collections.Generic;
using UnityEngine;

public class XRAAdvancedUIInteraction : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPosition = new Vector3(1, 1.5f, 2);
    [SerializeField] private Vector3 spawnRotation = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 spawnScale = new Vector3(8f, 8f, 8f);

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private Material activeMaterial;

    public void GenerateModel(GameObject modelTemplate)
    {
        GenerateModel(modelTemplate, null);
    }

    public void GenerateModel(GameObject modelTemplate, Transform spawnPoint)
    {
        if (modelTemplate == null) return;
        if (!HasCompatibleRenderer(modelTemplate)) return;

        GameObject instance = Instantiate(modelTemplate);
        if (spawnPoint != null)
        {
            instance.transform.position = spawnPoint.position;
            instance.transform.rotation = spawnPoint.rotation;
        }
        else
        {
            instance.transform.position = spawnPosition;
            instance.transform.rotation = Quaternion.Euler(spawnRotation);
        }
        instance.transform.localScale = spawnScale;
        spawnedObjects.Add(instance);

        if (activeMaterial != null)
        {
            ApplyMaterial(instance, activeMaterial);
        }
    }

    public void SetActiveMaterial(Material material)
    {
        if (material == null) return;
        activeMaterial = material;
    }

    private bool HasCompatibleRenderer(GameObject template)
    {
        return template != null && template.GetComponentInChildren<Renderer>() != null;
    }

    private void ApplyMaterial(GameObject target, Material material)
    {
        if (target == null || material == null) return;
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return;
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
