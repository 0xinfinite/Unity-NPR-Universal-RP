using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//using Nothke.Utils;

[System.Serializable]
public struct FrustumSetting
{
    public float range;
    public float nearPlane;
    public bool isOrthographic;
    public float fov;
    public float orthoSize;

    public FrustumSetting(float _range)
    {
        range = _range;
        nearPlane = 0.1f;
        isOrthographic = false;
        fov = 60;
        orthoSize = 5;
    }
}

[ExecuteInEditMode()]
[RequireComponent(typeof(Camera))]
public class CustomDepthCameraManager : MonoBehaviour
{
    private CustomDepthFeature myFeature;

    public Camera captureCamera;

    public List<CustomDepthCamera> customShadows = new List<CustomDepthCamera>();

    public RenderTexture depthMap;

    public Shader depthDecoder;

    public LayerMask layerMask = 268435456;

    private MeshRenderer previewQuad;

    [SerializeField]
    private int sliceRowCount = 1;
    public int SliceRowCount { get { return sliceRowCount; } }

    public Vector4 offset;

    private GameObject tempQuad;


    // Start is called before the first frame update
    void Awake()
    {
        transform.localScale = Vector3.one;
        gameObject.layer = LayerMask.NameToLayer("DepthDecoder");

        if(captureCamera == null)   
        captureCamera = GetComponent<Camera>();

        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");
        if (!TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData _))
        {
            gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(1);
        }
        customShadows = new List<CustomDepthCamera>();
     
        captureCamera.depth = 2;
        captureCamera.clearFlags = CameraClearFlags.Nothing;
        captureCamera.farClipPlane = 1.0f;
        
