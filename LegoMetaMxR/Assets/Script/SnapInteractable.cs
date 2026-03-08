using UnityEngine;
using System.Collections.Generic;

namespace LegoMetaMxR
{
    public class SnapInteractable : MonoBehaviour
    {
        [Tooltip("Puntos de conexión disponibles en este objeto (ej. Connect_Top)")]
        public List<Transform> snapPoints = new List<Transform>();

        [Tooltip("Radio de detección para el snap")]
        public float snapRadius = 0.1f;

        public bool IsOccupied { get; private set; }

        // Eventos para el sistema de visuales
        public System.Action<SnapInteractor> OnHoverEnter;
        public System.Action<SnapInteractor> OnHoverExit;
        public System.Action<SnapInteractor> OnSnap;

        private void Awake()
        {
            // Auto-detectar puntos de conexión si la lista está vacía
            if (snapPoints.Count == 0)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.Contains("Connect"))
                    {
                        snapPoints.Add(child);
                    }
                }
            }
        }

        public Transform GetClosestSnapPoint(Vector3 position, float maxDistance = -1f)
        {
            Transform closest = null;
            float minDistance = float.MaxValue;
            float checkRadius = (maxDistance < 0) ? snapRadius : maxDistance;

            foreach (var point in snapPoints)
            {
                float dist = Vector3.Distance(point.position, position);
                if (dist < minDistance && dist <= checkRadius)
                {
                    minDistance = dist;
                    closest = point;
                }
            }

            return closest;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (var point in snapPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, snapRadius);
            }
        }
    }
}
