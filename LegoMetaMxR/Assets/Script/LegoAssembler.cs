using UnityEngine;

public class LegoAssembler : MonoBehaviour
{
    public float snapThreshold = 0.02f; // Distancia para el "imán"
    public Transform mySocket; // Arrastra el Empty inferior aquí

    public void CheckForSnap()
    {
        // Buscamos pernos cercanos en un radio pequeño
        Collider[] hitColliders = Physics.OverlapSphere(mySocket.position, snapThreshold);
        foreach (var hit in hitColliders)
        {
            if (hit.CompareTag("LegoPlug"))
            {
                // 1. Alinear posición: El Socket debe coincidir con el Plug detectado
                transform.position = hit.transform.position + (transform.position - mySocket.position);

                // 2. Alinear rotación
                transform.rotation = hit.transform.rotation;

                // 3. Jerarquía (EL SECRETO): Hacer esta pieza hija de la que está abajo
                transform.SetParent(hit.transform.parent);

                // 4. Feedback: Sonido de click y vibración del control
                break;
            }
        }
    }
}