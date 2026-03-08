using UnityEngine;
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
        public float searchRadius = 0.15f;

        private Grabbable _grabbable;
        private Rigidbody _rigidbody;
        private bool _isSnapped = false;
        private bool _isGrabbed = false;
        private SnapInteractable _currentSnapTarget;
        private SnapInteractable _currentHoverInteractable;
        private Transform _targetSnapPoint;

        private void Awake()
        {
            _grabbable = GetComponent<Grabbable>();
            _rigidbody = GetComponent<Rigidbody>();

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
                _isSnapped = false;
                _rigidbody.isKinematic = false;
                
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

            // Buscar interactables cercanos para Hover
            Collider[] colliders = Physics.OverlapSphere(mainSnapPoint.position, currentSearchRadius);
            SnapInteractable bestInteractable = null;
            float minDst = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.transform.root == transform.root) continue;

                var interactable = col.GetComponentInParent<SnapInteractable>();
                if (interactable != null)
                {
                    // Buscar punto de snap con el radio de búsqueda actual (que puede ser mayor que el snapRadius interno)
                    Transform targetPoint = interactable.GetClosestSnapPoint(mainSnapPoint.position, currentSearchRadius);
                    
                    if (targetPoint != null)
                    {
                        float dst = Vector3.Distance(mainSnapPoint.position, targetPoint.position);
                        if (dst < minDst)
                        {
                            minDst = dst;
                            bestInteractable = interactable;
                        }
                    }
                }
            }

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

            // Buscar interactables cercanos
            Collider[] colliders = Physics.OverlapSphere(mainSnapPoint.position, searchRadius);
            Transform bestMatch = null;
            float minDst = float.MaxValue;

            foreach (var col in colliders)
            {
                // Ignorar colisión con uno mismo
                if (col.transform.root == transform.root) continue;

                var interactable = col.GetComponentInParent<SnapInteractable>();
                if (interactable != null)
                {
                    Transform targetPoint = interactable.GetClosestSnapPoint(mainSnapPoint.position);
                    if (targetPoint != null)
                    {
                        float dst = Vector3.Distance(mainSnapPoint.position, targetPoint.position);
                        if (dst < minDst)
                        {
                            minDst = dst;
                            bestMatch = targetPoint;
                            _currentSnapTarget = interactable;
                        }
                    }
                }
            }

            if (bestMatch != null)
            {
                PerformSnap(bestMatch);
            }
        }

        private void PerformSnap(Transform targetPoint)
        {
            _isSnapped = true;
            _rigidbody.isKinematic = true;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            // Calcular offset para alinear mainSnapPoint con targetPoint
            // Queremos que mainSnapPoint.position == targetPoint.position
            // Y alinear rotaciones (esto es simplificado, LEGO requiere rotaciones de 90 grados)
            
            // 1. Alinear posición
            Vector3 offset = transform.position - mainSnapPoint.position;
            transform.position = targetPoint.position + offset;

            // 2. Alinear rotación (Snap a 90 grados más cercano relativo al target)
            // Por ahora, simple snap de posición
            
            Debug.Log($"Snapped {name} to {targetPoint.parent.name}");
            
            // Notificar evento de Snap
            var interactable = targetPoint.GetComponentInParent<SnapInteractable>();
            if (interactable != null)
            {
                interactable.OnSnap?.Invoke(this);
            }
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
