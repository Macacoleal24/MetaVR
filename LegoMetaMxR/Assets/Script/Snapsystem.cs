using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Snapsystem : MonoBehaviour
{
    [Header("Manual Snap Points")]
    [Tooltip("Drag your TOP snap points (studs) here from the hierarchy.")]
    public List<Transform> topSnapPoints = new List<Transform>();

    [Tooltip("Drag your BOTTOM snap points (tubes/holes) here from the hierarchy.")]
    public List<Transform> bottomSnapPoints = new List<Transform>();

    [Header("Snap Settings")]
    [Tooltip("Maximum distance to trigger a snap.")]
    public float snapThreshold = 0.5f;

    [Tooltip("If true, it will try to snap every frame (good for testing, bad for performance/gameplay). Ideally call TrySnap() from your Grab script.")]
    public bool autoSnapInUpdate = false;

    [Header("Physics")]
    public bool setKinematicDuringSnap = true;
    public float separationEpsilon = 0.0f;

    // Cache to avoid FindObjectsOfType every frame if we optimize later
    // private static List<Snapsystem> allBricks = new List<Snapsystem>();

    private void OnEnable()
    {
        // allBricks.Add(this);
    }

    private void OnDisable()
    {
        // allBricks.Remove(this);
    }

    private void Update()
    {
        if (autoSnapInUpdate)
        {
            // Only snap if we are not already snapped? 
            // Or maybe just visualize it.
            // For now, let's just use it for testing if needed.
            TrySnap();
        }
    }

    /// <summary>
    /// Tries to snap this brick to the nearest valid brick in the scene.
    /// Call this when you release the brick (OnRelease / OnEndGrab).
    /// </summary>
    /// <returns>True if snapped, false otherwise.</returns>
    [ContextMenu("Try Snap Now")]
    public bool TrySnap()
    {
        if (bottomSnapPoints == null || bottomSnapPoints.Count == 0) return false;

        Snapsystem bestTargetBrick = null;
        Transform bestMyPoint = null;
        Transform bestTargetPoint = null;
        float minDistance = snapThreshold;

        // Find all other bricks
        Snapsystem[] allBricks = FindObjectsByType<Snapsystem>(FindObjectsSortMode.None);

        foreach (var otherBrick in allBricks)
        {
            if (otherBrick == this) continue;
            if (otherBrick.topSnapPoints == null || otherBrick.topSnapPoints.Count == 0) continue;

            // Check every combination of MY bottom points vs OTHER top points
            foreach (var myPoint in bottomSnapPoints)
            {
                foreach (var otherPoint in otherBrick.topSnapPoints)
                {
                    float dist = Vector3.Distance(myPoint.position, otherPoint.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestTargetBrick = otherBrick;
                        bestMyPoint = myPoint;
                        bestTargetPoint = otherPoint;
                    }
                }
            }
        }

        if (bestTargetBrick != null && bestMyPoint != null && bestTargetPoint != null)
        {
            SnapTo(bestTargetBrick, bestMyPoint, bestTargetPoint);
            return true;
        }

        return false;
    }

    private void SnapTo(Snapsystem targetBrick, Transform myConnector, Transform targetConnector)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        bool prevKinematic = false;
        if (rb != null && setKinematicDuringSnap)
        {
            prevKinematic = rb.isKinematic;
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Alinear normales (up) de los conectores
        Quaternion alignUp = Quaternion.FromToRotation(myConnector.up, targetConnector.up);
        transform.rotation = alignUp * transform.rotation;

        // Alinear torsión alrededor del eje up para que los ejes forward coincidan
        Vector3 newForward = alignUp * myConnector.forward;
        float twist = Vector3.SignedAngle(newForward, targetConnector.forward, targetConnector.up);
        transform.RotateAround(myConnector.position, targetConnector.up, twist);

        Vector3 delta = targetConnector.position - myConnector.position;
        transform.position += delta;

        if (rb != null && setKinematicDuringSnap)
        {
            rb.isKinematic = prevKinematic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log($"Snapped {name} to {targetBrick.name}");
    }

    // Helper to visualize connections in Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (topSnapPoints != null)
        {
            foreach (var p in topSnapPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.05f);
            }
        }

        Gizmos.color = Color.red;
        if (bottomSnapPoints != null)
        {
            foreach (var p in bottomSnapPoints)
            {
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.05f);
            }
        }
    }
}
