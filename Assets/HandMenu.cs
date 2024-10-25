using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandMenu : MonoBehaviour
{
    public Transform LeftHandAnchor;
    public Transform RightHandAnchor;
    public Hand leftHand;
    public Hand rightHand;
    public float distFromPalm = 0.15f;
    public float distFromFace = 0.1f;
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
        // See if palm is facing user
        leftHand.GetJointPose(HandJointId.HandPinky0, out Pose leftPalmPose);
        rightHand.GetJointPose(HandJointId.HandPinky0, out Pose rightPalmPose);
        float palmDotL = Vector3.Dot(mainCamera.transform.forward, leftPalmPose.up * -1f);
        float palmDotR = Vector3.Dot(mainCamera.transform.forward, rightPalmPose.up);
        float scoreL = dotL + palmDotL;
        float scoreR = dotR + palmDotR;
        Vector3 targetPos;
        if (scoreL > scoreR)
        {
            // Move object to in front of the left palm, offset away from it
            targetPos = leftPalmPose.position + leftPalmPose.up * distFromPalm;
            // Also offset it away from the face cause its too close
            targetPos += mainCamera.transform.forward * distFromFace;
        }
        else
        {
            targetPos = rightPalmPose.position + (rightPalmPose.up * -1f) * distFromPalm;
            targetPos += mainCamera.transform.forward * distFromFace;
        }
        float maxScore = Mathf.Max(scoreL, scoreR);
        // TODO: make the menus not show up if the hand is not in an open palm pose.
        // otherwise the menu is annoying showing up when grabbing objects.
        if (maxScore < 0.7f)
        {
            // Hide the menu if the user is looking away from both hands
            _visual.SetActive(false);
        }
        else
        {
            _visual.SetActive(true);
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.1f);
        }

        if (debugOutput != null)
        {
            debugOutput.text = $"dotL: {dotL}\ndotR: {dotR}";
        }
    }
}
