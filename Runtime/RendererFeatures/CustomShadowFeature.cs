using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


internal class CustomShadowFeature : ScriptableRendererFeature
{
    public CustomShadowPass customShadowPass;

    public RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
    public bool onlyAffectOnMainLight = false;

    public RenderTexture _depthMap;
    public int _sliceRowCount;

    public bool clearPass;

    //public Shader _blitShader;
    //public Material _copyMat;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        customShadowPass.SliceRowCount = _sliceRowCount;

            renderer.EnqueuePass(customShadowPass);   
    }

    public override void Create()
    {
        if (customShadowPass == null || clearPass)
        {
            customShadowPass = new CustomShadowPass(_renderPassEvent, onlyAffectOnMainLight);
            customShadowPass.depthMap = _depthMap;
        }
    }

    //public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    //{
    //    customShadowPass.SetupCustomShadows(ref renderingData);
    //}


    //public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
    //{
    //    //base.OnCameraPreCull(renderer, cameraData);
    //}
}

public partial class CustomShadowPass : ScriptableRenderPass
{
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
    const string customShadowOnlyMainKeyword = "CUSTOM_SHADOW_ONLY_MAIN_LIGHT";

    static ShaderTagId litShaderTagId = new ShaderTagId("SRPDefaultLit");

    Matrix4x4[] customShadowMatrices;
    Vector4[] customShadowParams;
    Vector4[] customShadowParams2;
    Vector4[] customShadowPosition;
    bool customShadowOnlyMain;

    public RenderTexture depthMap;
    public int SliceRowCount;
    Dictionary<CustomShadowCamera, CustomShadowProperty> customShadows;

    struct CustomShadowProperty
    {
        public CustomShadowProperty(int _index, bool _isActive)
        {
            index = _index;
            isActive = _isActive;
        }

        public int index;
        public bool isActive;

        public void SetActive(bool _active)
        {
            isActive = _active;
        }
    }

    public void AddCustomShadow(CustomShadowCamera shadow)
    {
        Debug.Log("shadow id " + shadow.GetInstanceID());
        if (!customShadows.ContainsKey(shadow))
        {
            Debug.Log("shadow id " + shadow.GetInstanceID()+" added");
            customShadows.Add(shadow, new CustomShadowProperty(customShadows.Count, shadow.enabled || shadow.gameObject.activeInHierarchy));
        }
        else
        {
            Debug.Log("shadow id " + shadow.GetInstanceID()+" already in directory");
        }
        
    }

    public void RemoveCustomShadow(CustomShadowCamera shadow)
    {
        if (customShadows.ContainsKey(shadow))
        {
            customShadows.Remove(shadow);
        }
    }

    public void ChangeStatusShadow(CustomShadowCamera shadow)
    {
        if (customShadows.ContainsKey(shadow))
        {
            customShadows[shadow].SetActive(shadow.enabled || shadow.gameObject.activeInHierarchy);
        }
    }
    public void ChangeStatusShadow(CustomShadowCamera shadow, bool _active)
    {
        if (customShadows.ContainsKey(shadow))
        {
            customShadows[shadow].SetActive(_active);
        }
    }


    public CustomShadowPass(RenderPassEvent evt, bool onlyAffectOnMainLight)
    {
        base.profilingSampler = new ProfilingSampler(nameof(CustomShadowPass));
        renderPassEvent = evt;

        customShadowMatrices = new Matrix4x4[maxShadowCount];
        customShadowParams = new Vector4[maxShadowCount];
        customShadowParams2 = new Vector4[maxShadowCount];
        customShadowPosition = new Vector4[maxShadowCount];
        customShadowOnlyMain = onlyAffectOnMainLight;
        if (customShadows == null)
        {
            customShadows = new Dictionary<CustomShadowCamera, CustomShadowProperty>();//<CustomShadowCamera>();
        }
        Debug.Log("Pass Created");
        //customShadowKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");
    }

    //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    //{
    //    //base.Configure(cmd, cameraTextureDescriptor);
    //    //ConfigureTarget(depthMap);

    //}


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        
        RenderCustomShadows(ref context, ref renderingData);

