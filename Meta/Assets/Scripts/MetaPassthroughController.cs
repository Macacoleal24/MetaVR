using UnityEngine;
// using UnityEngine.XR.ARFoundation; // Si usas ARFoundation (No necesario para Meta XR Core SDK)
using UnityEngine.XR.Management; // Para gestión básica

// Nota: Este script asume que tienes el paquete 'Oculus XR Plugin' instalado.
// Si usas el 'Meta XR Core SDK', la API es ligeramente diferente (OVRPassthroughLayer).

public class MetaPassthroughController : MonoBehaviour
{
    [Tooltip("Asigna aquí tu OVRCameraRig o XR Origin si usas el plugin de Oculus.")]
    public GameObject xrRig;

    void Start()
    {
        // 1. Configurar la cámara para que sea transparente
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0, 0, 0, 0); // Transparente
        }

        // 2. Activar Passthrough (si usas OVRManager del SDK de Meta)
        // OVRManager.instance.isInsightPassthroughEnabled = true;
        
        Debug.Log("✅ Passthrough configurado: Cámara en modo transparente.");
    }

    // Método para alternar Passthrough (útil para pruebas)
    public void TogglePassthrough()
    {
        // Implementación depende del SDK que elijas (Oculus XR Plugin vs Meta XR Core SDK)
        // Ejemplo genérico para cambiar el fondo:
        Camera.main.backgroundColor = Camera.main.backgroundColor.a == 0 ? Color.black : new Color(0,0,0,0);
    }
}
