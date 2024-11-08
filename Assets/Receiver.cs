using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using Assimp;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Surfaces;

public class Receiver : MonoBehaviour
{
    public StrodeloCore core;
    string savePath;// = Application.persistentDataPath + "/TransferDirectory/received.obj"; Can only do this in Start()
    int port = 8111;
    private GameObject modelTemplatePrefab;
    private GameObject cubeVisualizerPrefab;
    public Text notificationText;

    bool fileReadyFlag = false;
    bool busy = false;

    public void ImportAndCreateMeshes(string filePath)
    {
        Debug.Log($"Importing file: {filePath}");
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
            selectableModel.Selected += core.OnModelSelected;

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
    }

    private UnityEngine.Mesh CreateCubeMesh(Vector3 size)
    {
        UnityEngine.Mesh mesh = new UnityEngine.Mesh();

        // Define vertices
        Vector3[] vertices = {
            new Vector3(-size.x, -size.y, -size.z) * 0.5f,
            new Vector3(size.x, -size.y, -size.z) * 0.5f,
            new Vector3(size.x, size.y, -size.z) * 0.5f,
            new Vector3(-size.x, size.y, -size.z) * 0.5f,
            new Vector3(-size.x, -size.y, size.z) * 0.5f,
            new Vector3(size.x, -size.y, size.z) * 0.5f,
            new Vector3(size.x, size.y, size.z) * 0.5f,
            new Vector3(-size.x, size.y, size.z) * 0.5f
        };

        // Define triangles
        int[] triangles = {
            0, 2, 1, 0, 3, 2,
            2, 3, 6, 3, 7, 6,
            0, 7, 3, 0, 4, 7,
            1, 6, 5, 1, 2, 6,
            4, 5, 6, 4, 6, 7,
            0, 1, 5, 0, 5, 4
        };

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Recalculate normals for proper lighting
        mesh.RecalculateNormals();

        return mesh;
    }


    public async Task ReceiveFileAsync(string savePath, int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        Debug.Log("Now listening on port " + port);
        listener.Start();

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var stream = client.GetStream();
            using (var output = File.Create(savePath))
            {
                await stream.CopyToAsync(output);
                Debug.Log("I got something! Time to import it");
                fileReadyFlag = true;

                NotifyUser("File received!");
            }
            stream.Close();
            client.Close();
            Debug.Log("Time to wait again!");
        }
    }

    private void NotifyUser(string v)
    {
        if (notificationText != null)
        {
            notificationText.text = v;
        }
        else
        {
            Debug.LogError("No notification text object found");
        }
        Debug.Log(v);
    }


    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("Hello, World!");
        modelTemplatePrefab = Resources.Load<GameObject>("LoadedModelTemplate");
        cubeVisualizerPrefab = Resources.Load<GameObject>("LineCube");

        string directoryPath = Path.Combine(Application.persistentDataPath, "TransferDirectory");
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Created directory: " +  directoryPath);
        }

       
        string uniqueFileName = "received_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".obj";
        savePath = Path.Combine(directoryPath, uniqueFileName);
        

        await ReceiveFileAsync(savePath, port);
    }


    // Update is called once per frame
    void Update()
    {
        if (fileReadyFlag && !busy)
        {
            busy = true;
            ImportAndCreateMeshes(savePath);
            busy = false;
            fileReadyFlag = false;
        }
    }
}
