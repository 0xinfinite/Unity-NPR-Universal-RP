//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering.Universal;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal.Internal;
////using static Unity.Burst.Intrinsics.X86.Avx;

//public class AdditionalCustomShadowCasterPass: ScriptableRenderPass
//{
//    bool m_CreateEmptyShadowmap;

//    //internal RTHandle m_Handle;

//    static int m_ShadowmapID;
//    static int m_ShadowMatricesID;
//    static int m_ShadowParamsID;
//    static int m_ShadowmapSizeID;
//    static GlobalKeyword m_ShadowKeywordID;
//    //RTHandleSystem m_ShadowTextureSystem;
//    //RTHandle m_ColorHandle;
//    internal RTHandle m_ShadowmapTexture;
//    internal RenderTexture m_EmptyShadowmapTexture;
//    Matrix4x4[] m_ShadowMatrices;
//    Vector4[] m_ShadowParamsArray;
//    ShaderTagId depthTagId;

//    int m_MaxAdditionalCustomShadowCount = 256;

//    public int MaxAdditionalCustomShadowCount
//    {
//        get { return m_MaxAdditionalCustomShadowCount; }
//        set { m_MaxAdditionalCustomShadowCount = value; }
//    }

//    public AdditionalCustomShadowCasterPass(RenderPassEvent evt, int shadowCount = 256)
//    {
//        base.profilingSampler = new ProfilingSampler(nameof(AdditionalLightsShadowCasterPass));
//        renderPassEvent = evt;

//        m_MaxAdditionalCustomShadowCount = shadowCount;
//        m_ShadowParamsArray = new Vector4[shadowCount];

//        m_ShadowmapID = Shader.PropertyToID("CustomShadowmap");
//        m_ShadowMatricesID = Shader.PropertyToID("CustomShadowMatrices");
//        m_ShadowParamsID = Shader.PropertyToID("CustomShadowParams");
//        m_ShadowmapSizeID = Shader.PropertyToID("CustomShadowmapSize");
//        m_ShadowKeywordID = GlobalKeyword.Create("CUSTOM_SHADOW"); Shader.PropertyToID("CUSTOM_SHADOW");

//        depthTagId = new ShaderTagId("DepthOnly");

//        //m_ShadowTextureSystem = new RTHandleSystem();//new RenderTexture(2048, 2048, 16, UnityEngine.Experimental.Rendering.DefaultFormat.Shadow);

//        //m_EmptyShadowmapTexture = new RenderTexture(2, 2, 16, UnityEngine.Experimental.Rendering.DefaultFormat.Shadow);

//        m_ShadowMatrices = new Matrix4x4[shadowCount];
//    }

//    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//    {
//        if (!AdditionalShadowCameraManager.manager.cameras.Contains(renderingData.cameraData.camera))
//        {
//            return;
//        }

//        var desc = renderingData.cameraData.cameraTargetDescriptor;
//        //desc.depthBufferBits = 16;
//        //RenderingUtils.ReAllocateIfNeeded(ref m_Handle, desc, FilterMode.Point,
//        //    TextureWrapMode.Clamp, name: "Custom Shadow Pass");
//        //RenderingUtils.ReAllocateIfNeeded(ref m_ShadowmapTexture, desc, FilterMode.Point,
//        //    TextureWrapMode.Clamp, true);
//        //RenderingUtils.ReAllocateIfNeeded(ref m_ColorHandle, desc, FilterMode.Point,
//        //    TextureWrapMode.Clamp, false);
//        RenderingUtils.ReAllocateIfNeeded(ref m_ShadowmapTexture, desc, name: "_CustomShadowMap");
//        ConfigureTarget(m_ShadowmapTexture, m_ShadowmapTexture);
//    }

//    public override void OnCameraCleanup(CommandBuffer cmd)
//    {
//        m_ShadowmapTexture = null;
//    }

//    public void Dispose()
//    {
//        //m_Handle?.Release();
//        m_ShadowmapTexture?.Release();
//    }

//    public bool Setup(ref RenderingData renderingData)
//    {
//        Camera camera = renderingData.cameraData.camera;

//        if (!AdditionalShadowCameraManager.manager) return false;

//        if (!AdditionalShadowCameraManager.manager.cameras.Contains(camera)) return false;

//        int index = AdditionalShadowCameraManager.manager.cameras.IndexOf(camera);

//        m_ShadowMatrices[index] = camera.projectionMatrix * camera.cameraToWorldMatrix;

//        return true;
//    }

//    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//    {
//        m_CreateEmptyShadowmap = false;
//        if (!AdditionalShadowCameraManager.manager)
//            m_CreateEmptyShadowmap = true;