        if (previewQuad == null)
        {
            tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gameObject.AddComponent<MeshFilter>();
            previewQuad = gameObject.AddComponent<MeshRenderer>();
           
        }

        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);

        customShadows = new List<CustomDepthCamera>();

        CustomDepthCamera[] shadows = GameObject.FindObjectsOfType<CustomDepthCamera>();

        if (shadows.Length <= 0)
            return;

        
        for(int i = 0; i < shadows.Length; i++)
        {
            if (!customShadows.Contains(shadows[i]))
            {
                customShadows.Add(shadows[i]);
            }
        }

    }

    private void Start()
    {
        if (tempQuad != null)
        {
            GetComponent<MeshFilter>().sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempQuad);
        }
        this.enabled = true;

        myFeature = FindMyRendererFeature();

        if (myFeature != null)
        {
            for (int i = 0; i < customShadows.Count; i++)
            {
                myFeature.customDepthPass.AddCustomDepth(customShadows[i]);
            }
            myFeature.SetDirty();
        }
    }

    public void SetShadowActive(CustomDepthCamera shadow, bool _active)
    {
        if (myFeature != null)
        {
            myFeature.customDepthPass.ChangeStatusDepth(shadow, _active);
            myFeature.SetDirty();
        }
    }

    public void AddCustomDepth(CustomDepthCamera shadow)
    {
        if(!customShadows.Contains(shadow))
        customShadows.Add(shadow);
        OrientChildQuads();
        if (myFeature != null)
        {
            myFeature.customDepthPass.AddCustomDepth(shadow);
            myFeature.SetDirty();
        }
    }

    public void RemoveCustomShadow(CustomDepthCamera shadow)
    {
        if (customShadows.Contains(shadow))
            customShadows.Remove(shadow);
        OrientChildQuads();
        if (myFeature != null)
        {
            myFeature.customDepthPass.RemoveCustomDepth(shadow);
            myFeature.SetDirty();
        }
    }

    Vector2 SetTileViewport(int index, int split)
    {
        Vector2 offset = new Vector2((index % split), (index / split));

        return offset;
    }

    private void OnValidate()
    {
        sliceRowCount = GetRowCount();
        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");

        if (previewQuad == null)
        {
            if(TryGetComponent<MeshRenderer>(out previewQuad))
            {

            }
        }

        if(depthDecoder != null && depthMap != null && previewQuad !=null
            && (previewQuad.sharedMaterial == null|| previewQuad.sharedMaterials.Length<=0 || previewQuad.sharedMaterials[0]==null))
        {
            Material newMat = new Material(depthDecoder);
            newMat.SetTexture("_BaseMap", depthMap);
            previewQuad.sharedMaterial = newMat;
        }
    }

    CustomDepthFeature FindMyRendererFeature()
    {
        var camera = Camera.main;

        var selectedRenderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;

        foreach(var feature in selectedRenderer.rendererFeatures)
        {
            if(feature.GetType() == typeof(CustomDepthFeature))
            {
                return feature as CustomDepthFeature;
            }
        }

        return null;
    }

    private void OnEnable()
    {
        OrientChildQuads();
        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");

    }

    public void OrientChildQuads()
    {
        sliceRowCount = GetRowCount();

        float scaleOffset = 1.0f / (float)SliceRowCount;
        for (int i = 0; i < customShadows.Count; ++i)// (CustomDepthCamera shadow in customShadows)
        {
            CustomDepthCamera shadow = customShadows[i];

            Vector2 offset = SetTileViewport(i, sliceRowCount);

            shadow.quadRenderer.transform.localScale = Vector3.one * scaleOffset;
            shadow.quadRenderer.transform.localPosition = new Vector3(offset.x * scaleOffset + scaleOffset * 0.5f, offset.y * scaleOffset + scaleOffset * 0.5f, 0.5f);
        }

    }

    private void LateUpdate()
    {
        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);
    }

    private void OnDisable()
    {
        //depthMap.Release();
        captureCamera.ResetProjectionMatrix();
    }

    private void OnDestroy()
    {
         if(myFeature != null)
        {
            myFeature.clearPass = true;
        }
    }

    public void AddCustomDepth()
    {
        if (depthDecoder==null)
        {
            Debug.Log("Depth Decoder Shader not assigned!");
            return;
        }

        GameObject addedGO = new GameObject();
        addedGO.name = "Custom Depth Camera";
        CustomDepthCamera addedCustomShadow = addedGO.AddComponent<CustomDepthCamera>();

        //if (!customShadows.Contains(addedCustomShadow))
        //{
        //}

        if (addedCustomShadow.quadRenderer == null)
        {
            GameObject quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            addedCustomShadow.quadRenderer = quadGO.GetComponent<MeshRenderer>();
            quadGO.layer = LayerMask.NameToLayer("DepthDecoder");
            
                quadGO.transform.parent = transform;
                quadGO.transform.localPosition = Vector3.zero;
                quadGO.transform.localRotation = Quaternion.identity;
                quadGO.transform.localScale = Vector3.one;
            
            Camera newCam = addedCustomShadow.GetComponent<Camera>();
            newCam.targetTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.Depth);
            Material newMat = new Material(depthDecoder);
            addedCustomShadow.quadRenderer.sharedMaterial = newMat;
            newMat.SetTexture("_BaseMap", newCam.targetTexture);
            //addedCustomShadow.depthDecoder = depthDecoder;
            newCam.clearFlags = CameraClearFlags.Nothing;
            addedCustomShadow.enabled = true;
            addedGO.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
        }
        AddCustomDepth(addedCustomShadow);
        sliceRowCount = GetRowCount();
        if (myFeature != null)
        {
            myFeature._sliceRowCount = sliceRowCount;
            myFeature.SetDirty();
        }
        
    }

    int GetRowCount()
    {
        if (customShadows == null) return 0;

        switch (customShadows.Count)
        {
            case 1:
                return 1;
            case 2:
            case 3:
            case 4:
                return 2;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return 3;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
                return 4;
        }
        if (customShadows.Count > 25) return 6;

        return 5;
    }
    
}
