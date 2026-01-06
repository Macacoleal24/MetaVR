using UnityEngine;
using Meta.XR;
using UnityEngine.Android;
using System.Security.Cryptography;

public class RaycastPlacement : MonoBehaviour
{
    [SerializeField] private GameObject prefabToPlace;

    [SerializeField] private Transform rightcontroller;
    [SerializeField] private Transform leftcontroller;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private EnvironmentRaycastManager raycastManager;

    private const string SCENE_PERMISSION = "com.oculus.permission.USE_SCENE";

    private void Awake()
    {
        Permission.RequestUserPermission(SCENE_PERMISSION);
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) 
        {
            TrytoPlace(rightcontroller);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            TrytoPlace(leftcontroller);
        }
    }

    private void TrytoPlace(Transform controller)
    {
        Ray ray = new Ray(controller.position, controller.forward);

        if (raycastManager.Raycast(ray, out EnvironmentRaycastHit hit))
        {
            GameObject ObjectToPlace = Instantiate(prefabToPlace);

            ObjectToPlace.transform.position = hit.point;

            Vector3 up = hit.normal.normalized;
            Vector3 forward = Vector3.ProjectOnPlane(playerCamera.position - hit.point, up);
            ObjectToPlace.transform.rotation = Quaternion.LookRotation(forward, up);

            ObjectToPlace.transform.localScale = Random.Range(0.5f, 1f) * Vector3.one; 
        }
    }
}
