using Assimp;
using Oculus.Interaction.Surfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.Interfaces;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class ModelLoader : MonoBehaviour
{
    private GameObject modelTemplatePrefab;
    private GameObject cubeVisualizerPrefab;

    internal void OnFileReceived(object sender, EventArgs e)
    {
        Receiver receiver = sender as Receiver;
        if (receiver == null)
        {
            Debug.LogError("Someone strange fired the event.");
            return;
        }
        string path = receiver.savePath;
        ImportAndCreateMeshes(path);
    }

    public void ImportAndCreateMeshes(string filePath)
    {
        Debug.Log($"Importing file: {filePath}");

        //var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        AssetLoader.LoadModelFromFile(filePath,
            (assetLoaderContext) =>
            {
                Debug.Log("Model imported successfully!");


                GameObject loadedModel = assetLoaderContext.RootGameObject;

                if (loadedModel != null)
                {

                    ProcessLoadedModel(loadedModel);
                }
                else
                {
                    Debug.LogError("Failed to retrieve the loaded model.");
                }
            },
            (_) => { },
            (_, _) => { },
            (error) =>
            {
                Debug.LogError($"Failed to load model: {error}");
            });
    }


    private void ProcessLoadedModel(GameObject loadedModel)
    {
        if (loadedModel == null)
        {
            Debug.LogError("Loaded model is null!");
            return;
        }

        // Process the loaded model
        loadedModel.transform.position = UnityEngine.Camera.main.transform.position +
                                         (UnityEngine.Camera.main.transform.forward * 0.3f);

        // Loop through all child objects (meshes) and apply additional processing
        foreach (Transform child in loadedModel.transform)
        {
            // Example: Add box colliders to each child
            BoxCollider boxCollider = child.gameObject.AddComponent<BoxCollider>();
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Bounds bounds = meshRenderer.bounds;
                boxCollider.center = bounds.center - child.position;
                boxCollider.size = bounds.size;
            }

            // Rigidbody
            Rigidbody rigidbody = child.gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.useGravity = false;
            rigidbody.drag = 100;
            rigidbody.angularDrag = 100;

            // Grabbable
            Grabbable grabbable = child.gameObject.AddComponent<Grabbable>();
            grabbable.InjectOptionalRigidbody(rigidbody);
            GrabInteractable grabInteractable = child.gameObject.AddComponent<GrabInteractable>();
            grabInteractable.InjectRigidbody(rigidbody);
            HandGrabInteractable handGrabInteractable = child.gameObject.AddComponent<HandGrabInteractable>();
            handGrabInteractable.InjectOptionalPointableElement(grabbable);
            handGrabInteractable.InjectRigidbody(rigidbody);

            // Add collider surface and inject the collider
            ColliderSurface colliderSurface = child.gameObject.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(boxCollider);

            // RayInteractable
            RayInteractable rayInteractable = child.gameObject.AddComponent<RayInteractable>();
            rayInteractable.InjectSurface(colliderSurface);

            // Add event handling for selection
            SelectableModel selectableModel = child.gameObject.AddComponent<SelectableModel>();
            selectableModel.Selected += StrodeloCore.Instance.OnModelSelected;
            PointableUnityEventWrapper pointableUnityEventWrapper = child.gameObject.AddComponent<PointableUnityEventWrapper>();
            pointableUnityEventWrapper.InjectPointable(rayInteractable);
            //pointableUnityEventWrapper.WhenHover.AddListener((_) => { selectableModel.ShowVisual(); });
            //pointableUnityEventWrapper.WhenUnhover.AddListener((_) => { selectableModel.HideVisual(); });
            //pointableUnityEventWrapper.WhenSelect.AddListener((_) => { selectableModel.Select(); });
            //pointableUnityEventWrapper.WhenHover.AddListener(delegate { Debug.Log("shitted mysefl"); });
            //pointableUnityEventWrapper.WhenUnhover.AddListener(delegate { Debug.Log("unshitted mysefl"); });
            //pointableUnityEventWrapper.WhenSelect.AddListener((_) => { Debug.Log("wiped"); });
            //pointableUnityEventWrapper.WhenUnselect.AddListener((_) => { });
            //pointableUnityEventWrapper.WhenMove.AddListener((_) => { });
            //pointableUnityEventWrapper.WhenCancel.AddListener((_) => { });

            // Create visualizer for collider
            GameObject colliderVisualizer = Instantiate(cubeVisualizerPrefab);
            colliderVisualizer.tag = "SelectionVisualizer";
            colliderVisualizer.transform.SetParent(child);
            colliderVisualizer.transform.localPosition = boxCollider.center;
            colliderVisualizer.transform.localScale = boxCollider.size;
            colliderVisualizer.SetActive(false);
        }

        // Optionally, you can apply a material or shader to the loaded model
        Debug.Log("Model processing complete.");
    }
    void Start()
    {
        modelTemplatePrefab = Resources.Load<GameObject>("LoadedModelTemplate");
        cubeVisualizerPrefab = Resources.Load<GameObject>("LineCube");
    }

    void Update()
    {

    }
}
