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

public class Receiver : MonoBehaviour
{
    string savePath;// = Application.persistentDataPath + "/TransferDirectory/received.obj"; Can only do this in Start()
    int port = 8111;

    bool fileReadyFlag = false;

    public void ImportAndCreateMeshes(string filePath)
    {
        AssimpContext importer = new AssimpContext();
        Scene model = importer.ImportFile(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);

        foreach (var mesh in model.Meshes)
        {
            GameObject newObject = new GameObject(mesh.Name);
            MeshFilter meshFilter = newObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = newObject.AddComponent<MeshRenderer>();

            // Create and populate the unity mesh
            UnityEngine.Mesh unityMesh = new UnityEngine.Mesh();
            unityMesh.vertices = mesh.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
            unityMesh.normals = mesh.Normals.Select(n => new Vector3(n.X, n.Y, n.Z)).ToArray();
            unityMesh.uv = mesh.TextureCoordinateChannels[0].Select(uv => new Vector2(uv.X, uv.Y)).ToArray();
            unityMesh.triangles = mesh.GetIndices();

            // Assign the mesh to the mesh filter
            meshFilter.mesh = unityMesh;

            // Assign a material to the mesh renderer
            meshRenderer.material = new UnityEngine.Material(Shader.Find("Standard"));
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
        savePath = Application.persistentDataPath + "/TransferDirectory/received.obj"; // TODO: MAKE IT CREATE TransferDirectory IF IT DOESN'T EXIST!!
        await ReceiveFileAsync(savePath, port);
    }

    float timer = 0.0f;
    float interval = 8.0f; // This is the interval in seconds. 0.5 seconds means 2 times per second.

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime; // Time.deltaTime gives the time between the current and last frame

        if (timer > interval)
        {
            Debug.Log("twiddles thumbs");
            timer = 0.0f;
        }

        if (fileReadyFlag)
        {
            ImportAndCreateMeshes(savePath);
            fileReadyFlag = false;
        }
    }
}
