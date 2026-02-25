# Configuración de Realidad Mixta (Passthrough) para Meta Quest 2

## 1. Instalación de Paquetes
Abre Unity y ve a **Window > Package Manager**.
1.  Asegúrate de estar en **Unity Registry**.
2.  Instala **XR Plugin Management**.
3.  Instala **Oculus XR Plugin**.
4.  *(Recomendado)* Ve a la **Asset Store** y busca **Meta XR All-in-One SDK**. Descárgalo e impórtalo. Te dará acceso a herramientas avanzadas como `OVRCameraRig`, `OVRPassthroughLayer` y `Hand Tracking`.

## 2. Configuración del Proyecto (Automática)
He creado una herramienta para ti.
1.  En la barra superior de Unity, verás un nuevo menú: **MetaVR**.
2.  Haz clic en **MetaVR > Setup Project for Quest 2**.
3.  Esto configurará automáticamente:
    *   Plataforma Android.
    *   Color Space Linear.
    *   Graphics API (OpenGLES3).
    *   Arquitectura ARM64.

## 3. Activar XR Plug-in Management
1.  Ve a **Edit > Project Settings > XR Plug-in Management**.
2.  En la pestaña de **Android** (icono del robot), marca la casilla **Oculus**.

## 4. Configurar la Escena para Passthrough (Realidad Mixta)
Para ver el mundo real a través de las gafas (Passthrough), necesitas:

### Opción A: Usando Meta XR Core SDK (Recomendado)
1.  Elimina la `Main Camera` por defecto.
2.  Busca el prefab `OVRCameraRig` (en `Oculus/VR/Prefabs`) y arrástralo a la escena.
3.  Selecciona el objeto `OVRCameraRig` en la jerarquía.
4.  En el inspector, busca el componente `OVRManager`:
    *   En **Quest Features**, cambia `Passthrough Support` a **Supported** o **Required**.
    *   Marca la casilla **Enable Passthrough**.
5.  Añade el componente `OVRPassthroughLayer` al objeto `OVRCameraRig`.
    *   Configura `Placement` a **Underlay**.
6.  Selecciona la cámara hija `CenterEyeAnchor` (dentro de `OVRCameraRig/TrackingSpace`):
    *   Cambia **Clear Flags** a **Solid Color**.
    *   Cambia **Background** a **Negro con Alpha 0** `(0,0,0,0)` (Transparente).

### Opción B: Usando solo XR Plugin Management (Básico)
1.  Selecciona tu `Main Camera`.
2.  Cambia **Clear Flags** a **Solid Color**.
3.  Cambia **Background** a **Negro con Alpha 0** `(0,0,0,0)`.
4.  Usa el script `Assets/Scripts/MetaPassthroughController.cs` para controlar la transparencia.
