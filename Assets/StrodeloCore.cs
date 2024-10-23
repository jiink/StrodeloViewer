using Meta.WitAi;
using Meta.XR.MRUtilityKit;
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

    private int debugNum = 0;

    enum PlaceOnSurfaceState
    {
        Idle,
        SelectingModel,
        SelectingSurface
    }
    private PlaceOnSurfaceState placeOnSurfaceState = PlaceOnSurfaceState.Idle;

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
    }

    void Update()
    {
        if (placeOnSurfaceState == PlaceOnSurfaceState.SelectingSurface)
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

                SetInstruction("Select a surface to place the model on.");
            }
            else
            {
                Debug.LogError("Ray cast not working!");
                SetInstruction("ERR!");
            }
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
            //var rightHand = _cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>();
            //// can be null if running in Editor with Meta Linq app and the headset is put off
            //if (rightHand != null)
            //{
            //    rayOrigin = rightHand.PointerPose.position;
            //    //rayDirection = rightHand.PointerPose.forward; // this is actually coming out of the back of the hand
            //    rayDirection = rightHand.PointerPose.right;
            //}
            //else
            //{
            //    rayOrigin = _cameraRig.centerEyeAnchor.position;
            //    rayDirection = _cameraRig.centerEyeAnchor.forward;
            //}
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

        if (placeOnSurfaceState == PlaceOnSurfaceState.SelectingModel)
        {
            placeOnSurfaceState = PlaceOnSurfaceState.SelectingSurface;
            SetInstruction("Select a surface to place the model on.");
        }
        // You happen to be pointing at the model when selecting a surface, so handle the click here.
        else if (placeOnSurfaceState == PlaceOnSurfaceState.SelectingSurface)
        {
            placeOnSurfaceState = PlaceOnSurfaceState.Idle;
            ClearInstruction();
        }
    }

    internal void PlaceOnSurfaceAct()
    {
        if (placeOnSurfaceState == PlaceOnSurfaceState.Idle)
        {
            placeOnSurfaceState = PlaceOnSurfaceState.SelectingModel;
            SetInstruction("Select a model to place on a surface.");
        }
        else
        {
            placeOnSurfaceState = PlaceOnSurfaceState.Idle;
            ClearInstruction();
        }
        //switch (placeOnSurfaceState)
        //{
        //    case PlaceOnSurfaceState.Idle:
        //        placeOnSurfaceState = PlaceOnSurfaceState.SelectingModel;
        //        SetInstruction("Select a model to place on a surface.");
        //        break;
        //    case PlaceOnSurfaceState.SelectingModel:
        //        placeOnSurfaceState = PlaceOnSurfaceState.SelectingSurface;
        //        SetInstruction("Select a surface to place the model on.");
        //        break;
        //    case PlaceOnSurfaceState.SelectingSurface:
        //        placeOnSurfaceState = PlaceOnSurfaceState.Idle;
        //        ClearInstruction();
        //        break;
        //}
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
}
