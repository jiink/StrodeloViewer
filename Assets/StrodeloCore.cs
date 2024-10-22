using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;
    private GameObject selectedModel;

    enum PlaceOnSurfaceState
    {
        Idle,
        SelectingModel,
        SelectingSurface
    }
    private PlaceOnSurfaceState placeOnSurfaceState = PlaceOnSurfaceState.Idle;

    void Start()
    {
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
    }

    internal void PlaceOnSurfaceAct()
    {
        switch (placeOnSurfaceState)
        {
            case PlaceOnSurfaceState.Idle:
                placeOnSurfaceState = PlaceOnSurfaceState.SelectingModel;
                break;
            case PlaceOnSurfaceState.SelectingModel:
                placeOnSurfaceState = PlaceOnSurfaceState.SelectingSurface;
                break;
            case PlaceOnSurfaceState.SelectingSurface:
                placeOnSurfaceState = PlaceOnSurfaceState.Idle;
                break;
        }
    }
}
