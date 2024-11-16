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
    public string savePath;// = Application.persistentDataPath + "/TransferDirectory/received.obj"; Can only do this in Start()
    int port = 8111;

    bool fileReadyFlag = false;

    public event EventHandler FileReceived;   

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

                StrodeloCore.Instance.SpawnNotification("File received!");
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
        if (fileReadyFlag)
        {
            fileReadyFlag = false;
            // whoever is listening will hear this and do something with the new file
            FileReceived?.Invoke(this, EventArgs.Empty); 
        }
    }
}
