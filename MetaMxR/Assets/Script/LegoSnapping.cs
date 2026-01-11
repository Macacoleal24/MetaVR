using UnityEngine;

public class LegoSnapping : MonoBehaviour
{
    // Define el radio dentro del cual buscar un punto de enganche (Snapping Point)
    public float snapRadius = 0.05f;

    // Referencia al punto de enganche (Snapping Point) de esta pieza
    public Transform[] snapPoints;

    // Cached component for performance
    private Rigidbody cachedRigidbody;

    private void OnEnable()
    {
        // Cache the Rigidbody component
        if (cachedRigidbody == null)
            cachedRigidbody = GetComponent<Rigidbody>();
    }

    // Llamado cuando la pieza se suelta
    public void OnBlockReleased()
    {
        // Validate snap points
        if (snapPoints == null || snapPoints.Length == 0)
            return;

        // 1. Iterar a través de todos los puntos de enganche de esta pieza
        foreach (Transform ownPoint in snapPoints)
        {
            if (ownPoint == null)
                continue;

            // 2. Buscar otros puntos de enganche cercanos
            Collider[] colliders = Physics.OverlapSphere(ownPoint.position, snapRadius);

            foreach (Collider col in colliders)
            {
                if (col == null)
                    continue;

                // Asegúrate de que no seas tú mismo y de que tiene el script de enganche
                LegoSnapping otherBlock = col.GetComponentInParent<LegoSnapping>();
                if (otherBlock != null && otherBlock.gameObject != gameObject)
                {
                    // Encuentra el punto de enganche más cercano en el otro bloque
                    Transform closestOtherPoint = GetClosestSnapPoint(ownPoint.position, otherBlock);

                    if (closestOtherPoint != null)
                    {
                        // 3. Realizar el enganche
                        SnapBlock(ownPoint, closestOtherPoint);
                        return; // Enganche exitoso, terminamos la búsqueda
                    }
                }
            }
        }
    }

    private Transform GetClosestSnapPoint(Vector3 targetPosition, LegoSnapping otherBlock)
    {
        if (otherBlock.snapPoints == null || otherBlock.snapPoints.Length == 0)
            return null;

        Transform closestPoint = null;
        float closestDistance = float.MaxValue;

        // Iterate through all snap points in the other block and find the closest one
        foreach (Transform snapPoint in otherBlock.snapPoints)
        {
            if (snapPoint == null)
                continue;

            float distance = Vector3.Distance(targetPosition, snapPoint.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = snapPoint;
            }
        }

        return closestPoint;
    }

    private void SnapBlock(Transform ownPoint, Transform otherPoint)
    {
        // 4. Mover y rotar la pieza para que los puntos coincidan

        // Desactivar temporalmente la física
        if (cachedRigidbody != null)
            cachedRigidbody.isKinematic = true;

        // Calcular la rotación necesaria.
        // Las piezas de LEGO generalmente no cambian de orientación (solo rotación en Y), 
        // pero la rotación de acoplamiento es compleja (depende de la cara que engancha).

        // Mover la pieza a la posición correcta: 
        // Posición deseada = Posición del punto enganchado + (Vector del punto propio al punto enganchado)
        Vector3 offset = otherPoint.position - ownPoint.position;
        transform.position += offset;

        // Opcional: Anclar la pieza al padre (esto crea una construcción única)
        transform.SetParent(otherPoint.root);
    }
}