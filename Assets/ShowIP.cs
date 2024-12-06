using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class ShowIP : MonoBehaviour
{
    TextMeshProUGUI textMeshPro;

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>(); // Get the TextMeshPro component
        StartCoroutine(UpdateIPCoroutine());
    }

    // Coroutine to update the IP address every 10 seconds
    IEnumerator UpdateIPCoroutine()
    {
        while (true)
        {
            UpdateIP();
            yield return new WaitForSeconds(10f);
        }
    }

    // Method to update the IP address
    void UpdateIP()
    {
        string localIP = "???.???.???.???";
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break;
            }
        }
        textMeshPro.text = localIP;
    }
}
