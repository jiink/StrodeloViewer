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
using System.Threading.Tasks;

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

    // Returns the loaded model object once its done loading.
    public async Task<GameObject> ImportAndCreateMeshes(string filePath)
    {
        Debug.Log($"Importing file: {filePath}");

        var tcs = new TaskCompletionSource<GameObject>();
        AssetLoader.LoadModelFromFile(filePath,
            (assetLoaderContext) =>
            {
                Debug.Log("Model imported successfully!");


                GameObject loadedModel = assetLoaderContext.RootGameObject;

                if (loadedModel != null)
                {
                    ProcessLoadedModel(loadedModel, filePath);
                    StrodeloCore.Instance.SpawnNotification("Model loaded!");
                    tcs.SetResult(loadedModel);
                }
                else
                {
                    Debug.LogError("Failed to retrieve the loaded model.");
                    tcs.SetResult(null);
                }
            },
            (_) => { },
            (_, _) => { },
            (error) =>
            {
                Debug.LogError($"Failed to load model: {error}");
                StrodeloCore.Instance.SpawnNotification("Failed to load model: " + error);
                tcs.SetResult(null);
            });
        return await tcs.Task;
    }


    private void ProcessLoadedModel(GameObject loadedModel, string filePath)
    {
        if (loadedModel == null)
        {
            Debug.LogError("Loaded model is null!");
            return;
        }

        // Process the loaded model

        GameObject template = Instantiate(modelTemplatePrefab);
        loadedModel.transform.SetParent(template.transform);

        Bounds bounds = new(loadedModel.transform.position, Vector3.zero);
        // Calculate bounding box for collider of whole object including all children
        AddBoundsRecursively(loadedModel.transform, ref bounds);

        // Need collider for the whole object
        BoxCollider boxCollider = template.AddComponent<BoxCollider>();
        boxCollider.center = bounds.center - template.transform.position;
        boxCollider.size = bounds.size;

        // fill in collider field
        ColliderSurface colliderSurface = template.GetComponent<ColliderSurface>();
        colliderSurface.InjectCollider(boxCollider);

        // Have Core listen to wselection events
        SelectableModel selectableModel = template.GetComponent<SelectableModel>();
        selectableModel.Selected += StrodeloCore.Instance.OnModelSelected;
        selectableModel.modelFileSourcePath = filePath; // just so it can be saved later

        template.transform.position = UnityEngine.Camera.main.transform.position +
                                         (UnityEngine.Camera.main.transform.forward * 0.3f);
        //loadedModel.transform.position = template.transform.position;

        GameObject colliderVisualizer = Instantiate(cubeVisualizerPrefab);
        colliderVisualizer.tag = "SelectionVisualizer";
        colliderVisualizer.transform.SetParent(template.transform);
        colliderVisualizer.transform.localPosition = boxCollider.center;
        colliderVisualizer.transform.localScale = boxCollider.size;
        colliderVisualizer.SetActive(false);
    }

    private void AddBoundsRecursively(Transform transform, ref Bounds bounds)
    {
        foreach (Transform child in transform)
        {
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                Debug.Log("Hit!");
                bounds.Encapsulate(meshRenderer.bounds);
            }
            // Recursively call this method for each child
            AddBoundsRecursively(child, ref bounds);
        }
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
