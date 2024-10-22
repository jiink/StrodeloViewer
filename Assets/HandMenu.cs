using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandMenu : MonoBehaviour
{
    public Transform LeftHandAnchor;
    public Transform RightHandAnchor;
    public Vector3 offset = new Vector3(0, 0.1f, 0);
    public TextMeshPro debugOutput;
    public StrodeloCore core;
    private Camera mainCamera;
    public Button placeOnSurfaceButton;

    void Start()
    {
        mainCamera = Camera.main;

        // Hook up the buttons to the core
        placeOnSurfaceButton.onClick.AddListener(core.PlaceOnSurfaceAct);
    }

    void Update()
    {
        // Which hand is the user looking at more?
        Vector3 dirToLeftHand = (LeftHandAnchor.position - mainCamera.transform.position).normalized;
        float dotL = Vector3.Dot(mainCamera.transform.forward, dirToLeftHand);
        Vector3 dirToRightHand = (RightHandAnchor.position - mainCamera.transform.position).normalized;
        float dotR = Vector3.Dot(mainCamera.transform.forward, dirToRightHand);
        if (dotL > dotR)
        {
            // Move object to somewhere just above the left hand
            transform.position = LeftHandAnchor.position + offset;
        }
        else
        {
            transform.position = RightHandAnchor.position + offset;
        }

        if (debugOutput != null)
        {
            debugOutput.text = $"dotL: {dotL}\ndotR: {dotR}";
        }
    }
}
