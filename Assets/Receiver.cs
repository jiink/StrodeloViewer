using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;


public class Receiver : MonoBehaviour
{
    public string savePath;// = Application.persistentDataPath + "/TransferDirectory/received.obj"; Can only do this in Start()
    int port = 8111;

    bool fileReadyFlag = false;

    public event EventHandler FileReceived;
    private string directoryPath;

    public async Task ReceiveFileAsync(int port)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        Debug.Log("Now listening on port " + port);
        listener.Start();

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            var stream = client.GetStream();

            try
            {
                byte[] lengthBuffer = new byte[4];
                await stream.ReadAsync(lengthBuffer, 0, 4);
                int fileNameLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] fileNameBuffer = new byte[fileNameLength];
                await stream.ReadAsync(fileNameBuffer, 0, fileNameLength);
                string fileName = System.Text.Encoding.UTF8.GetString(fileNameBuffer);

                savePath = Path.Combine(directoryPath, fileName);

                using (var output = File.Create(savePath))
                {
                    await stream.CopyToAsync(output);
                    Debug.Log("I got something! Time to import it");
                    fileReadyFlag = true;

                    StrodeloCore.Instance.SpawnNotification("File received: " + fileName);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error receiving file: " + ex.Message);
            }
            finally
            {
                stream.Close();
                client.Close();
                Debug.Log("Time to wait again!");
            }
        }

    }

    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("Hello, World!");
        

        directoryPath = Path.Combine(Application.persistentDataPath, "TransferDirectory");
        
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("Created directory: " +  directoryPath);
        }

        await ReceiveFileAsync(port);
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
