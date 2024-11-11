using Oculus.Interaction.Input;
using System;
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
    public GameObject handMenuButtonPrefab; // This is a template that has its picture and label and action changed accordingly.
    public Transform buttonsParent;
    public TextMeshProUGUI instructionBoard;
    private Camera mainCamera;
    private GameObject _visual;

    public bool isLeftHandAvailableForMenu = false;
    public bool isRightHandAvailableForMenu = false;

    private HButtonEntry[] hButtonEntries;

    void Start()
    {
        _visual = transform.GetChild(0).gameObject;
        mainCamera = Camera.main;

        // Define data for all buttons
        hButtonEntries = new HButtonEntry[]
        {
            new("Place on Surface", null, StrodeloCore.Instance.PlaceOnSurfaceAct),
            new("Debug", null, StrodeloCore.Instance.DebugButtonPressed),
            new("Inspect Material", null, StrodeloCore.Instance.InspectMaterialAct),
            new("Add Point Light", null, StrodeloCore.Instance.AddPointLightAct),
            new("Delete Light", null, StrodeloCore.Instance.DeleteLightAct),
        };

        InitializeButtons(hButtonEntries, handMenuButtonPrefab, buttonsParent);
        // Hook up the buttons to the core
        //placeOnSurfaceButton.onClick.AddListener(StrodeloCore.Instance.PlaceOnSurfaceAct);
        //debugButton.onClick.AddListener(StrodeloCore.Instance.DebugButtonPressed);
        //inspectMaterialButton.onClick.AddListener(StrodeloCore.Instance.InspectMaterialAct);
        //addPointLightButton.onClick.AddListener(StrodeloCore.Instance.AddPointLightAct);
    }

    // Spawns the buttons and hooks up the events
    private void InitializeButtons(HButtonEntry[] hButtonEntries, GameObject btemplate, Transform parent)
    {
        for (int i = 0; i < hButtonEntries.Length; i++)
        {
            GameObject newButton = Instantiate(btemplate, parent);
            HandMenuButton hmb = newButton.GetComponent<HandMenuButton>();
            hmb.SetData(hButtonEntries[i]);
        }
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
        if (!isLeftHandAvailableForMenu)
        {
            scoreL = 0f;
        }
        if (!isRightHandAvailableForMenu)
        {
            scoreR = 0f;
        }
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

    // When a hand is available for menu, that means a handmenu is allowed to be over it.
    // we only want a hand to show the menu if it's open-palm and facing user.
    public void SetHandAvailableForMenu(bool leftHand, bool available)
    {
        if (leftHand == true)
        {
            isLeftHandAvailableForMenu = available;
        }
        else
        {
            isRightHandAvailableForMenu = available;
        }
    }

    // The following functions are so the SelectorUnityEventWrappers can call them.
    public void SetLeftHandAvailableForMenuFalse()
    {
        isLeftHandAvailableForMenu = false;
        Debug.Log("LF");
    }
    public void SetLeftHandAvailableForMenuTrue()
    {
        isLeftHandAvailableForMenu = true;
        Debug.Log("LT");
    }
    public void SetRightHandAvailableForMenuFalse()
    {
        isRightHandAvailableForMenu = false;
    }
    public void SetRightHandAvailableForMenuTrue()
    {
        isRightHandAvailableForMenu = true;
    }
}

struct HButtonEntry
{
    public string Name;
    public Sprite Icon;
    public Action OnClick;

    public HButtonEntry(string name, Sprite icon, Action onClick)
    {
        Name = name;
        Icon = icon;
        OnClick = onClick;
    }
}
