using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assimp;
using System.Linq;

public class Importer : MonoBehaviour
{
    void Start()
    {
        AssimpContext importer = new AssimpContext();
        Scene model = importer.ImportFile("ReceivedFiles\\received.obj", PostProcessPreset.TargetRealTimeMaximumQuality);

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