//        if (AdditionalShadowCameraManager.manager.addtionalShadows.Count <= 0)
//            m_CreateEmptyShadowmap = true;

//        if (m_CreateEmptyShadowmap)
//        {
//            SetEmptyShadowmap(ref context, ref renderingData);
//            //renderingData.commandBuffer.SetGlobalTexture(m_ShadowmapID, m_EmptyShadowmapTexture);//m_ShadowmapTexture);

//            return;
//        }

//        var cmd = renderingData.commandBuffer;

//        cmd.ClearRenderTarget(true, true, Color.clear);
//        cmd.BeginSample("Custom Shadow Render");
//        context.ExecuteCommandBuffer(cmd);
//        cmd.Clear();

//        RenderShadowmap(ref context, ref renderingData);
//        renderingData.commandBuffer.SetGlobalTexture(m_ShadowmapID, m_ShadowmapTexture);

//        cmd.EndSample("Custom Shadow Render");
//        context.ExecuteCommandBuffer(cmd);
//        cmd.Clear();
//    }

//    void SetEmptyShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
//    {
//        //var cmd = renderingData.commandBuffer;
//        //CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainLightShadows, true);
//        //cmd.SetGlobalVectorArray(m_ShadowParamsID,//MainLightShadowConstantBuffer._ShadowParams,
//        //                                          //new Vector4(1, 0, 1, 0)
//        //   m_ShadowParamsArray);
//        //cmd.SetGlobalVector(m_ShadowmapSizeID, //MainLightShadowConstantBuffer._ShadowmapSize,
//        //    new Vector4(1f / m_EmptyShadowmapTexture.width, 1f / m_EmptyShadowmapTexture.height, m_EmptyShadowmapTexture.width, m_EmptyShadowmapTexture.height));
//        //cmd.SetKeyword(m_ShadowKeywordID, false);
//        //context.ExecuteCommandBuffer(cmd);
//        //cmd.Clear();
//    }

//    void RenderShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
//    {
//        var cmd = renderingData.commandBuffer;


//        cmd.SetGlobalDepthBias(1.0f, 2.5f); // these values match HDRP defaults (see https://github.com/Unity-Technologies/Graphics/blob/9544b8ed2f98c62803d285096c91b44e9d8cbc47/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowAtlas.cs#L197 )

//        if(AdditionalShadowCameraManager.manager.cameras.Contains( renderingData.cameraData.camera))
//        {
         
//        //foreach(AdditionalShadowCamera shadow in AdditionalShadowCameraManager.manager.addtionalShadows)
        
//            //context.SetupCameraProperties(shadow.shadowCamera);
//            Camera camera = renderingData.cameraData.camera;
//            //camera.rect = new Rect(0,0,m_ShadowmapTexture.rt.width, m_ShadowmapTexture.rt.height);
//            //shadow.shadowCamera.targetTexture = m_ShadowmapTexture.rt;
//            //shadow.shadowCamera.Render();

//            CullingResults cullingResults;  //https://mathmakeworld.tistory.com/59

//            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
//            {
//                //ScriptableRenderer.SetRenderTarget(cmd, null, m_ShadowmapTexture, clearFlag, clearColor);

//                cullingResults = context.Cull(ref p);

//                context.SetupCameraProperties(camera);
//                //cmd.GetTemporaryRT(0, 0, m_ShadowmapTexture.rt.width, m_ShadowmapTexture.rt.height,


//                SortingSettings sortingSettings = new SortingSettings(camera);
//                sortingSettings.criteria = SortingCriteria.CommonOpaque;
//                DrawingSettings drawingSettings = new DrawingSettings(depthTagId, sortingSettings);
//                FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

//                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

//            }
//        }
//        //cmd.SetViewport(new Rect(shadowSliceData.offsetX, shadowSliceData.offsetY, shadowSliceData.resolution, shadowSliceData.resolution));
//        //cmd.SetViewProjectionMatrices(view, proj);
//        //context.ExecuteCommandBuffer(cmd);
//        //cmd.Clear();
//        //context.DrawShadows(ref settings);
//        //cmd.DisableScissorRect();
//        //context.ExecuteCommandBuffer(cmd);
//        //cmd.Clear();

//        cmd.SetGlobalDepthBias(0.0f, 0.0f); // Restore previous depth bias values

//        cmd.SetGlobalTexture(m_ShadowmapID, m_ShadowmapTexture);
//        cmd.SetGlobalMatrixArray(m_ShadowMatricesID, m_ShadowMatrices);
//        cmd.SetGlobalVectorArray(m_ShadowParamsID, m_ShadowParamsArray);
//        cmd.SetKeyword(m_ShadowKeywordID, true);
//        context.ExecuteCommandBuffer(cmd);
//        cmd.Clear();
//    }

//}
