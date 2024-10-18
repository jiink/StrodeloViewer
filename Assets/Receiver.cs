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

            // Add a collider so rigidbody works
            // (Maybe needs to just be a bounding box if there's performance issues)
            MeshCollider meshCollider = newObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = unityMesh;
            meshCollider.convex = true; // Non-convex mesh colliders with non-kinematic rigidbodies is not supported

            // Add rigidbody to object
            Rigidbody rb = newObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.angularDrag = 100f; // Don't want it flying away
            rb.drag = 100f;

            // Add grabbable component and inject rigidbody
            Grabbable grabbable = newObject.AddComponent<Grabbable>();
            grabbable.InjectOptionalRigidbody(rb);

            // Add grabinteractable component 
            GrabInteractable grabInteractable = newObject.AddComponent<GrabInteractable>();
            grabInteractable.InjectOptionalPointableElement(grabbable);
            grabInteractable.InjectRigidbody(rb);

            // Add handgrabinteractable component
            HandGrabInteractable handGrabInteractable = newObject.AddComponent<HandGrabInteractable>();
            handGrabInteractable.InjectOptionalPointableElement(grabbable);
            handGrabInteractable.InjectRigidbody(rb);

            // Add component to handle selection
            SelectableModel selectableModel = newObject.AddComponent<SelectableModel>();
            selectableModel.Selected += core.OnModelSelected;

            // Add ColliderSurface component for ray interaction
            ColliderSurface colliderSurface = newObject.AddComponent<ColliderSurface>();
            colliderSurface.InjectCollider(meshCollider);

            // Add RayInteractable so it can be selected from distance
            RayInteractable rayInteractable = newObject.AddComponent<RayInteractable>();
            rayInteractable.InjectSurface(colliderSurface);

            // Add PointableUnityEventWrapper so we can listen to events (e.g. hover, select, etc.)
            PointableUnityEventWrapper pointableUnityEventWrapper = newObject.AddComponent<PointableUnityEventWrapper>();
            pointableUnityEventWrapper.InjectPointable(rayInteractable);
            //pointableUnityEventWrapper.WhenSelect.AddListener((PointerEvent e) => selectableModel.Select()); // THE PROBLEM

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
