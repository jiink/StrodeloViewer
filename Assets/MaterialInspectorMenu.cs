using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class MaterialInspectorMenu : MonoBehaviour
{
    private GameObject _inspectedModel;
    public GameObject InspectedModel
    {
        get
        {
            return _inspectedModel;
        }
        set
        {
            _inspectedModel = value;
            if (_inspectedModel != null)
            {
                gameObjectNameLabel.text = _inspectedModel.name;
                AddMaterialEntries(_inspectedModel);
            }
        }
    }
    public TextMeshProUGUI gameObjectNameLabel;
    public GameObject materialListingPrefab;
    public Transform materialListingsParent;

    void Start()
    {
        if (gameObjectNameLabel == null)
        {
            Debug.LogError("TextMeshProUGUI not assigned.");
            this.enabled = false;
            return;
        }
    }

    void Update()
    {

    }

    // Add all materials from the model to the menu
    void AddMaterialEntries(GameObject model)
    {
        // Remove what's already there
        foreach (Transform child in materialListingsParent)
        {
            Destroy(child.gameObject);
        }
        Renderer renderer = _inspectedModel.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materialsArray = renderer.materials;
            List<Material> materials = new List<Material>(materialsArray);
            // Process the materials list as needed
            for (int i = 0; i < materialsArray.Length; i++)
            {
                MaterialListing materialListing = Instantiate(materialListingPrefab, materialListingsParent).GetComponent<MaterialListing>();
                materialListing.MaterialNumber = i;
                materialListing.Material = materialsArray[i];
            }
        }
    }
}
