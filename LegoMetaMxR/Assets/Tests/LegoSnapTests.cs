using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using LegoMetaMxR;

public class LegoSnapTests
{
    private GameObject handObj;
    private GameObject targetObj;
    private SnapInteractor interactor;
    private SnapInteractable interactable;

    [SetUp]
    public void Setup()
    {
        // Setup Hand (Interactor)
        handObj = new GameObject("Hand");
        var rb = handObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        // Mock Grabbable dependency (might need a real component or mock if possible, 
        // but SnapInteractor requires it. We'll add it but not use it actively)
        handObj.AddComponent<Oculus.Interaction.Grabbable>(); 
        interactor = handObj.AddComponent<SnapInteractor>();

        // Setup Target (Interactable)
        targetObj = new GameObject("Target");
        targetObj.AddComponent<BoxCollider>().size = Vector3.one; // For OverlapSphere
        interactable = targetObj.AddComponent<SnapInteractable>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(handObj);
        Object.Destroy(targetObj);
    }

    [UnityTest]
    public IEnumerator Snap_RespectsPolarity_TopToBottom()
    {
        // 1. Setup Points
        // Hand has Connect_Top
        var handPoint = new GameObject("Connect_Top").transform;
        handPoint.SetParent(handObj.transform);
        handPoint.localPosition = Vector3.zero;
        interactor.mainSnapPoint = handPoint; // Manually assign or let Awake find it

        // Target has Connect_Top (Incompatible) and Connect_Bottom (Compatible)
        var targetTop = new GameObject("Connect_Top").transform;
        targetTop.SetParent(targetObj.transform);
        targetTop.localPosition = Vector3.zero; // Same position

        var targetBottom = new GameObject("Connect_Bottom").transform;
        targetBottom.SetParent(targetObj.transform);
        targetBottom.localPosition = new Vector3(0, -0.04f, 0); // Within 0.05f radius

        // Update Interactable list manually or let Awake handle it
        interactable.snapPoints.Add(targetTop);
        interactable.snapPoints.Add(targetBottom);

        // 2. Position Hand close to Target Top (Incompatible)
        handObj.transform.position = targetObj.transform.position; // Overlapping Top points

        // 3. Simulate Grab state via Reflection
        var grabField = typeof(SnapInteractor).GetField("_isGrabbed", BindingFlags.NonPublic | BindingFlags.Instance);
        grabField.SetValue(interactor, true);

        // 4. Wait for Physics and Update
        yield return new WaitForFixedUpdate();
        yield return null;

        // 4.5. Simulate Release (TrySnap) to trigger PerformSnap
        var trySnapMethod = typeof(SnapInteractor).GetMethod("TrySnap", BindingFlags.NonPublic | BindingFlags.Instance);
        trySnapMethod.Invoke(interactor, null);

        // 5. Assert
        // Should ignore Top (incompatible) and find Bottom (compatible) if within range
        // Or if Bottom is too far, find nothing.
        // Let's make Bottom close enough (0.1f is within default 0.15f radius)
        
        Assert.IsNotNull(interactor.BestTargetPoint, "Should find a valid snap point");
        Assert.AreEqual("Connect_Bottom", interactor.BestTargetPoint.name, "Should pick Bottom point due to polarity");
        Assert.AreNotEqual("Connect_Top", interactor.BestTargetPoint.name, "Should NOT pick Top point (incompatible)");

        // Object should be snapped to targetBottom.position relative to targetObj (parent)
        // targetBottom.localPosition is (0, -0.04f, 0)
        // Since handPoint is at (0,0,0) of handObj, handObj should align exactly with targetBottom
        Assert.AreEqual(new Vector3(0, -0.04f, 0), handObj.transform.localPosition, "Object should be snapped to local position of the target point");
        Assert.AreEqual(targetObj.transform, handObj.transform.parent, "Object should become child of the target interactable");
        Assert.IsTrue(handObj.GetComponent<Rigidbody>().isKinematic, "Object should be kinematic after snap");
    }

    [UnityTest]
    public IEnumerator Snap_Fails_When_Only_Incompatible_Available()
    {
        // 1. Setup Points
        var handPoint = new GameObject("Connect_Top").transform;
        handPoint.SetParent(handObj.transform);
        handPoint.localPosition = Vector3.zero;
        interactor.mainSnapPoint = handPoint;

        // Target ONLY has Connect_Top (Incompatible)
        var targetTop = new GameObject("Connect_Top").transform;
        targetTop.SetParent(targetObj.transform);
        targetTop.localPosition = Vector3.zero;
        interactable.snapPoints.Add(targetTop);

        // 2. Position
        handObj.transform.position = targetObj.transform.position;

        // 3. Simulate Grab
        var grabField = typeof(SnapInteractor).GetField("_isGrabbed", BindingFlags.NonPublic | BindingFlags.Instance);
        grabField.SetValue(interactor, true);

        // 4. Wait
        yield return new WaitForFixedUpdate();
        yield return null;

        // 5. Assert
            Assert.IsNull(interactor.BestTargetPoint, "Should NOT find any snap point if only incompatible ones exist");
            Assert.AreNotEqual(targetObj.transform, handObj.transform.parent, "Should NOT parent if snap failed");
        }
}
