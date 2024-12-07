using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class MaterialInspectorMenu : MonoBehaviour
{
    private GameObject _inspectedObject;
    public GameObject InspectedObject
    {
        get
        {
            return _inspectedObject;
        }
        set
        {
            _inspectedObject = value;
            if (_inspectedObject != null)
            {
                gameObjectNameLabel.text = _inspectedObject.name;
                AddMaterialEntries(_inspectedObject);
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

    // Add all materials from the object to the menu
    void AddMaterialEntries(GameObject model)
    {
        // Clean existing entries
        foreach (Transform child in materialListingsParent)
        {
            Destroy(child.gameObject);
        }
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Count() > 0)
        {
            foreach (Renderer renderer in renderers)
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
}
