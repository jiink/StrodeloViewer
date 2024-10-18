using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;
    private GameObject selectedModel;

    void Start()
    {
        GameObject receiverObject = Instantiate(receiverPrefab);
        Receiver receiver = receiverObject.GetComponent<Receiver>();
        if (receiver == null)
        {
            Debug.LogError("Receiver component not found on the instantiated prefab.");
            return;
        }
        receiver.core = this;
    }

    void Update()
    {
        
    }

    public void OnModelSelected(object sender, System.EventArgs e)
    {
        var model = sender as GameObject;
        if (model == null)
        {
            Debug.LogError("Model is null");
            return;
        }

        selectedModel = model;
        Debug.Log($"Selected model: {selectedModel.name}");
    }
}
