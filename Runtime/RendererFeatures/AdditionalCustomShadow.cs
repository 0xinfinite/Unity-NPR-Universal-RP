using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


internal class AdditionalCustomShadow : ScriptableRendererFeature
{
    private AdditionalCustomShadowPass customShadowPass;

    public RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
    public bool onlyAffectOnMainLight = false;

    public Shader _blitShader;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
            renderer.EnqueuePass(customShadowPass);   
    }

    public override void Create()
    {
        customShadowPass = new AdditionalCustomShadowPass(_renderPassEvent, onlyAffectOnMainLight);
        customShadowPass.blitShader = _blitShader;

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

public partial class AdditionalCustomShadowPass : ScriptableRenderPass
{
    const int maxShadowCount = 256;

    static int customShadowmapId = Shader.PropertyToID("_CustomShadowmapAtlas");
    static int customShadowMatricesId = Shader.PropertyToID("_CustomShadowMatrices");
    static int customShadowParamsId = Shader.PropertyToID("_CustomShadowParams");
    static int customShadowPositionId = Shader.PropertyToID("_CustomShadowPositions");
    static int customShadowSizeId = Shader.PropertyToID("_CustomShadowmapSize");
    static int customShadowCountId = Shader.PropertyToID("_CustomShadowCount");
    //GlobalKeyword customShadowKeyword;
    const string customShadowKeyword = "CUSTOM_SHADOW_ON";
    const string customShadowOnlyMainKeyword = "CUSTOM_SHADOW_ONLY_MAIN_LIGHT";

    static ShaderTagId litShaderTagId = new ShaderTagId("SRPDefaultLit");

    Matrix4x4[] customShadowMatrices;
    Vector4[] customShadowParams;
    Vector4[] customShadowPosition;
    bool customShadowOnlyMain;

    public Shader blitShader;

    public AdditionalCustomShadowPass(RenderPassEvent evt, bool onlyAffectOnMainLight)
    {
        base.profilingSampler = new ProfilingSampler(nameof(AdditionalCustomShadowPass));
        renderPassEvent = evt;

        customShadowMatrices = new Matrix4x4[maxShadowCount];
        customShadowParams = new Vector4[maxShadowCount];
        customShadowPosition = new Vector4[maxShadowCount];
        customShadowOnlyMain = onlyAffectOnMainLight;
        //customShadowKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");
    }

    //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    //{
    //    //base.Configure(cmd, cameraTextureDescriptor);
    //    //ConfigureTarget(AdditionalShadowCameraManager.manager.depthMap);

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
        cmd.BeginSample(bufferName);
        if (AdditionalShadowCameraManager.manager == null)
        {


            cmd.EndSample(bufferName);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //context.Submit();
            return;
        }
        if (AdditionalShadowCameraManager.manager.addtionalShadows.Count > 0)
        {

            SetCustomShadowMatricesAndParams(ref customShadowMatrices, ref customShadowParams, ref customShadowPosition
              ,ref context, ref cmd, ref renderingData);

            RenderTexture depthMap = AdditionalShadowCameraManager.manager.depthMap;
            ////cmd.GetTemporaryRT(depthMap.GetNativeTexturePtr().ToInt32(), depthMap.width, depthMap.height, depthMap.depth);
            cmd.SetGlobalTexture(customShadowmapId, depthMap);//(customShadowmapId, depthMap);
            cmd.SetGlobalMatrixArray(customShadowMatricesId, customShadowMatrices);
            cmd.SetGlobalVectorArray(customShadowParamsId, customShadowParams);
            cmd.SetGlobalVectorArray(customShadowPositionId, customShadowPosition);
            cmd.SetGlobalInteger(customShadowCountId, AdditionalShadowCameraManager.manager.addtionalShadows.Count);
            cmd.SetGlobalVector(customShadowSizeId, new Vector4(1 / depthMap.width, 1 / depthMap.height, depthMap.width, depthMap.height));

            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, true);
            CoreUtils.SetKeyword(cmd, customShadowKeyword, true);
            CoreUtils.SetKeyword(cmd, customShadowOnlyMainKeyword, customShadowOnlyMain);

        }
        else
        {
            CoreUtils.SetKeyword(cmd, customShadowKeyword, false);
            CoreUtils.SetKeyword(cmd, customShadowOnlyMainKeyword, false);
        }
        
        cmd.EndSample(bufferName);

    }


    void SetCustomShadowMatricesAndParams(ref Matrix4x4[] matrices, ref Vector4[] shadowParams,ref Vector4[] shadowPos
        ,ref ScriptableRenderContext context , ref CommandBuffer cmd, ref RenderingData renderingData)
    {
        int count = AdditionalShadowCameraManager.manager.addtionalShadows.Count;

        Matrix4x4 sliceTransform;

        float scaleOffset = 1.0f / (float)(AdditionalShadowCameraManager.manager.SliceRowCount);
        RenderTexture depthMap = AdditionalShadowCameraManager.manager.depthMap;

        cmd.SetRenderTarget(depthMap);
        for (int i = 0; i < count; i++)
        {
            AdditionalShadowCamera shadow = AdditionalShadowCameraManager.manager.addtionalShadows[i];
            Camera shadowCam = shadow.shadowCamera;

            sliceTransform = Matrix4x4.identity;
            sliceTransform.m00 = scaleOffset;
            sliceTransform.m11 = scaleOffset;

            Vector2 offset = SetTileViewport(i, AdditionalShadowCameraManager.manager.SliceRowCount);


            sliceTransform.m03 = offset.x * scaleOffset;
            sliceTransform.m13 = offset.y * scaleOffset;

            Matrix4x4 viewMatrix = shadowCam.worldToCameraMatrix;
           

            matrices[i] = ShadowUtils.GetShadowTransform(shadowCam.projectionMatrix, sliceTransform *
                viewMatrix);
            shadowParams[i] = new Vector4(1, shadow.softShadow?1:0, shadow.bias, shadow.falloffThreshold);
            Vector3 pos = shadow.frustumSetting.isOrthographic ? -shadow.transform.forward : shadow.transform.position;
            shadowPos[i] = new Vector4(pos.x, pos.y, pos.z, shadow.frustumSetting.isOrthographic?0:1);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
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
            customShadowPosition[i] = new Vector4(0, 0, 0, 0);
        }
    }

    public bool Setup(ref RenderingData renderingData)
    {
        if (!AdditionalShadowCameraManager.manager) return false;

        if (!AdditionalShadowCameraManager.manager.depthMap) return false;

        if (AdditionalShadowCameraManager.manager.addtionalShadows.Count <= 0) return false;

        return true;
    }

    public void Dispose()
    {

    }
}
