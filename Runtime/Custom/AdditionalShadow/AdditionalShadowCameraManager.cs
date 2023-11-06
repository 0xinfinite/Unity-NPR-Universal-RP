using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
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
public class AdditionalShadowCameraManager : MonoBehaviour
{
    public static AdditionalShadowCameraManager manager;

    public List<AdditionalShadowCamera> addtionalShadows;

    //public List<Camera> cameras;

    public RenderTexture depthMap;

    //public List<Renderer> targetRendererList = new List<Renderer>();

    [SerializeField]
    private int sliceRowCount = 1;
    public int SliceRowCount { get { return sliceRowCount; } }

    const int maxShadowCount = 256;

    //readonly static int customShadowmapId = Shader.PropertyToID("_CustomShadowmapAtlas");
    //readonly static int customShadowMatricesId = Shader.PropertyToID("_CustomShadowMatrices");
    //readonly static int customShadowParamsId = Shader.PropertyToID("_CustomShadowParams");
    //readonly static int customShadowSizeId = Shader.PropertyToID("_CustomShadowmapSize");
    //readonly static int customShadowCountId = Shader.PropertyToID("_CustomShadowCount");
    //GlobalKeyword customShadowKeyword;

    //Light light;

    //Matrix4x4[] customShadowMatrices;
    //Vector4[] customShadowParams;

    // Start is called before the first frame update
    void Awake()
    {
        if(manager == null)
        {
            manager = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        addtionalShadows = new List<AdditionalShadowCamera>();
        //cameras = new List<Camera>();
        AdditionalShadowCamera[] shadows = GameObject.FindObjectsOfType<AdditionalShadowCamera>();

        if (shadows.Length <= 0)
            return;

        addtionalShadows.AddRange(shadows);

        if (depthMap == null)
        {
            depthMap = new RenderTexture(2048, 2048, 16, RenderTextureFormat.Shadowmap);
        }
        foreach (AdditionalShadowCamera shadow in shadows)
        {
            if (sliceRowCount == 1)
            {
                shadow.shadowCamera.targetTexture = depthMap;
            }
            else
            {
                shadow.shadowCamera.targetTexture = new RenderTexture(depthMap.width / sliceRowCount, depthMap.height / sliceRowCount,
                    depthMap.depth, depthMap.format);// depthMap;
                                                     //   cameras.Add(shadow.shadowCamera);
            }
        }


        //customShadowMatrices = new Matrix4x4[maxShadowCount];
        //customShadowParams = new Vector4[maxShadowCount];
        //customShadowKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");

        //light = FindAnyObjectByType<Light>();
    }

    float Remap(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

    }

    Vector2 SetTileViewport(int index, int split)
    {
        Vector2 offset = new Vector2((index % split), (index / split));

        return offset;
    }



    private void OnEnable()
    {
        float scaleOffset = 1.0f / (float)SliceRowCount;
        for (int i = 0; i<addtionalShadows.Count;++i)// (AdditionalShadowCamera shadow in addtionalShadows)
        {
            AdditionalShadowCamera shadow = addtionalShadows[i];
            
            //shadow.shadowCamera.enabled = true;
            Vector2 offset = SetTileViewport(i, sliceRowCount);
            shadow.shadowCamera.rect = new Rect(0, 0, 1, 1);// new Rect(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);
            
        }

    }

    private void OnDisable()
    {
        depthMap.Release();
        //Shader.DisableKeyword(customShadowKeyword);
    }

    private void OnDestroy()
    {
        if (manager == this)
        {
            manager = null;
        }
    }

    //void LateUpdate()
    //{
    //    float scaleOffset = 1.0f / (float)(SliceRowCount);
        
    //    for (int i = 0; i < addtionalShadows.Count; ++i)// (AdditionalShadowCamera shadow in addtionalShadows)
    //    {
    //        AdditionalShadowCamera shadow = addtionalShadows[i];

    //        //Camera shadowCam = shadow.shadowCamera;

    //        //depthMap.BeginPerspectiveRendering(shadow.)
            
    //        //shadow.shadowCamera.worldToCameraMatrix = sliceTransform * shadow.shadowCamera.worldToCameraMatrix;//Matrix4x4.TRS(camTf.position, camTf.rotation, Vector3.one) ;
    //        //shadow.shadowCamera.Render();
    //        //shadow.shadowCamera.SetTargetBuffers(depthMap.colorBuffer, depthMap.depthBuffer);
    //        //shadow.shadowCamera.targetTexture = depthMap;
    //    }
    //    //RenderCustomShadows();
    //}
    //const string bufferName = "Custom Shadow";

    //private void RenderCustomShadows()
    //{
    //    CommandBuffer buffer = new CommandBuffer() { name = bufferName };//CommandBufferPool.Get(bufferName);

    //    buffer.BeginSample(bufferName);

    //    if (addtionalShadows.Count>0)
    //    {
    //        SetCustomShadowMatricesAndParams();

    //        buffer.SetGlobalTexture(customShadowmapId, depthMap);
    //        buffer.SetGlobalMatrixArray(customShadowMatricesId, customShadowMatrices);
    //        buffer.SetGlobalVectorArray(customShadowParamsId, customShadowParams);
    //        buffer.SetGlobalInteger(customShadowCountId, addtionalShadows.Count);
    //        buffer.SetGlobalVector(customShadowSizeId, new Vector4(1 / depthMap.width, 1 / depthMap.height, depthMap.width, depthMap.height));

    //        buffer.EnableKeyword(customShadowKeyword);
    //        //Debug.Log(addtionalShadows.Count);
    //    }
    //    else
    //    {
    //        buffer.DisableKeyword(customShadowKeyword);
    //        Debug.Log("No Custom Shadows");

    //        buffer.EndSample(bufferName);
    //        ResetShadowParams();
    //        return;
    //    }

    //    buffer.EndSample(bufferName);

    //    light.AddCommandBuffer(LightEvent.BeforeShadowMap, buffer);

    //    ResetShadowParams();

    //}
    //void SetCustomShadowMatricesAndParams()
    //{
    //    int count = addtionalShadows!=null ? addtionalShadows.Count : 0;
    //    for (int i = 0; i < count; i++)
    //    {
    //        AdditionalShadowCamera shadow = addtionalShadows[i];
    //        Camera shadowCam = shadow.shadowCamera;
    //        //shadowCam.Render();
    //        customShadowMatrices[i] = ShadowUtils.GetShadowTransform(shadowCam.projectionMatrix, shadowCam.cameraToWorldMatrix);
    //        customShadowParams[i] = new Vector4(1, 0, 0, 0);
    //    }
    //}

    //void ResetShadowParams()
    //{
    //    for (int i = 0; i < maxShadowCount; i++)
    //    {
    //        customShadowParams[i] = new Vector4(0, 0, 0, 0);
    //    }
    //}

}
