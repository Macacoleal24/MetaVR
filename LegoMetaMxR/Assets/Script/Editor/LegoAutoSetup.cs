using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Oculus.Interaction;

namespace LegoMetaMxR.Editor
{
    public class LegoAutoSetup : UnityEditor.Editor
    {
        [MenuItem("Lego/Setup Snapping for All Prefabs")]
        public static void SetupSnapping()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Models/Prefabs" });
            int count = 0;

            // Crear/Cargar material Ghost
            Material ghostMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GhostHover.mat");
            if (ghostMat == null)
            {
                ghostMat = new Material(Shader.Find("Standard"));
                ghostMat.color = new Color(0.0f, 1.0f, 0.0f, 0.4f); // Verde transparente
                // Configurar para transparencia (Standard Shader)
                ghostMat.SetFloat("_Mode", 3);
                ghostMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                ghostMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                ghostMat.SetInt("_ZWrite", 0);
                ghostMat.DisableKeyword("_ALPHATEST_ON");
                ghostMat.EnableKeyword("_ALPHABLEND_ON");
                ghostMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                ghostMat.renderQueue = 3000;
                
                AssetDatabase.CreateAsset(ghostMat, "Assets/Materials/GhostHover.mat");
                Debug.Log("Created GhostHover material");
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Cargar el contenido del prefab para edición segura
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);

                if (prefabContents != null)
                {
                    bool modified = false;

                    // 1. Añadir SnapInteractable
                    SnapInteractable interactable = prefabContents.GetComponent<SnapInteractable>();
                    if (interactable == null)
                    {
                        interactable = prefabContents.AddComponent<SnapInteractable>();
                        modified = true;
                    }

                    // Configurar puntos de conexión
                    if (interactable.snapPoints == null) interactable.snapPoints = new List<Transform>();
                    
                    // Buscar hijos Connect_ recursivamente
                    Transform[] allChildren = prefabContents.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in allChildren)
                    {
                        if (t.name.Contains("Connect") && !interactable.snapPoints.Contains(t))
                        {
                            interactable.snapPoints.Add(t);
                            modified = true;
                        }
                    }

                    // 2. Añadir SnapInteractor
                    SnapInteractor interactor = prefabContents.GetComponent<SnapInteractor>();
                    if (interactor == null)
                    {
                        interactor = prefabContents.AddComponent<SnapInteractor>();
                        modified = true;
                    }

                    // Configurar Main Snap Point (Bottom)
                    if (interactor.mainSnapPoint == null)
                    {
                        foreach (Transform t in allChildren)
                        {
                            if (t.name == "Connect_Bottom")
                            {
                                interactor.mainSnapPoint = t;
                                modified = true;
                                break;
                            }
                        }
                    }

                    // 3. Añadir SnapInteractableVisuals (NUEVO)
                    SnapInteractableVisuals visuals = prefabContents.GetComponent<SnapInteractableVisuals>();
                    if (visuals == null)
                    {
                        visuals = prefabContents.AddComponent<SnapInteractableVisuals>();
                        modified = true;
                    }

                    // Configurar campos privados de SnapInteractableVisuals usando SerializedObject
                    SerializedObject soVisuals = new SerializedObject(visuals);
                    SerializedProperty propInteractable = soVisuals.FindProperty("snapInteractable");
                    SerializedProperty propMaterial = soVisuals.FindProperty("hoverMaterial");

                    if (propInteractable.objectReferenceValue == null)
                    {
                        propInteractable.objectReferenceValue = interactable;
                        modified = true;
                    }
                    if (propMaterial.objectReferenceValue == null)
                    {
                        propMaterial.objectReferenceValue = ghostMat;
                        modified = true;
                    }
                    soVisuals.ApplyModifiedProperties();


                    // Asegurar Rigidbody (necesario para interacciones físicas)
                    if (prefabContents.GetComponent<Rigidbody>() == null)
                    {
                        Rigidbody rb = prefabContents.AddComponent<Rigidbody>();
                        rb.isKinematic = true; // Por defecto kinematic para que no caiga solo
                        rb.useGravity = false;
                        modified = true;
                    }

                    if (modified)
                    {
                        // Guardar cambios en el prefab
                        PrefabUtility.SaveAsPrefabAsset(prefabContents, path);
                        count++;
                        Debug.Log($"Updated prefab: {prefabContents.name}");
                    }
                    
                    // Descargar contenido de memoria
                    PrefabUtility.UnloadPrefabContents(prefabContents);
                }
            }

            Debug.Log($"Lego Snapping Setup Complete. Updated {count} prefabs.");
        }
    }
}
