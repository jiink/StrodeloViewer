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
            MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = unityMesh;
            meshCollider.convex = true;
            ColliderSurface colliderSurface = newObject.GetComponent<ColliderSurface>(); // need to fill in the collider field
            colliderSurface.InjectCollider(meshCollider);

            // Place object in front of user
            const float spawnDistanceM = 0.3f;
            newObject.transform.position = UnityEngine.Camera.main.transform.position + (UnityEngine.Camera.main.transform.forward * spawnDistanceM);
        }
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
