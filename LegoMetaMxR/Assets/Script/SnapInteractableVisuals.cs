using UnityEngine;
using System.Collections.Generic;

namespace LegoMetaMxR
{
    public class SnapInteractableVisuals : MonoBehaviour
    {
        [SerializeField] private SnapInteractable snapInteractable;
        [SerializeField] private Material hoverMaterial;

        private GameObject currentGhostObject;
        private SnapInteractor currentInteractor;

        private void OnEnable()
        {
            if (snapInteractable != null)
            {
                snapInteractable.OnHoverEnter += OnHoverEnter;
                snapInteractable.OnHoverExit += OnHoverExit;
                snapInteractable.OnSnap += OnSnap;
            }
        }

        private void OnDisable()
        {
            if (snapInteractable != null)
            {
                snapInteractable.OnHoverEnter -= OnHoverEnter;
                snapInteractable.OnHoverExit -= OnHoverExit;
                snapInteractable.OnSnap -= OnSnap;
            }
        }

        private void OnHoverEnter(SnapInteractor interactor)
        {
            if (currentInteractor == interactor) return;
            
            // Si ya hay uno diferente, limpiar
            if (currentInteractor != null && currentInteractor != interactor)
            {
                CleanupGhost();
            }

            currentInteractor = interactor;
            SetupGhostModel(interactor);
        }

        private void OnHoverExit(SnapInteractor interactor)
        {
            if (currentInteractor == interactor)
            {
                CleanupGhost();
                currentInteractor = null;
            }
        }

        private void OnSnap(SnapInteractor interactor)
        {
            // Al hacer snap, ocultar el ghost (el objeto real toma su lugar)
            CleanupGhost();
            currentInteractor = null;
        }

        private void CleanupGhost()
        {
            if (currentGhostObject != null)
            {
                Destroy(currentGhostObject);
                currentGhostObject = null;
            }
        }

        private void Update()
        {
            if (currentInteractor != null && currentGhostObject != null)
            {
                // Usar los puntos calculados por el interactor para una visualización precisa
                Transform myPoint = currentInteractor.BestLocalPoint;
                Transform targetPoint = currentInteractor.BestTargetPoint;

                // Si por alguna razón el interactor no tiene puntos válidos (ej. acaba de salir de rango pero no hemos procesado exit), usar fallback
                if (myPoint != null && targetPoint != null)
                {
                    // Calcular offset para alinear myPoint con targetPoint
                    Vector3 offset = currentInteractor.transform.position - myPoint.position;
                    currentGhostObject.transform.position = targetPoint.position + offset;
                    
                    // Mantener la rotación del interactor para el ghost
                    currentGhostObject.transform.rotation = currentInteractor.transform.rotation;
                }
            }
        }

        private void SetupGhostModel(SnapInteractor interactor)
        {
            // Crear un objeto vacío para contener el ghost
            currentGhostObject = new GameObject($"{interactor.name}_Ghost");
            currentGhostObject.transform.localScale = interactor.transform.localScale;
            
            // No lo hacemos hijo del interactable para tener libertad de movimiento global
            // Pero lo destruiremos al salir del hover
            
            // Copiar MeshFilter/Renderer del objeto raíz del interactor
            CopyMeshVisuals(interactor.gameObject, currentGhostObject);

            // Copiar MeshFilter/Renderer de los hijos
            foreach (Transform child in interactor.transform)
            {
                // Ignorar puntos de conexión y otros objetos vacíos sin mesh
                if (child.GetComponent<MeshRenderer>() == null) continue;

                GameObject ghostChild = new GameObject(child.name);
                ghostChild.transform.SetParent(currentGhostObject.transform, false);
                ghostChild.transform.localPosition = child.localPosition;
                ghostChild.transform.localRotation = child.localRotation;
                ghostChild.transform.localScale = child.localScale;

                CopyMeshVisuals(child.gameObject, ghostChild);
            }
        }

        private void CopyMeshVisuals(GameObject source, GameObject destination)
        {
            var sourceMeshFilter = source.GetComponent<MeshFilter>();
            var sourceRenderer = source.GetComponent<MeshRenderer>();

            if (sourceMeshFilter != null && sourceRenderer != null)
            {
                var destFilter = destination.AddComponent<MeshFilter>();
                destFilter.mesh = sourceMeshFilter.sharedMesh;

                var destRenderer = destination.AddComponent<MeshRenderer>();
                // Usar material de hover si está asignado, sino mantener original (opcional, aquí forzamos hover)
                if (hoverMaterial != null)
                {
                    destRenderer.material = hoverMaterial;
                }
                else
                {
                    destRenderer.material = sourceRenderer.sharedMaterial;
                }
            }
        }
    }
}
