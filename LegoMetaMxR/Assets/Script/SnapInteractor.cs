using UnityEngine;
using System.Collections.Generic;
using Oculus.Interaction;

namespace LegoMetaMxR
{
    [RequireComponent(typeof(Grabbable))]
    [RequireComponent(typeof(Rigidbody))]
    public class SnapInteractor : MonoBehaviour
    {
        [Tooltip("Punto de conexión principal de este objeto (ej. Connect_Bottom)")]
        public Transform mainSnapPoint;
        
        [Tooltip("Radio de búsqueda de otros interactables")]
        public float searchRadius = 0.05f;

        // Nuevas propiedades públicas para exponer el mejor punto de snap calculado
        public Transform BestLocalPoint { get; private set; }
        public Transform BestTargetPoint { get; private set; }

        private Grabbable _grabbable;
        private Rigidbody _rigidbody;
        private bool _isGrabbed = false;
        private SnapInteractable _currentSnapTarget;
        private SnapInteractable _currentHoverInteractable;
        private SnapInteractable _selfInteractable; // Referencia al interactable propio

        private void Awake()
        {
            _grabbable = GetComponent<Grabbable>();
            _rigidbody = GetComponent<Rigidbody>();
            _selfInteractable = GetComponent<SnapInteractable>();

            // Intentar encontrar el punto de conexión inferior por defecto
            if (mainSnapPoint == null)
            {
                Transform bottom = transform.Find("Connect_Bottom");
                if (bottom != null) mainSnapPoint = bottom;
                else mainSnapPoint = transform; // Fallback
            }
        }

        private void OnEnable()
        {
            if (_grabbable != null)
            {
                _grabbable.WhenPointerEventRaised += HandlePointerEvent;
            }
        }

        private void OnDisable()
        {
            if (_grabbable != null)
            {
                _grabbable.WhenPointerEventRaised -= HandlePointerEvent;
            }
        }

        private void HandlePointerEvent(PointerEvent evt)
        {
            if (evt.Type == PointerEventType.Select)
            {
                _isGrabbed = true;
                // Al agarrar, desactivar kinematic para física (si estaba en snap)
                _rigidbody.isKinematic = false;
                
                // Desvincular (Unparent) del objeto padre para permitir separación
                transform.SetParent(null);
                
                // Limpiar hover previo si existe
                if (_currentHoverInteractable != null)
                {
                    _currentHoverInteractable.OnHoverExit?.Invoke(this);
                    _currentHoverInteractable = null;
                }
            }
            else if (evt.Type == PointerEventType.Unselect)
            {
                _isGrabbed = false;
                // Al soltar, intentar hacer snap
                TrySnap();
                
                // Limpiar hover al soltar
                if (_currentHoverInteractable != null)
                {
                    _currentHoverInteractable.OnHoverExit?.Invoke(this);
                    _currentHoverInteractable = null;
                }
            }
        }

        private void Update()
        {
            if (_isGrabbed)
            {
                UpdateHoverLogic();
            }
        }

