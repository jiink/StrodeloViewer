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
using System.IO;

public class ModelLoader : MonoBehaviour
{
    private GameObject modelTemplatePrefab;
    private GameObject cubeVisualizerPrefab;
    private UnityEngine.Material occlusionFriendlyLit;
    private GameObject modelLoadingIndicator;

    private GameObject currentModelLoadingIndicator;

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

        // Spawn a model loading indicator that will be destroyed when the model is done loading
        currentModelLoadingIndicator = Instantiate(modelLoadingIndicator);
        currentModelLoadingIndicator.transform.position = UnityEngine.Camera.main.transform.position +
                                         (UnityEngine.Camera.main.transform.forward * 0.3f);
        currentModelLoadingIndicator.GetComponent<ModelLoadingIndicator>().ModelName = Path.GetFileName(filePath);

        var tcs = new TaskCompletionSource<GameObject>();
        AssetLoader.LoadModelFromFile(filePath,
            (assetLoaderContext) => // when model is done loading
            {
                Debug.Log("Model imported successfully!");
                GameObject loadedModel = assetLoaderContext.RootGameObject;
                if (loadedModel != null)
                {
                    var loadedRoot = ProcessLoadedModel(loadedModel, filePath);
                    Destroy(currentModelLoadingIndicator);
                    StrodeloCore.Instance.SpawnNotification("Model loaded!");
                    tcs.SetResult(loadedRoot);
                }
                else
                {
                    Debug.LogError("Failed to retrieve the loaded model.");
                    Destroy(currentModelLoadingIndicator);
                    tcs.SetResult(null);
                }
            },
            (assetLoaderContext) => // when model's materials are done loading
            {
                ReplaceMaterialsRecursively(assetLoaderContext.RootGameObject.transform, occlusionFriendlyLit);
            },
            (_, _) => { },
            (error) =>
            {
                Debug.LogError($"Failed to load model: {error}");
                StrodeloCore.Instance.SpawnNotification("Failed to load model: " + error);
                Destroy(currentModelLoadingIndicator);
                tcs.SetResult(null);
            });
        return await tcs.Task;
    }


    private GameObject ProcessLoadedModel(GameObject loadedModel, string filePath)
    {
        if (loadedModel == null)
        {
            Debug.LogError("Loaded model is null!");
            return null;
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

        return template;
    }

    private void ReplaceMaterialsRecursively(Transform transform, UnityEngine.Material newMaterial)
    {
        MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // copy the materials array into a new array
            int numMats = meshRenderer.materials.Length;
            if (numMats == 0)
            {
                Debug.LogError("No materials found on mesh renderer!");
                return;
            }
            UnityEngine.Material[] newMats = new UnityEngine.Material[numMats];
            Debug.Log($"Number of materials: {numMats}");
            for (int i = 0; i < numMats; i++)
            {
                UnityEngine.Material oldMat = meshRenderer.materials[i];
                UnityEngine.Material newMat = new(newMaterial);
                // transfer properties
                newMat.CopyPropertiesFromMaterial(oldMat);
                newMats[i] = newMat;
            }
            meshRenderer.materials = newMats;
        }
        // Recursively call this method for each child
        foreach (Transform child in transform)
        {
            ReplaceMaterialsRecursively(child, newMaterial);
        }
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
        occlusionFriendlyLit = Resources.Load<UnityEngine.Material>("OcclusionCompatibleLit");
        modelLoadingIndicator = Resources.Load<GameObject>("ModelLoadingIndicator");
    }

    void Update()
    {

    }
}
