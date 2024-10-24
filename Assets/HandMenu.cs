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
    public Button placeOnSurfaceButton;
    public Button debugButton;
    public Button inspectMaterialButton;
    private Camera mainCamera;
    private GameObject _visual;

    void Start()
    {
        _visual = transform.GetChild(0).gameObject;
        mainCamera = Camera.main;

        // Hook up the buttons to the core
        placeOnSurfaceButton.onClick.AddListener(core.PlaceOnSurfaceAct);
        debugButton.onClick.AddListener(core.DebugButtonPressed);
        inspectMaterialButton.onClick.AddListener(core.InspectMaterialAct);
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
        var maxDot = Mathf.Max(dotL, dotR);
        if (maxDot < 0.75f)
        {
            // Hide the menu if the user is looking away from both hands
            _visual.SetActive(false);
        }
        else
        {
            _visual.SetActive(true);
        }

        if (debugOutput != null)
        {
            debugOutput.text = $"dotL: {dotL}\ndotR: {dotR}";
        }
    }
}