        private void UpdateHoverLogic()
        {
            if (mainSnapPoint == null) return;

            // Histéresis: si ya tenemos un hover, usar un radio mayor para evitar flickering
            float currentSearchRadius = (_currentHoverInteractable != null) ? searchRadius * 1.2f : searchRadius;

            // Buscar interactables cercanos (Broad Phase)
            Collider[] colliders = Physics.OverlapSphere(mainSnapPoint.position, currentSearchRadius);
            
            SnapInteractable bestInteractable = null;
            float minDst = float.MaxValue;
            Transform bestLocal = null;
            Transform bestTarget = null;

            foreach (var col in colliders)
            {
                if (col.transform.root == transform.root) continue;

                var interactable = col.GetComponentInParent<SnapInteractable>();
                if (interactable != null)
                {
                    // Narrow Phase: Comparar todos los puntos locales contra todos los puntos del target
                    List<Transform> myPoints = new List<Transform>();
                    if (_selfInteractable != null && _selfInteractable.snapPoints.Count > 0)
                    {
                        myPoints.AddRange(_selfInteractable.snapPoints);
                    }
                    else
                    {
                        myPoints.Add(mainSnapPoint);
                    }

                    foreach (Transform myPoint in myPoints)
                    {
                        // Iterar manualmente sobre los puntos del target para verificar compatibilidad (polaridad)
                        foreach (Transform targetPoint in interactable.snapPoints)
                        {
                            // Verificar distancia dentro del radio
                            float dst = Vector3.Distance(myPoint.position, targetPoint.position);
                            if (dst > currentSearchRadius) continue;

                            // Verificar compatibilidad (Top con Bottom)
                            if (!IsCompatible(myPoint.name, targetPoint.name)) continue;

                            if (dst < minDst)
                            {
                                minDst = dst;
                                bestInteractable = interactable;
                                bestLocal = myPoint;
                                bestTarget = targetPoint;
                            }
                        }
                    }
                }
            }

            // Actualizar propiedades públicas
            BestLocalPoint = bestLocal;
            BestTargetPoint = bestTarget;

            // Gestionar eventos de Hover
            if (bestInteractable != _currentHoverInteractable)
            {
                if (_currentHoverInteractable != null)
                {
                    _currentHoverInteractable.OnHoverExit?.Invoke(this);
                }

                _currentHoverInteractable = bestInteractable;

                if (_currentHoverInteractable != null)
                {
                    _currentHoverInteractable.OnHoverEnter?.Invoke(this);
                }
            }
        }

        private void TrySnap()
        {
            if (mainSnapPoint == null) return;

            // Reutilizamos la lógica de UpdateHoverLogic que ya calculó el mejor punto
            if (_currentHoverInteractable != null && BestLocalPoint != null && BestTargetPoint != null)
            {
                PerformSnap(BestLocalPoint, BestTargetPoint);
            }
        }

        private void PerformSnap(Transform myPoint, Transform targetPoint)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            // Calcular offset para alinear myPoint con targetPoint
            // Queremos que myPoint.position == targetPoint.position
            
            // 1. Alinear posición
            Vector3 offset = transform.position - myPoint.position;
            transform.position = targetPoint.position + offset;

            // 2. Establecer jerarquía (Parenting) para que se muevan juntos
            // El objeto actual se vuelve hijo del objeto al que se conectó
            var targetInteractable = targetPoint.GetComponentInParent<SnapInteractable>();
            if (targetInteractable != null)
            {
                transform.SetParent(targetInteractable.transform);
            }

            // 3. Alinear rotación (Opcional por ahora, mantenemos rotación del usuario)
            
            Debug.Log($"Snapped {name} (via {myPoint.name}) to {targetPoint.parent.name} (via {targetPoint.name})");
            
            // Notificar evento de Snap
            var interactable = targetPoint.GetComponentInParent<SnapInteractable>();
            if (interactable != null)
            {
                interactable.OnSnap?.Invoke(this);
            }
        }

        private bool IsCompatible(string nameA, string nameB)
        {
            bool aIsTop = nameA.Contains("Top");
            bool aIsBottom = nameA.Contains("Bottom");
            bool bIsTop = nameB.Contains("Top");
            bool bIsBottom = nameB.Contains("Bottom");

            // Solo permitir conexiones Top-Bottom o Bottom-Top
            if (aIsTop && bIsBottom) return true;
            if (aIsBottom && bIsTop) return true;
            
            // Si alguno de los dos no tiene etiquetas estándar, asumimos que no es un bloque Lego estándar
            // y permitimos la conexión por defecto (o podemos ser estrictos y retornar false)
            if (!aIsTop && !aIsBottom && !bIsTop && !bIsBottom) return true;

            return false;
        }

        private void OnDrawGizmos()
        {
            if (mainSnapPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(mainSnapPoint.position, searchRadius);
            }
        }
    }
}
