using Meta.WitAi;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;
    public TextMeshProUGUI instructionBoard;
    private GameObject selectedModel;
    private OVRCameraRig _cameraRig;
    public GameObject materialInspectorMenuPrefab;
    private LineRenderer laser;
    public RayInteractor rayInteractor; 

    private int debugNum = 0;

    enum ActionState
    {
        Idle,
        SelectingModelForSurface,
        SelectingSurface,
        SelectingModelForInspection
    }
    private ActionState actionState = ActionState.Idle;

    void Start()
    {
        _cameraRig = FindObjectOfType<OVRCameraRig>();
        GameObject receiverObject = Instantiate(receiverPrefab);
        Receiver receiver = receiverObject.GetComponent<Receiver>();
        if (receiver == null)
        {
            Debug.LogError("Receiver component not found on the instantiated prefab.");
            return;
        }
        receiver.core = this;

        laser = gameObject.AddComponent<LineRenderer>();
        laser.startWidth = 0.01f;
        laser.endWidth = 0.01f;
        laser.material = new Material(Shader.Find("Unlit/Color"));
        laser.startColor = Color.red;
    }

    void Update()
    {
        if (actionState == ActionState.SelectingSurface)
        {
            // TODO: either make the room clickable to set placeonsurface state back to idle, or make model line up a lot better with the UI ray :|
            var ray = GetControllerRay();
            MRUKAnchor sceneAnchor = null;
            var positioningMethod = MRUK.PositioningMethod.DEFAULT;
            var bestPose = MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast(ray, Mathf.Infinity,
                            new LabelFilter(), out sceneAnchor, positioningMethod); // see MRUK sample
            if (bestPose.HasValue && sceneAnchor && selectedModel)
            {
                selectedModel.transform.position = bestPose.Value.position;
                selectedModel.transform.rotation = bestPose.Value.rotation;
                // TODO: Adjust the position so the model doesn't clip into the surface (using bounding box). or get the physics to do it somehow.
                //var bounds = selectedModel.GetComponent<MeshFilter>().mesh.bounds;
                //var hit = new RaycastHit();
                //MRUK.Instance?.GetCurrentRoom()?.Raycast(ray, Mathf.Infinity, out hit, out _);
                //var normal = hit.normal;
                //var mSize = bounds.size;
                //var offset = normal * mSize.y / 2;
                //selectedModel.transform.position += offset;
            }
            //else
            //{
            //    Debug.LogError("Ray cast not working!");
            //    SetInstruction("ERR!");
            //}
        }
        if (actionState == ActionState.SelectingModelForInspection ||
            actionState == ActionState.SelectingModelForSurface)
        {
            // Show laser to indicate it's waiting for a selection
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.enabled = true;
        }
        else
        {
            laser.enabled = false; // don't need the laser
        }
    }

        // Copied from SceneDebugger.cs (see MR utility kit samples)
        private Ray GetControllerRay()
    {
        Vector3 rayOrigin;
        Vector3 rayDirection;
        if (OVRInput.activeControllerType == OVRInput.Controller.Touch
            || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
        {
            rayOrigin = _cameraRig.rightHandOnControllerAnchor.position;
            rayDirection = _cameraRig.rightHandOnControllerAnchor.forward;
        }
        else if (OVRInput.activeControllerType == OVRInput.Controller.LTouch)
        {
            rayOrigin = _cameraRig.leftHandOnControllerAnchor.position;
            rayDirection = _cameraRig.leftHandOnControllerAnchor.forward;
        }
        else // hands
        {
            rayOrigin = _cameraRig.rightHandAnchor.position;
            rayDirection = _cameraRig.rightHandAnchor.right * -1; // .forward goes the wrong way :v
        }

        return new Ray(rayOrigin, rayDirection);
    }

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
}
