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
        AssetLoader.LoadModelFromFile(filePath, (assetLoaderContext) =>
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

            // Add collider surface and inject the collider
            ColliderSurface colliderSurface = child.gameObject.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(boxCollider);

            // Add event handling for selection
            SelectableModel selectableModel = child.gameObject.AddComponent<SelectableModel>();
            selectableModel.Selected += StrodeloCore.Instance.OnModelSelected;

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

        /*
        AssimpContext importer = new AssimpContext();
        Scene model;
        try
        {
            model = importer.ImportFile(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to import file: " + e.Message);
            return;
        }

        int counter = 0;
        foreach (var mesh in model.Meshes)
        {
            counter += 1;
            Debug.Log($"Making mesh {counter} of {model.Meshes.Count}");
            // Load a template and make the necessary changes (e.g. the mesh)
            GameObject newObject = Instantiate(modelTemplatePrefab);
            newObject.name = mesh.Name;
            MeshFilter meshFilter = newObject.GetComponent<MeshFilter>();

            // Create and populate the unity mesh
            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();
            unityMesh.vertices = mesh.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            unityMesh.normals = mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z)).ToArray();
            unityMesh.uv = mesh.TextureCoordinateChannels[0].Select(uv => new Vector2(uv.X, uv.Y)).ToArray();
            unityMesh.triangles = mesh.GetIndices();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = unityMesh;

            // Assign a material to the mesh renderer
            //meshRenderer.material = new UnityEngine.Material(Shader.Find("Standard"));

            // Need to create a collider. Not included in template because it depends on the shape of the model being loaded
            //MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
            //meshCollider.sharedMesh = unityMesh;
            //meshCollider.convex = true;
            BoxCollider boxCollider = newObject.AddComponent<BoxCollider>();
            Bounds meshBounds = unityMesh.bounds;
            boxCollider.center = meshBounds.center;
            boxCollider.size = meshBounds.size;

            // need to fill in the collider field
            ColliderSurface colliderSurface = newObject.GetComponent<ColliderSurface>();
            colliderSurface.InjectCollider(boxCollider);

            // Have the "core" listen to the selection events
            SelectableModel selectableModel = newObject.GetComponent<SelectableModel>();
            selectableModel.Selected += StrodeloCore.Instance.OnModelSelected;

            // Place object in front of user
            const float spawnDistanceM = 0.3f;
            newObject.transform.position = UnityEngine.Camera.main.transform.position + (UnityEngine.Camera.main.transform.forward * spawnDistanceM);

            GameObject colliderVisualizer = Instantiate<GameObject>(cubeVisualizerPrefab);
            colliderVisualizer.tag = "SelectionVisualizer";
            colliderVisualizer.transform.SetParent(newObject.transform);
            colliderVisualizer.transform.localPosition = boxCollider.center;
            // Match bounds by scaling
            colliderVisualizer.transform.localScale = boxCollider.size;
            colliderVisualizer.SetActive(false);
        }
        */
    

    void Start()
    {
        modelTemplatePrefab = Resources.Load<GameObject>("LoadedModelTemplate");
        cubeVisualizerPrefab = Resources.Load<GameObject>("LineCube");
    }

    void Update()
    {
        
    }
}
