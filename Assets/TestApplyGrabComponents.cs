using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class TestApplyGrabComponents : MonoBehaviour
{
    void Start()
    {
        // These components make it so you can grab the object with your hands or controllers.
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.angularDrag = 100f; // Don't want it flying away
        rb.drag = 100f;
        Grabbable grabbable = gameObject.AddComponent<Grabbable>();
        grabbable.InjectOptionalRigidbody(rb);
        GrabInteractable grabInteractable = gameObject.AddComponent<GrabInteractable>();
        grabInteractable.InjectOptionalPointableElement(grabbable);
        grabInteractable.InjectRigidbody(rb);
        HandGrabInteractable handGrabInteractable = gameObject.AddComponent<HandGrabInteractable>();
        handGrabInteractable.InjectOptionalPointableElement(grabbable);
        handGrabInteractable.InjectRigidbody(rb);
    }

    void Update()
    {
        
    }
}
