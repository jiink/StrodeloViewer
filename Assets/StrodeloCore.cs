using Meta.WitAi;
using Meta.XR.EnvironmentDepth;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Oculus.Platform;
using Superla.RadianceHDR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;
using TMPro;
using UnityEngine;

public class StrodeloCore : MonoBehaviour
{
    public GameObject receiverPrefab;
    public GameObject modelLoaderPrefab;
    public HandMenu handMenu;
    private TextMeshProUGUI instructionBoard;
    private GameObject selectedModel;
    private OVRCameraRig _cameraRig;
    public GameObject materialInspectorMenuPrefab;
    public LineRenderer laser;
    public RayInteractor rayInteractor;
    public GameObject fakeLoadedModelPrefab; // Just for debugging purposes
    public SkinnedMeshRenderer leftHandRenderer; // So its material can be changed when toggling occlusion
    public SkinnedMeshRenderer rightHandRenderer; // Usually on OVRCameraRigInteraction>OVRCameraRig>OVRInteractionComprehensive>OVRHands>RightHandGrabUseSynthetic>OVRRightHandVisual>OculusHand_R>r_handMeshNode
    public Material handPassthroughMaterial; // For when occlusion is enabled, hand is masked out
    public Material handStandardMaterial; // For when occlusion is off
    public EnvironmentDepthManager environmentDepthManager; // Handles occlusion

    private GameObject notificationPrefab;
    private GameObject lightEditMenuPrefab;
    private GameObject pointLightPrefab;
    private GameObject pointLight;
    private GameObject sunLightPrefab;
    private GameObject sunLight;
    private GameObject parentOfLights; // spawn all lights under this object

    private ModelLoader modelLoader;
    private string currentEnvironmentMapPath;

    private bool rotationLock = false;
    private bool occlusionEnabled = true;