        ResetShadowParams();
    }

    const string bufferName = "Custom Shadow Pass";
    public void RenderCustomShadows(ref ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = //new CommandBuffer { name =  bufferName}; //
                                                           renderingData.commandBuffer;
        //cmd.BeginSample(bufferName);

        //Debug.Log("Count : "+ customShadows.Keys.Count);
        if (customShadows==null || SliceRowCount == 0)
        {

            CoreUtils.SetKeyword(cmd, customShadowKeyword, false);
            CoreUtils.SetKeyword(cmd, customShadowOnlyMainKeyword, false);

            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();

            //context.Submit();
            //return;
        }
        else if (customShadows.Keys.Count > 0)
        {

            SetCustomShadowMatricesAndParams(ref customShadowMatrices, ref customShadowParams,  ref customShadowPosition, ref customShadowParams2
              );

            //RenderTexture depthMap = depthMap;
            ////cmd.GetTemporaryRT(depthMap.GetNativeTexturePtr().ToInt32(), depthMap.width, depthMap.height, depthMap.depth);
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

            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, true);
            CoreUtils.SetKeyword(cmd, customShadowKeyword, customShadowOnlyMain?false:true);
            CoreUtils.SetKeyword(cmd, customShadowOnlyMainKeyword, customShadowOnlyMain?true:false);

            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();
        }
        else
        {
            CoreUtils.SetKeyword(cmd, customShadowKeyword, false);
            CoreUtils.SetKeyword(cmd, customShadowOnlyMainKeyword, false);


            //cmd.EndSample(bufferName);

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();
        }
        
        
       

    }


    void SetCustomShadowMatricesAndParams(ref Matrix4x4[] matrices, ref Vector4[] shadowParams, ref Vector4[] shadowPos, ref Vector4[] shadowParams2)
    {
        int count = customShadows.Count;

        Matrix4x4 sliceTransform;

        float scaleOffset = 1.0f / (float)(SliceRowCount);
        //scaleOffset *= offset.x;
        //RenderTexture depthMap = depthMap;

        //cmd.SetRenderTarget(depthMap);
        //for (int i = 0; i < count; i++)
        foreach(CustomShadowCamera shadow in customShadows.Keys)
        {
            //CustomShadowCamera shadow = customShadows[i];
            int index = customShadows[shadow].index;
            //Debug.Log(index);

            if(shadow == null)
            {
                ChangeStatusShadow(shadow, false);
                shadowParams[index] = Vector4.zero;
                continue;
            }

            if (!customShadows[shadow].isActive||shadow.shadowStrength == 0) {
                shadowParams[index] = Vector4.zero;
                continue; 
            }

            Camera shadowCam = shadow.shadowCamera;

            if(shadowCam == null)
            {
                ChangeStatusShadow(shadow, false);
                shadowParams[index] = Vector4.zero;
                continue;
            }
            shadowCam.ResetProjectionMatrix();
            shadowCam.ResetWorldToCameraMatrix();
            //Transform quadTf = shadow.quadRenderer.transform;
            sliceTransform = Matrix4x4.identity;
            sliceTransform.m00 = scaleOffset;//shadow.quadOffset.z;//
                                             //quadTf.localScale.x;//scaleOffset;// * offset.z;
            sliceTransform.m11 = scaleOffset;//shadow.quadOffset.w;//
                                 //quadTf.localScale.y; //scaleOffset;// * offset.w;

            Vector2 offset = //shadow.quadOffset;//new Vector2(shadow.quadRenderer.transform.localPosition.x, shadow.quadRenderer.transform.localPosition.y);//
                                               SetTileViewport(index, SliceRowCount);

            //Debug.Log(offset);

            sliceTransform.m03 = //shadow.quadOffset.x;//
                                 //quadTf.localPosition.x; //
                                 offset.x* scaleOffset;// * scaleOffset;// + offset.y;
            sliceTransform.m13 = //shadow.quadOffset.y;//
                                //quadTf.localPosition.y; //
                                offset.y* scaleOffset;// * scaleOffset;// + offset.z;

            //shadow.quadOffset = new Vector4(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset,  scaleOffset);
            //Matrix4x4 viewMatrix = 
            //     sliceTransform*shadowCam.worldToCameraMatrix;

            //copyMat.SetTexture("_BaseMap", depthMap );
            //copyMat.SetTexture("_OverlayMap", shadowCam.targetTexture);
            //copyMat.SetVector("_OverlayMapParams", new Vector4(0,0,1,1));//offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset));
            //cmd.Blit(null, depthMap, copyMat);

            matrices[index] = sliceTransform * ShadowUtils.GetShadowTransform(shadowCam.projectionMatrix, shadowCam.worldToCameraMatrix);
            shadowParams[index] = new Vector4(shadow.shadowStrength, shadow.softShadow?
                (shadow.shadowQuality == SoftShadowQuality.UsePipelineSettings?3:(int)shadow.shadowQuality)
                :0//offset.x* scaleOffset, offset.y* scaleOffset /*shadow.softShadow?1:0*/
                , shadow.bias, shadow.falloffThreshold);
            //shadow.quadOffset = new Vector4(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);
            shadowParams2[index] = new Vector4(offset.x* scaleOffset, offset.x* scaleOffset + scaleOffset, offset.y* scaleOffset, offset.y* scaleOffset + scaleOffset);
           // Debug.Log(new Vector4(offset.x, offset.x + scaleOffset, offset.y, offset.y + scaleOffset));
                //new Vector4(1, 1, 1, 1);
            Vector3 pos = shadow.frustumSetting.isOrthographic ? -shadow.transform.forward : shadow.transform.position;
            shadowPos[index] = new Vector4(pos.x, pos.y, pos.z, shadow.frustumSetting.isOrthographic?0:1);
        }
        //context.ExecuteCommandBuffer(cmd);
        //cmd.Clear();
    }


    Vector2 SetTileViewport(int index, int split)
    {
        Vector2 offset = new Vector2((index % split), (index / split));

        return offset;
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

    //public bool Setup(ref RenderingData renderingData)
    //{
    //    //if (!CustomShadowCameraManager.manager) return false;

    //    if (!depthMap) return false;

    //    if (customShadows.Count <= 0) return false;

    //    return true;
    //}

    //public void Dispose()
    //{

    //}
}
