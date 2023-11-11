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
public class CustomShadowCameraManager : MonoBehaviour
{
    public static CustomShadowCameraManager manager;

    private CustomShadowFeature myFeature;

    const int maxShadowCount = 256;

    static int customShadowmapId = Shader.PropertyToID("_CustomShadowmapAtlas");
    static int customShadowMatricesId = Shader.PropertyToID("_CustomShadowMatrices");
    static int customShadowParamsId = Shader.PropertyToID("_CustomShadowParams");
    static int customShadowParams2Id = Shader.PropertyToID("_CustomShadowParams2");
    static int customShadowPositionId = Shader.PropertyToID("_CustomShadowPositions");
    static int customShadowSizeId = Shader.PropertyToID("_CustomShadowmapSize");
    static int customShadowCountId = Shader.PropertyToID("_CustomShadowCount");
    static int customShadowOffset0Id = Shader.PropertyToID("_CustomShadowOffset0");
    static int customShadowOffset1Id = Shader.PropertyToID("_CustomShadowOffset1");
    //GlobalKeyword customShadowKeyword;
    const string customShadowKeyword = "CUSTOM_SHADOW_ON";
    const string
    //GlobalKeyword
    customShadowOnlyMainKeyword = "CUSTOM_SHADOW_ONLY_MAIN_LIGHT";

    //static ShaderTagId litShaderTagId = new ShaderTagId("SRPDefaultLit");

    Matrix4x4[] customShadowMatrices;
    Vector4[] customShadowParams;
    Vector4[] customShadowParams2;
    Vector4[] customShadowPosition;
    bool customShadowOnlyMain;



    public Camera captureCamera;

    public List<CustomShadowCamera> customShadows = new List<CustomShadowCamera>();

    public RenderTexture depthMap;

    public Shader depthDecoder;

    public LayerMask layerMask = 268435456;

    private MeshRenderer previewQuad;

    [SerializeField]
    private int sliceRowCount = 1;
    public int SliceRowCount { get { return sliceRowCount; } }

    public bool onlyAffectOnMainLight = true;
    public bool setCustomShadowOnManager = false;

    public Vector4 offset;

    private GameObject tempQuad;


    // Start is called before the first frame update
    void Awake()
    {
        if (!setCustomShadowOnManager)
        {
            if (manager == null)
            {
                manager = this;
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
        }

        if (setCustomShadowOnManager)
        {
            customShadowMatrices = new Matrix4x4[maxShadowCount];
            customShadowParams = new Vector4[maxShadowCount];
            customShadowParams2 = new Vector4[maxShadowCount];
            customShadowPosition = new Vector4[maxShadowCount];
            customShadowOnlyMain = onlyAffectOnMainLight;
            //customShadowKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");
            //customShadowOnlyMainKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ONLY_MAIN_LIGHT");
        }


        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        gameObject.layer = LayerMask.NameToLayer("DepthDecoder");

        if(captureCamera == null)   
        captureCamera = GetComponent<Camera>();

        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");
        if (!TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData _))
        {
            gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(1);
        }
        customShadows = new List<CustomShadowCamera>();
        //cameras = new List<Camera>();

        //if (depthMap == null)
        //{
        //    depthMap = new RenderTexture(2048, 2048, 16, RenderTextureFormat.Depth);
        //}

        //captureCamera.targetTexture = depthMap;
        captureCamera.depth = 2;
        captureCamera.clearFlags = CameraClearFlags.Nothing;
        captureCamera.farClipPlane = 1.0f;
        //foreach (CustomShadowCamera shadow in shadows)
        //{

        //    if (shadow.shadowCamera.targetTexture)
        //    {
        //        shadow.shadowCamera.targetTexture.Release();
        //        shadow.shadowCamera.targetTexture = null;
        //    }
        //        shadow.shadowCamera.targetTexture = new RenderTexture(depthMap.width / sliceRowCount, depthMap.height / sliceRowCount,
        //            16, RenderTextureFormat.Depth );
        //}


        //customShadowMatrices = new Matrix4x4[maxShadowCount];
        //customShadowParams = new Vector4[maxShadowCount];

        //light = FindAnyObjectByType<Light>();
        if (previewQuad == null)
        {
            tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gameObject.AddComponent<MeshFilter>();
            previewQuad = gameObject.AddComponent<MeshRenderer>();
           
        }

        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);

        customShadows = new List<CustomShadowCamera>();

        CustomShadowCamera[] shadows = GameObject.FindObjectsOfType<CustomShadowCamera>();

        if (shadows.Length <= 0)
            return;

        //customShadows.AddRange(shadows);
        for(int i = 0; i < shadows.Length; i++)
        {
            if (!customShadows.Contains(shadows[i]))
            {
                customShadows.Add(shadows[i]);
            }
        }

        //OnEnable();
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