    private static StrodeloCore _instance;
    public static StrodeloCore Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StrodeloCore>();
            }
            return _instance;
        }
    }

    private int debugNum = 0;
    private GameObject _fileBrowserPrefab;

    enum ActionState
    {
        Idle,
        SelectingModelForSurface,
        SelectingSurface,
        SelectingModelForInspection,
        SelectingLightPosition,
        SelectingLightPower,
        SelectingForDeletion,
        SelectingLightForEditing,
        SelectingSunPosition,
        SelectingSunDirAndPow,
    }
    private ActionState actionState = ActionState.Idle;

    void Start()
    {
        parentOfLights = new GameObject("Lights");
        pointLightPrefab = Resources.Load<GameObject>("StrodeloPointLight");
        sunLightPrefab = Resources.Load<GameObject>("StrodeloDirectionalLight");
        _fileBrowserPrefab = Resources.Load<GameObject>("FileBrowser Variant");
        notificationPrefab = Resources.Load<GameObject>("StrodeloNotification");
        lightEditMenuPrefab = Resources.Load<GameObject>("LightEditMenu");
        instructionBoard = handMenu.instructionBoard;
        _cameraRig = FindObjectOfType<OVRCameraRig>();
        var receiverObject = Instantiate(receiverPrefab);
        var receiver = receiverObject.GetComponent<Receiver>();
        if (receiver == null)
        {
            Debug.LogError("Receiver component not found on the instantiated prefab.");
            return;
        }
        var modelLoaderObject = Instantiate(modelLoaderPrefab);
        modelLoader = modelLoaderObject.GetComponent<ModelLoader>();
        if (modelLoader == null)
        {
            Debug.LogError("ModelLoader component not found on the instantiated prefab.");
            return;
        }
        // receiver fires an event and modelLoader listens to it
        receiver.FileReceived += modelLoader.OnFileReceived;

        laser.startWidth = 0.01f;
        laser.endWidth = 0.01f;
        _instance = this;
    }

    void Update()
    {
        if (actionState == ActionState.SelectingSurface)
        {
            var ray = rayInteractor.Ray;
            MRUKAnchor sceneAnchor = null;
            var positioningMethod = MRUK.PositioningMethod.DEFAULT;
            var bestPose = MRUK.Instance?.GetCurrentRoom()?.GetBestPoseFromRaycast(ray, Mathf.Infinity,
                            new LabelFilter(), out sceneAnchor, positioningMethod); // see MRUK sample
            if (bestPose.HasValue && sceneAnchor && selectedModel)
            {
                selectedModel.transform.position = bestPose.Value.position;
                selectedModel.transform.rotation = bestPose.Value.rotation;
            }
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.green;
            laser.endColor = Color.green;
            laser.enabled = true;
        }
        if (actionState == ActionState.SelectingModelForInspection ||
            actionState == ActionState.SelectingModelForSurface)
        {
            // Show laser to indicate it's waiting for a selection
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.red;
            laser.endColor = Color.red;
            laser.enabled = true;
        }
        if (actionState == ActionState.SelectingLightPosition)
        {
            // make light follow hand
            pointLight.transform.position = rayInteractor.Origin;
        }
        else if (actionState == ActionState.SelectingLightPower)
        {
            // Power is proportional to the distance from the pointLight to the hand.
            float dist = Vector3.Distance(pointLight.transform.position, rayInteractor.Origin);
            float multiplier = 10f;
            pointLight.GetComponent<Light>().intensity = dist * multiplier;
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, pointLight.transform.position);
            laser.startColor = Color.yellow;
            laser.endColor = Color.yellow;
            laser.enabled = true;
        }
        else if (actionState == ActionState.SelectingSunPosition)
        {
            // make sun follow hand. doesn't effect the lighting, but good for positioning the icon.
            sunLight.transform.position = rayInteractor.Origin;
        }
        else if (actionState == ActionState.SelectingSunDirAndPow)
        {
            // make sun point towards the hand
            sunLight.transform.LookAt(rayInteractor.Origin);
            // Power is proportional to the distance from the sunLight to the hand.
            float dist = Vector3.Distance(sunLight.transform.position, rayInteractor.Origin);
            float multiplier = 10f;
            sunLight.GetComponent<Light>().intensity = dist * multiplier;
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, sunLight.transform.position);
            laser.startColor = Color.yellow;
            laser.endColor = Color.yellow;
            laser.enabled = true;
        }
        else if (actionState == ActionState.SelectingForDeletion)
        {
            // Show laser to indicate it's waiting for a selection
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.red;
            laser.endColor = Color.red;
            laser.enabled = true;
        }
        else if (actionState == ActionState.SelectingLightForEditing)
        {
            // Show laser to indicate it's waiting for a selection
            laser.SetPosition(0, rayInteractor.Origin);
            laser.SetPosition(1, rayInteractor.End);
            laser.startColor = Color.blue;
            laser.endColor = Color.blue;
            laser.enabled = true;
        }
        if (actionState == ActionState.Idle)
        {
            laser.enabled = false; // don't need the laser
        }
    }

    // TODO: right now we only reference the right hand rayinteractor. re-employ the use of this function but 
    // make it get the right rayinteractor to use.
    // Copied from SceneDebugger.cs (see MR utility kit samples)
    //    private Ray GetControllerRay()
    //{
    //    Vector3 rayOrigin;
    //    Vector3 rayDirection;
    //    if (OVRInput.activeControllerType == OVRInput.Controller.Touch
    //        || OVRInput.activeControllerType == OVRInput.Controller.RTouch)
    //    {
    //        rayOrigin = _cameraRig.rightHandOnControllerAnchor.position;
    //        rayDirection = _cameraRig.rightHandOnControllerAnchor.forward;
    //    }
    //    else if (OVRInput.activeControllerType == OVRInput.Controller.LTouch)
    //    {
    //        rayOrigin = _cameraRig.leftHandOnControllerAnchor.position;
    //        rayDirection = _cameraRig.leftHandOnControllerAnchor.forward;
    //    }
    //    else // hands
    //    {
    //        rayOrigin = _cameraRig.rightHandAnchor.position;
    //        rayDirection = _cameraRig.rightHandAnchor.right * -1; // .forward goes the wrong way :v
    //    }

    //    return new Ray(rayOrigin, rayDirection);
    //}

    public void OnModelSelected(object sender, System.EventArgs e)
    {
        var model = sender as GameObject;
        if (model == null)
        {
            Debug.LogError("Model is null");
            return;
        }
        // before we change what the selectedModel is, set its visualizer line color back to normal.
        if (selectedModel != null)
        {
            var oldVis = selectedModel.GetComponent<SelectableModel>().GetVisualizerChild();
            var oldVisColorChanger = oldVis.GetComponent<ChildrenLineColorChanger>();
            oldVisColorChanger.LineColor = Color.white;
        }

        selectedModel = model;
        Debug.Log($"Selected model: {selectedModel.name}");

        // Enable its collider visualizer if it has one
        var vis = selectedModel.GetComponent<SelectableModel>().GetVisualizerChild();
        if (vis != null)
        {
            vis.SetActive(true);
            // Change its color
            var visColorChanger = vis.GetComponent<ChildrenLineColorChanger>();
            if (visColorChanger != null)
            {
                visColorChanger.LineColor = Color.green;
            }
        }

        if (actionState == ActionState.SelectingModelForSurface)
        {
            actionState = ActionState.SelectingSurface;
            SetInstruction("Select a surface to place the model on.");
        }
        else if (actionState == ActionState.SelectingModelForInspection)
        {
            actionState = ActionState.Idle;
            ClearInstruction();
            SpawnMaterialInspector(selectedModel);
        }
        else if (actionState == ActionState.SelectingForDeletion)
        {
            Destroy(selectedModel);
            selectedModel = null;
            actionState = ActionState.Idle;
            ClearInstruction();
        }
        // You happen to be pointing at the model when selecting a surface, so handle the click here.
        else if (actionState == ActionState.SelectingSurface)
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    public void OnLightSelected(object sender, EventArgs e)
    {
        if (actionState == ActionState.SelectingForDeletion)
        {
            var light = sender as StrodeloLight;
            if (light == null)
            {
                Debug.LogError("Light is null");
                return;
            }
            Destroy(light.gameObject);
            actionState = ActionState.Idle;
            ClearInstruction();
        }
        else if (actionState == ActionState.SelectingLightForEditing)
        {
            var light = sender as StrodeloLight;
            if (light == null)
            {
                Debug.LogError("Light is null");
                return;
            }
            // Open a menu to edit the light
            GameObject lightEditMenu = SpawnMenu(lightEditMenuPrefab);
            var lem = lightEditMenu.GetComponent<LightEditMenu>();
            lem.InspectedLight = light.GetComponent<Light>();
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void PlaceOnSurfaceAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingModelForSurface;
            SetInstruction("Select a model to place on a surface.");
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void InspectMaterialAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingModelForInspection;
            SetInstruction("Select a model to inspect its material.");
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    internal void SetInstruction(string instruction)
    {
        instructionBoard.text = instruction;
    }

    internal void ClearInstruction()
    {
        instructionBoard.text = "";
    }

    internal void DebugButtonPressed()
    {
        debugNum++;
        SpawnFakeLoadedModel();
        SpawnNotification("Debug button pressed " + debugNum + " times.");
    }

    internal GameObject SpawnMaterialInspector(GameObject inspectedObj)
    {
        var m = SpawnMenu(materialInspectorMenuPrefab);
        var materialInspector = m.GetComponent<MaterialInspectorMenu>();

        // Find the MeshRenderer in the children of the inspected object
        MeshRenderer meshRenderer = FindMeshRendererInChildren(inspectedObj);
        if (meshRenderer != null)
        {
            materialInspector.InspectedModel = meshRenderer.gameObject;
        }
        else
        {
            Debug.LogError("No MeshRenderer found in the inspected object or its children.");
        }

        return m;
    }

    private MeshRenderer FindMeshRendererInChildren(GameObject parent)
    {
        if (parent == null) return null;

        // Check if the parent itself has a MeshRenderer component
        MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            return meshRenderer;
        }

        // Recursively check each child
        foreach (Transform child in parent.transform)
        {
            meshRenderer = FindMeshRendererInChildren(child.gameObject);
            if (meshRenderer != null)
            {
                return meshRenderer;
            }
        }

        // Return null if no MeshRenderer is found
        return null;
    }

    // Spawns a menu, and returns the menu it spawned
    internal GameObject SpawnMenu(GameObject menuPrefab)
    {
        float spawnDistance = 0.5f;
        Transform userTransform = _cameraRig.centerEyeAnchor;
        Vector3 spawnPos = userTransform.position + userTransform.forward * spawnDistance;
        // face the menu towards the user
        Quaternion rotation = Quaternion.LookRotation(userTransform.position - spawnPos);
        GameObject menu = Instantiate(menuPrefab, spawnPos, rotation);
        return menu;
    }

    // Just for debugging purposes
    private void SpawnFakeLoadedModel()
    {
        var m = Instantiate(fakeLoadedModelPrefab);
        var cubeVisualizerPrefab = Resources.Load<GameObject>("LineCube");
        m.transform.position = _cameraRig.centerEyeAnchor.position + _cameraRig.centerEyeAnchor.forward * 0.3f;
        GameObject colliderVisualizer = Instantiate<GameObject>(cubeVisualizerPrefab);
        colliderVisualizer.tag = "SelectionVisualizer";
        colliderVisualizer.transform.SetParent(m.transform);
        var boxCollider = m.GetComponent<BoxCollider>();
        colliderVisualizer.transform.localPosition = boxCollider.center;
        colliderVisualizer.transform.localScale = boxCollider.size;
        colliderVisualizer.SetActive(false);
        SelectableModel selectableModel = m.GetComponent<SelectableModel>();
        selectableModel.Selected += OnModelSelected;
    }

    internal void AddPointLightAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingLightPosition;
            SetInstruction("Select a position to place a point light.");
            // Create light now and have it follow the hand until a certain hand pose is made
            pointLight = Instantiate(pointLightPrefab, parentOfLights.transform);
            var sl = pointLight.GetComponent<StrodeloLight>();
            sl.OnSelectAction += OnLightSelected;
            sl.DisableCollider();
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    // Hand forms a certain pose
    public void HandPoseStrike()
    {
        // make the hand pose to place the light.
        // Light was already following hand in Update(), so this just makes it stop following the hand.
        if (actionState == ActionState.SelectingLightPosition)
        {
            // next, move posed hand away to define its power
            actionState = ActionState.SelectingLightPower;
        }
        else if (actionState == ActionState.SelectingSunPosition)
        {
            actionState = ActionState.SelectingSunDirAndPow;
        }
    }

    // Hand stops forming a certain pose
    public void HandPoseUnstrike()
    {
        if (actionState == ActionState.SelectingLightPower)
        {
            actionState = ActionState.Idle;
            pointLight.GetComponent<StrodeloLight>().EnableCollider();
        }
        else if (actionState == ActionState.SelectingSunDirAndPow)
        {
            actionState = ActionState.Idle;
            sunLight.GetComponent<StrodeloLight>().EnableCollider();
        }
    }

    internal void EditLightAct()
    {
        if (actionState == ActionState.SelectingLightForEditing)
        {
            actionState = ActionState.Idle;
        }
        else
        {
            actionState = ActionState.SelectingLightForEditing;
            SetInstruction("Select a light to edit.");
        }
    }

    internal void DeleteThingAct()
    {
        if (actionState == ActionState.SelectingForDeletion)
        {
            actionState = ActionState.Idle;
        }
        else
        {
            actionState = ActionState.SelectingForDeletion;
            SetInstruction("Select a light to delete.");
        }
    }

    internal void AddDirectionalLightAct()
    {
        if (actionState == ActionState.Idle)
        {
            actionState = ActionState.SelectingSunPosition;
            SetInstruction("Select a position to place the directional light.");
            // Delete any existing sun light (see "Sun" tag)
            foreach (Transform child in parentOfLights.transform)
            {
                if (child.CompareTag("Sun"))
                {
                    Destroy(child.gameObject);
                }
            }
            // Create light now and have it follow the hand until a certain hand pose is made
            sunLight = Instantiate(sunLightPrefab, parentOfLights.transform);
            var sl = sunLight.GetComponent<StrodeloLight>();
            sl.OnSelectAction += OnLightSelected;
            sl.DisableCollider(); // to not interfere with placement and dir/strength control gestures
        }
        else
        {
            actionState = ActionState.Idle;
            ClearInstruction();
        }
    }

    // Makes it so that models keep their rotation when you grab em and stuff
    internal void LockRotationToggleAct()
    {
        rotationLock = !rotationLock;
        // Get all "SelectableObject" tagged things, and tell the SelectableObject component to lock/unlock rotation
        var selectableObjects = GameObject.FindGameObjectsWithTag("SelectableObject");
        foreach (var obj in selectableObjects)
        {
            var selectableModel = obj.GetComponent<SelectableModel>();
            if (selectableModel != null)
            {
                selectableModel.LockRotation(rotationLock);
            }
        }
        if (rotationLock)
        {
            SetInstruction("Locked rotation.");
        } else
        {
            SetInstruction("Unlocked Rotation.");
        }
    }

    internal void ToggleOcclusionAct()
    {
        occlusionEnabled = !occlusionEnabled;

        if (occlusionEnabled)
        {
            environmentDepthManager.enabled = true;
            environmentDepthManager.OcclusionShadersMode = OcclusionShadersMode.SoftOcclusion;
            leftHandRenderer.material = handPassthroughMaterial;
            rightHandRenderer.material = handPassthroughMaterial;
            SetInstruction("Occlusion enabled.");
        }
        else
        {
            environmentDepthManager.enabled = false;
            environmentDepthManager.OcclusionShadersMode = OcclusionShadersMode.None;
            leftHandRenderer.material = handStandardMaterial;
            rightHandRenderer.material = handStandardMaterial;
            SetInstruction("Occlusion disabled.");
        }
    }

    // So you can change what the ambient lighting and reflections look like
    internal void ReflectionMapAct()
    {
        GameObject fileBrowserO = SpawnMenu(_fileBrowserPrefab);
        FileBrowser fileBrowser = fileBrowserO.GetComponent<FileBrowser>();
        fileBrowser.FileOpen += (sender, e) =>
        {
            SetEnvMapFromFilePath(fileBrowser.FullFilePath);
            currentEnvironmentMapPath = fileBrowser.FullFilePath; // so it can be saved to json later
        };
    }

    private void SetEnvMapFromFilePath(string fullFilePath)
    {
        // make a new "Skybox/Panoramic" material and set its texture to the loaded texture
        Material newSkyMat = new Material(Shader.Find("Skybox/Panoramic"));
        if (string.IsNullOrEmpty(fullFilePath) || !System.IO.File.Exists(fullFilePath))
        {
            Debug.LogError("Invalid file path.");
            return;
        }
        try
        {
            // Load the texture
            byte[] fileData = System.IO.File.ReadAllBytes(fullFilePath);
            Texture2D _texture = new(2, 2, TextureFormat.RGBAHalf, false);

            if (fullFilePath.EndsWith(".hdr") || fullFilePath.EndsWith(".exr")) // exr not working tho :(
            {
                try
                {
                    // Use RadianceHDRTexture to load HDR texture
                    RadianceHDRTexture hdrTexture = new RadianceHDRTexture(fileData);
                    _texture = hdrTexture.texture;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load HDR texture from file: {ex.Message}");
                    return;
                }
            }
            else
            {
                if (!_texture.LoadImage(fileData))
                {
                    Debug.LogError("Failed to load texture from file.");
                    return;
                }
            }

            _texture.Apply();

            newSkyMat.SetTexture("_MainTex", _texture);

            // Change texture of material being used for skybox, which is "Skybox/Panoramic"
            //RenderSettings.skybox.SetTexture("_MainTex", _texture);
            RenderSettings.skybox = newSkyMat;
            ReflectionProbe reflectionProbe = FindObjectOfType<ReflectionProbe>();
            if (reflectionProbe != null)
            {
                // Exclude all objects from being baked into the reflection probe
                reflectionProbe.cullingMask = 0;
                reflectionProbe.RenderProbe();
            }
            else
            {
                Debug.LogError("Reflection probe not found.");
            }
            DynamicGI.UpdateEnvironment();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading texture: {ex.Message}");
        }
    }

    internal void ExitAct()
    {
        UnityEngine.Application.Quit();
    }

    public void SpawnNotification(string message)
    {
        GameObject notification = SpawnMenu(notificationPrefab);
        notification.GetComponent<StrodeloNotification>().Message = message;
    }

    internal void LoadLocal3DModelAct()
    {
        GameObject fileBrowserO = SpawnMenu(_fileBrowserPrefab);
        FileBrowser fileBrowser = fileBrowserO.GetComponent<FileBrowser>();
        fileBrowser.FileOpen += (sender, e) =>
        {
            modelLoader.ImportAndCreateMeshes(fileBrowser.FullFilePath);
        };
    }

    // Saves the current setup to a file
    // Things to save:
    // - The current environment map (file path)
    // - The current models in the scene (file paths) and their positions and rotations
    // - The current lights in the scene and their positions, rotations, ranges, intensities, colors, and shadow settings
    internal void SaveSetupAct()
    {
        StrodeloSetupData setupData = new StrodeloSetupData();
        // Populate setupData with the current setup
        setupData.EnvironmentMapPath = currentEnvironmentMapPath;
        setupData.Models = new List<ModelSetupData>();
        // Models are anything with Selectable Model component
        // Materials for each model not supported yet but that would 
        // be like Material #0: "<texturepath>", <metallic>, <smoothness>, Material #1: ...
        var modelObjects = GameObject.FindObjectsOfType<SelectableModel>();
        Debug.Log($"Num of models: {modelObjects.Length}");
        foreach (var model in modelObjects)
        {
            ModelSetupData modelData = new ModelSetupData();
            modelData.ModelPath = model.modelFileSourcePath;
            modelData.pos = model.transform.position;
            modelData.rot = model.transform.rotation;
            setupData.Models.Add(modelData);
        }
        setupData.Lights = new List<LightSetupData>();
        var lightObjects = GameObject.FindObjectsOfType<StrodeloLight>();
        Debug.Log($"Num of lights: {lightObjects.Length}");
        foreach (var l in lightObjects)
        {
            Light lightComponent = l.gameObject.GetComponent<Light>();
            LightSetupData lightData = new LightSetupData();
            lightData.pos = l.gameObject.transform.position;
            lightData.rot = l.gameObject.transform.rotation;
            lightData.color = lightComponent.color;
            lightData.intensity = lightComponent.intensity;
            lightData.range = lightComponent.range;
            lightData.type = lightComponent.type;
            setupData.Lights.Add(lightData);
        }
        // Now that setupData is populated, save it to a json file
        string json = JsonUtility.ToJson(setupData);
        string date = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
        string savePath = UnityEngine.Application.persistentDataPath + $"/strodelo_setup-{date}.json";
        System.IO.File.WriteAllText(savePath, json);
        SpawnNotification("Setup saved to " + savePath);
    }

    internal void LoadSetupAct()
    {
        GameObject fileBrowserO = SpawnMenu(_fileBrowserPrefab);
        FileBrowser fileBrowser = fileBrowserO.GetComponent<FileBrowser>();
        fileBrowser.FileOpen += (sender, e) =>
        {
            _ = LoadSetupFromFilePath(fileBrowser.FullFilePath);
            Destroy(fileBrowserO);
        };
    }

    // for loading json file and affecting the scene accordingly
    private async Task LoadSetupFromFilePath(string fullFilePath)
    {
        if (string.IsNullOrEmpty(fullFilePath) || !System.IO.File.Exists(fullFilePath))
        {
            Debug.LogError("Invalid file path.");
            return;
        }
        // First, clear everything (destroy all lights and loaded models)
        foreach (Transform child in parentOfLights.transform)
        {
            Destroy(child.gameObject);
        }
        var modelObjects = GameObject.FindObjectsOfType<SelectableModel>();
        foreach (var model in modelObjects)
        {
            Destroy(model.gameObject);
        }
        Debug.Log("Loading setup...");
        // Parse the file into a StrodeloSetupData object
        string json = System.IO.File.ReadAllText(fullFilePath);
        StrodeloSetupData setupData = JsonUtility.FromJson<StrodeloSetupData>(json);
        // Set the environment map
        SetEnvMapFromFilePath(setupData.EnvironmentMapPath);
        // Load the models
        foreach (var modelData in setupData.Models)
        {
            Debug.Log($"Loading model...");
            GameObject m = await modelLoader.ImportAndCreateMeshes(modelData.ModelPath);
            // Move the model to the saved position and rotation
            if (m != null)
            {
                m.transform.position = modelData.pos;
                m.transform.rotation = modelData.rot;
            }
        }
        // Load the lights
        foreach (var lightData in setupData.Lights)
        {
            Debug.Log($"Loading light...");
            GameObject light;
            if (lightData.type == LightType.Point)
            {
                light = Instantiate(pointLightPrefab, parentOfLights.transform);
            }
            else if (lightData.type == LightType.Directional)
            {
                light = Instantiate(sunLightPrefab, parentOfLights.transform);
            }
            else
            {
                Debug.LogError("Unknown light type.");
                continue;
            }
            light.transform.position = lightData.pos;
            light.transform.rotation = lightData.rot;
            Light lightComponent = light.GetComponent<Light>();
            lightComponent.color = lightData.color;
            lightComponent.intensity = lightData.intensity;
            lightComponent.range = lightData.range;
            lightComponent.type = lightData.type;
            
            var sl = light.GetComponent<StrodeloLight>();
            sl.OnSelectAction += OnLightSelected;
        }
    }
}
