using Meta.WitAi;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Oculus.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;
    public HandMenu handMenu;
    private TextMeshProUGUI instructionBoard;
    private GameObject selectedModel;
    private OVRCameraRig _cameraRig;
    public GameObject materialInspectorMenuPrefab;
    public LineRenderer laser;
    public RayInteractor rayInteractor;
    public GameObject fakeLoadedModelPrefab; // Just for debugging purposes

    private GameObject pointLightPrefab;
    private GameObject pointLight;

    private int debugNum = 0;

    enum ActionState
    {
        Idle,
        SelectingModelForSurface,
        SelectingSurface,
        SelectingModelForInspection,
        SelectingLightPosition,
        SelectingLightPower
    }
    private ActionState actionState = ActionState.Idle;

    void Start()
    {
        pointLightPrefab = Resources.Load<GameObject>("StrodeloPointLight");
        instructionBoard = handMenu.instructionBoard;
        _cameraRig = FindObjectOfType<OVRCameraRig>();
        GameObject receiverObject = Instantiate(receiverPrefab);
        Receiver receiver = receiverObject.GetComponent<Receiver>();
        if (receiver == null)
        {
            Debug.LogError("Receiver component not found on the instantiated prefab.");
            return;
        }
        receiver.core = this;

        laser.startWidth = 0.01f;
        laser.endWidth = 0.01f;
    }

    void Update()
    {
        if (actionState == ActionState.SelectingSurface)
        {
            var ray = rayInteractor.Ray;
            MRUKAnchor sceneAnchor = null;
            var positioningMethod = MRUK.PositioningMethod.DEFAULT;
            var bestPose = MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast(ray, Mathf.Infinity,
                            new LabelFilter(), out sceneAnchor, positioningMethod); // see MRUK sample
            if (bestPose.HasValue && sceneAnchor && selectedModel)
            {
                selectedModel.transform.position = bestPose.Value.position;
                selectedModel.transform.rotation = bestPose.Value.rotation;
            }
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.cyan;
            laser.endColor = Color.cyan;
            laser.enabled = true;
        }
        if (actionState == ActionState.SelectingModelForInspection ||
            actionState == ActionState.SelectingModelForSurface)
        {
            // Show laser to indicate it's waiting for a selection
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.red;
            laser.endColor = Color.red;
            laser.enabled = true;
        }
        else
        {
            laser.enabled = false; // don't need the laser
        }
        if (actionState == ActionState.SelectingLightPosition)
        {
            // make light follow hand
            pointLight.transform.position = _cameraRig.rightHandAnchor.position;
        }
        else if (actionState == ActionState.SelectingLightPower)
        {
            // Power is proportional to the distance from the pointLight to the hand.
            float dist = Vector3.Distance(pointLight.transform.position, _cameraRig.rightHandAnchor.position);
            float multiplier = 10f;
            pointLight.GetComponent<Light>().intensity = dist * multiplier;
        }
    }

    // TODO: right now we only reference the right hand rayinteractor. re-employ the use of this function but 
    // make it get the right rayinteractor to use.
    // Copied from SceneDebugger.cs (see MR utility kit samples)
    //    private Ray GetControllerRay()
    //{
    //    Vector3 rayOrigin;
    //    Vector3 rayDirection;
    //    if (OVRInput.activeControllerType == OVRInput.Controller.Touch
    //        || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
    //    {
    //        rayOrigin = _cameraRig.rightHandOnControllerAnchor.position;
    //        rayDirection = _cameraRig.rightHandOnControllerAnchor.forward;
    //    }
    //    else if (OVRInput.activeControllerType == OVRInput.Controller.LTouch)
    //    {
    //        rayOrigin = _cameraRig.leftHandOnControllerAnchor.position;
    //        rayDirection = _cameraRig.leftHandOnControllerAnchor.forward;
    //    }
    //    else // hands
    //    {
    //        rayOrigin = _cameraRig.rightHandAnchor.position;
    //        rayDirection = _cameraRig.rightHandAnchor.right * -1; // .forward goes the wrong way :v
    //    }

    //    return new Ray(rayOrigin, rayDirection);
    //}

    public void OnModelSelected(object sender, System.EventArgs e)
    {
        var model = sender as GameObject;
        if (model == null)
        {
            Debug.LogError("Model is null");
            return;
        }
        // before we change what the selectedModel is, set its visualizer line color back to normal.
        if (selectedModel != null)
        {
            var oldVis = selectedModel.GetComponent<SelectableModel>().GetVisualizerChild();
            var oldVisColorChanger = oldVis.GetComponent<ChildrenLineColorChanger>();
            oldVisColorChanger.LineColor = Color.white;
        }

        selectedModel = model;
        Debug.Log($"Selected model: {selectedModel.name}");

        // Enable its collider visualizer if it has one
        var vis = selectedModel.GetComponent<SelectableModel>().GetVisualizerChild();
        if (vis != null)
        {
            vis.SetActive(true);
            // Change its color
            var visColorChanger = vis.GetComponent<ChildrenLineColorChanger>();
            if (visColorChanger != null)
            {
                visColorChanger.LineColor = Color.green;
            }
        }

        if (actionState == ActionState.SelectingModelForSurface)
        {
            actionState = ActionState.SelectingSurface;
            SetInstruction("Select a surface to place the model on.");
        }
        else if (actionState == ActionState.SelectingModelForInspection)
        {
            actionState = ActionState.Idle;
            ClearInstruction();
            SpawnMaterialInspector(selectedModel);
        }
        // You happen to be pointing at the model when selecting a surface, so handle the click here.
        else if (actionState == ActionState.SelectingSurface)
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void PlaceOnSurfaceAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingModelForSurface;
            SetInstruction("Select a model to place on a surface.");
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void InspectMaterialAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingModelForInspection;
            SetInstruction("Select a model to inspect its material.");
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void SetInstruction(string instruction)
    {
        instructionBoard.text = instruction;
    }

    internal void ClearInstruction()
    {
        instructionBoard.text = "";
    }

    internal void DebugButtonPressed()
    {
        debugNum++;
        SpawnFakeLoadedModel();
    }

    internal GameObject SpawnMaterialInspector(GameObject inspectedObj)
    {
        var m = SpawnMenu(materialInspectorMenuPrefab);
        var materialInspector = m.GetComponent<MaterialInspectorMenu>();
        materialInspector.InspectedModel = inspectedObj;
        return m;
    }

    // Returns the spawned menu
    internal GameObject SpawnMenu(GameObject menuPrefab)
    {
        float spawnDistance = 0.5f;
        Transform userTransform = _cameraRig.centerEyeAnchor;
        Vector3 spawnPos = userTransform.position + userTransform.forward * spawnDistance;
        // face the menu towards the user
        Quaternion rotation = Quaternion.LookRotation(userTransform.position - spawnPos);
        GameObject menu = Instantiate(menuPrefab, spawnPos, rotation);
        return menu;
    }

    // Just for debugging purposes
    private void SpawnFakeLoadedModel()
    {
        var m = Instantiate(fakeLoadedModelPrefab);
        var cubeVisualizerPrefab = Resources.Load<GameObject>("LineCube");
        m.transform.position = _cameraRig.centerEyeAnchor.position + _cameraRig.centerEyeAnchor.forward * 0.3f;
        GameObject colliderVisualizer = Instantiate<GameObject>(cubeVisualizerPrefab);
        colliderVisualizer.tag = "SelectionVisualizer";
        colliderVisualizer.transform.SetParent(m.transform);
        var boxCollider = m.GetComponent<BoxCollider>();
        colliderVisualizer.transform.localPosition = boxCollider.center;
        colliderVisualizer.transform.localScale = boxCollider.size;
        colliderVisualizer.SetActive(false);
        SelectableModel selectableModel = m.GetComponent<SelectableModel>();
        selectableModel.Selected += OnModelSelected;
    }

    internal void AddPointLightAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingLightPosition;
            SetInstruction("Select a position to place a point light.");
            // Create light now and have it follow the hand until a certain hand pose is made
            pointLight = Instantiate(pointLightPrefab);
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    // Hand forms a certain pose
    public void HandPoseStrike()
    {
        // make the hand pose to place the light.
        // Light was already following hand in Update(), so this just makes it stop following the hand.
        if (actionState == ActionState.SelectingLightPosition)
        {
            // next, move posed hand away to define its power
            actionState = ActionState.SelectingLightPower;
        }
    }

    // Hand stops forming a certain pose
    public void HandPoseUnstrike()
    {
        if (actionState == ActionState.SelectingLightPower)
        {
            actionState = ActionState.Idle;
        }
    }
}