        for (int i = 0; i < customShadows.Count; i++)
        {
            myFeature.customShadowPass.AddCustomShadow(customShadows[i]); 
        }
        myFeature.SetDirty();
    }

    public void SetShadowActive(CustomShadowCamera shadow, bool _active)
    {
        myFeature.customShadowPass.ChangeStatusShadow(shadow, _active);
        myFeature.SetDirty();
    }

    public void AddCustomShadow(CustomShadowCamera shadow)
    {
        if(!customShadows.Contains(shadow))
        customShadows.Add(shadow);
        OrientChildQuads();
        myFeature.customShadowPass.AddCustomShadow(shadow);
        myFeature.SetDirty();
    }

    public void RemoveCustomShadow(CustomShadowCamera shadow)
    {
        if (customShadows.Contains(shadow))
            customShadows.Remove(shadow);
        OrientChildQuads();
        myFeature.customShadowPass.RemoveCustomShadow(shadow);
        myFeature.SetDirty();
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

    CustomShadowFeature FindMyRendererFeature()
    {
        var camera = Camera.main;

        var selectedRenderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;

        foreach(var feature in selectedRenderer.rendererFeatures)
        {
            if(feature.GetType() == typeof(CustomShadowFeature))
            {
                return feature as CustomShadowFeature;
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
        for (int i = 0; i < customShadows.Count; ++i)// (CustomShadowCamera shadow in customShadows)
        {
            CustomShadowCamera shadow = customShadows[i];

            //shadow.shadowCamera.enabled = true;
            Vector2 offset = SetTileViewport(i, sliceRowCount);
            //shadow.shadowCamera.rect = //new Rect(0, 0, 1, 1);//
            //                                                new Rect(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);

            shadow.quadRenderer.transform.localScale = Vector3.one * scaleOffset;
            shadow.quadRenderer.transform.localPosition = new Vector3(offset.x * scaleOffset + scaleOffset * 0.5f, offset.y * scaleOffset + scaleOffset * 0.5f, 0.5f);
        }

    }

    private void LateUpdate()
    {
        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);

        if (setCustomShadowOnManager && Application.isPlaying)
        {
            SetCustomShadows();
        }
    }

    private void OnDisable()
    {
        //depthMap.Release();
        //Shader.DisableKeyword(customShadowKeyword);
        captureCamera.ResetProjectionMatrix();
    }

    private void OnDestroy()
    {
        if (!setCustomShadowOnManager)
        {
            if (manager == this)
            {
                manager = null;
            }
        }
    }

    public void AddCustomShadow()
    {
        if (depthDecoder==null)
        {
            Debug.Log("Depth Decoder Shader not assigned!");
            return;
        }

        GameObject addedGO = new GameObject();
        addedGO.name = "Custom Shadow";
        CustomShadowCamera addedCustomShadow = addedGO.AddComponent<CustomShadowCamera>();

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
        AddCustomShadow(addedCustomShadow);
        sliceRowCount = GetRowCount();
        myFeature._sliceRowCount = sliceRowCount;
        myFeature.SetDirty();
        
        
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

    public void SetCustomShadows()
    {
        var cmd = new CommandBuffer(); //new CommandBuffer { name =  bufferName}; //
                                                           // .commandBuffer;
        //cmd.BeginSample(bufferName);
        if ( customShadows == null)
        {
            //Shader.DisableKeyword(customShadowKeyword);
            //Shader.DisableKeyword(customShadowOnlyMainKeyword);
            CoreUtils.SetKeyword(cmd,customShadowKeyword, false);
            CoreUtils.SetKeyword(cmd,customShadowOnlyMainKeyword, false);

            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();

            //context.Submit();
            //return;
        }
        else if (customShadows.Count > 0)
        {

            SetCustomShadowMatricesAndParams(
              );

            cmd.SetGlobalTexture(customShadowmapId, depthMap);//(customShadowmapId, depthMap);
            cmd.SetGlobalMatrixArray(customShadowMatricesId, customShadowMatrices);
            cmd.SetGlobalVectorArray(customShadowParamsId, customShadowParams);
            cmd.SetGlobalVectorArray(customShadowParams2Id, customShadowParams2);
            cmd.SetGlobalVectorArray(customShadowPositionId, customShadowPosition);
            cmd.SetGlobalInteger(customShadowCountId, customShadows.Count);
            Vector2 oneDivDepthMapSize = Vector2.one / new Vector2(depthMap.width, depthMap.height);
            cmd.SetGlobalVector(customShadowSizeId, new Vector4(oneDivDepthMapSize.x, oneDivDepthMapSize.y, depthMap.width, depthMap.height));

            Vector2Int allocatedShadowAtlasSize = new Vector2Int(depthMap.width, depthMap.height);
            Vector2 invShadowAtlasSize = Vector2.one / allocatedShadowAtlasSize;
            Vector2 invHalfShadowAtlasSize = invShadowAtlasSize * 0.5f;

            cmd.SetGlobalVector(customShadowOffset0Id,
                    new Vector4(-invHalfShadowAtlasSize.x, -invHalfShadowAtlasSize.y,
                        invHalfShadowAtlasSize.x, -invHalfShadowAtlasSize.y));
            cmd.SetGlobalVector(customShadowOffset1Id,
                    new Vector4(-invHalfShadowAtlasSize.x, invHalfShadowAtlasSize.y,
                        invHalfShadowAtlasSize.x, invHalfShadowAtlasSize.y));

            //if (customShadowOnlyMain)
            //{
            //    Shader.EnableKeyword(customShadowOnlyMainKeyword);
            //    Shader.DisableKeyword(customShadowKeyword);
            //}
            //else
            //{
            //    Shader.EnableKeyword(customShadowKeyword);
            //    Shader.DisableKeyword(customShadowOnlyMainKeyword);
            //}
            CoreUtils.SetKeyword(cmd,customShadowKeyword, customShadowOnlyMain ? false : true);
            CoreUtils.SetKeyword(cmd,customShadowOnlyMainKeyword, customShadowOnlyMain ? true : false);

            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();
        }
        else
        {
            //Shader.DisableKeyword(customShadowKeyword);
            //Shader.DisableKeyword(customShadowOnlyMainKeyword);
            CoreUtils.SetKeyword(cmd,customShadowKeyword, false);
            CoreUtils.SetKeyword(cmd,customShadowOnlyMainKeyword, false);


            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();
        }

        captureCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmd);

        //cmd.Clear();

        ResetShadowParams();


    }


    void SetCustomShadowMatricesAndParams()
    {
        int count = customShadows.Count;

        Matrix4x4 sliceTransform;

        float scaleOffset = 1.0f / (float)(SliceRowCount);
        for (int i = 0; i < count; i++)
        {
            CustomShadowCamera shadow = customShadows[i];

            if (shadow.shadowStrength == 0)
            {
                customShadowParams[i] = Vector4.zero;
                continue;
            }

            Camera shadowCam = shadow.shadowCamera;
            shadowCam.ResetProjectionMatrix();
            shadowCam.ResetWorldToCameraMatrix();
            sliceTransform = Matrix4x4.identity;
            sliceTransform.m00 = scaleOffset;//shadow.quadOffset.z;//
                                             //quadTf.localScale.x;//scaleOffset;// * .offset.z;
            sliceTransform.m11 = scaleOffset;//shadow.quadOffset.w;//
                                             //quadTf.localScale.y; //scaleOffset;// * .offset.w;

            Vector2 offset = //shadow.quadOffset;//new Vector2(shadow.quadRenderer.transform.localPosition.x, shadow.quadRenderer.transform.localPosition.y);//
                                               SetTileViewport(i, SliceRowCount);


            sliceTransform.m03 = //shadow.quadOffset.x;//
                                 //quadTf.localPosition.x; //
                                 offset.x * scaleOffset;// * scaleOffset;// + .offset.y;
            sliceTransform.m13 = //shadow.quadOffset.y;//
                                 //quadTf.localPosition.y; //
                                offset.y * scaleOffset;// * scaleOffset;// + .offset.z;

            customShadowMatrices[i] = sliceTransform * ShadowUtils.GetShadowTransform(shadowCam.projectionMatrix, shadowCam.worldToCameraMatrix);
            customShadowParams[i] = new Vector4(shadow.shadowStrength, shadow.softShadow ?
                (shadow.shadowQuality == SoftShadowQuality.UsePipelineSettings ? 3 : (int)shadow.shadowQuality)
                : 0//offset.x* scaleOffset, offset.y* scaleOffset /*shadow.softShadow?1:0*/
                , shadow.bias, shadow.falloffThreshold);
            //shadow.quadOffset = new Vector4(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);
            customShadowParams2[i] = new Vector4(offset.x * scaleOffset, offset.x * scaleOffset + scaleOffset, offset.y * scaleOffset, offset.y * scaleOffset + scaleOffset);
            // Debug.Log(new Vector4(offset.x, offset.x + scaleOffset, offset.y, offset.y + scaleOffset));
            //new Vector4(1, 1, 1, 1);
            Vector3 pos = shadow.frustumSetting.isOrthographic ? -shadow.transform.forward : shadow.transform.position;
            customShadowPosition[i] = new Vector4(pos.x, pos.y, pos.z, shadow.frustumSetting.isOrthographic ? 0 : 1);
        }
    }



    void ResetShadowParams()
    {
        for (int i = 0; i < maxShadowCount; i++)
        {
            customShadowParams[i] = new Vector4(0, 0, 0, 0);
            customShadowParams2[i] = new Vector4(0, 0, 0, 0);
            customShadowPosition[i] = new Vector4(0, 0, 0, 0);
        }
    }
}
