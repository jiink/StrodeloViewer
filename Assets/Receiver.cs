using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    bool fileReadyFlag = false;
    bool busy = false;

    public void ImportAndCreateMeshes(string filePath)
    {
        AssimpContext importer = new AssimpContext();
        Scene model = importer.ImportFile(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);

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

            GameObject colliderVisualizer = new GameObject("ColliderVisualizer");
            colliderVisualizer.tag = "SelectionVisualizer";
            colliderVisualizer.transform.SetParent(newObject.transform);
            colliderVisualizer.transform.localPosition = boxCollider.center;
            UnityEngine.Mesh cubeMesh = CreateCubeMesh(boxCollider.size);
            MeshFilter colliderMeshFilter = colliderVisualizer.AddComponent<MeshFilter>();
            colliderMeshFilter.mesh = cubeMesh;
            MeshRenderer colliderMeshRenderer = colliderVisualizer.AddComponent<MeshRenderer>();
            colliderMeshRenderer.material = new UnityEngine.Material(Shader.Find("Standard"));
            //colliderMeshRenderer.material.color = new Color(0.0f, 0.0f, 1.0f, 0.5f);
            // that visualzer is disabled by default
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

    // It's made of lines.
    private UnityEngine.Mesh CreateCubeFrameMeshL(Vector3 size)
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

        // Define edges (lines) for the frame
        int[] lines = {
            0, 1, 1, 2, 2, 3, 3, 0, // Bottom face
            4, 5, 5, 6, 6, 7, 7, 4, // Top face
            0, 4, 1, 5, 2, 6, 3, 7  // Vertical edges
        };

        // Assign vertices and lines to the mesh
        mesh.vertices = vertices;
        mesh.SetIndices(lines, MeshTopology.Lines, 0);

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
            }
            stream.Close();
            client.Close();
            Debug.Log("Time to wait again!");
        }
    }


    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("Hello, World!");
        modelTemplatePrefab = Resources.Load<GameObject>("LoadedModelTemplate");

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
