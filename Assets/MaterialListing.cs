using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MaterialListing : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    private Material _material;
    public Material Material
    {
        get
        {
            return _material;
        }
        set
        {
            _material = value;
            UpdateLabel();
        }
    }
    private int _materialNumber;
    public int MaterialNumber
    {
        get
        {
            return _materialNumber;
        }
        set
        {
            _materialNumber = value;
            UpdateLabel();
        }
    }

    void UpdateLabel()
    {
        if (Material == null)
        {
            nameLabel.text = $"Material #{MaterialNumber}: (none)";
            return;
        }
        nameLabel.text = $"Material #{MaterialNumber}: {Material.name}";
    }

    public void SetMetallic(float value)
    {
        Material.SetFloat("_Metallic", value);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
