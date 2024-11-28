using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelSetupData
{
	public string ModelPath;
	public Vector3 pos;
    public Quaternion rot;

	public ModelSetupData()
    {
    }
}

[Serializable]
public class LightSetupData
{
    public Vector3 pos;
    public Quaternion rot;
    public Color color;
    public float intensity;
    public float range;
    public LightType type;
    public bool hasShadows;

    public LightSetupData()
    {
    }
}

// this is what's saved and loaded to a json file
[Serializable]
public class StrodeloSetupData
{
	public string EnvironmentMapPath;
	public List<ModelSetupData> Models;
	public List<LightSetupData> Lights;

    public StrodeloSetupData()
	{
	}
}
