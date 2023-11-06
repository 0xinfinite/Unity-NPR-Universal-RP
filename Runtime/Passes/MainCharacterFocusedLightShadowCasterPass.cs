using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal
{
//    /// <summary>
//    /// Renders a shadow map for the main Light.
//    /// </summary>
//    public class MainCharacterFocusedLightShadowCasterPass : ScriptableRenderPass
//    {
//        private static class MainCharacterLightShadowConstantBuffer
//        {
//            public static int _WorldToShadow;
//            public static int _ShadowParams;
//            //public static int _CascadeShadowSplitSpheres0;
//            //public static int _CascadeShadowSplitSpheres1;
//            //public static int _CascadeShadowSplitSpheres2;
//            //public static int _CascadeShadowSplitSpheres3;
//            //public static int _CascadeShadowSplitSphereRadii;
//            public static int _ShadowOffset0;
//            public static int _ShadowOffset1;
//            public static int _ShadowmapSize;
//        }

//        const int k_MaxCharacters = 1;
//        const int k_ShadowmapBufferBits = 16;
//        float m_CascadeBorder;
//        float m_MaxShadowDistanceSq;
//        //int m_ShadowCasterCascadesCount;

//        int m_MainCharacterLightShadowmapID;
//        internal RTHandle m_MainCharacterLightShadowmapTexture;
//        internal RTHandle m_EmptyLightShadowmapTexture;

//        Matrix4x4[] m_MainCharacterLightShadowMatrices;
//        ShadowSliceData[] m_CharacterSlices;
//        //Vector4[] m_CascadeSplitDistances;

//        bool m_CreateEmptyShadowmap;

//        int renderTargetWidth;
//        int renderTargetHeight;

//        float m_MainCharacterLightShadowFrustumSize = 5;
//        public float mainCharacterLightShadowFrustumSize
//        {
//            get { return m_MainCharacterLightShadowFrustumSize; }
//            set { m_MainCharacterLightShadowFrustumSize = value; }
//        }
//        LayerMask m_MainCharacterLightShadowLayerMask = 1;
//        public LayerMask mainCharacterLightShadowLayerMask
//        {
//            get { return mainCharacterLightShadowLayerMask; }
//            set { m_MainCharacterLightShadowLayerMask = value; }
//        }

//        ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Main Shadowmap");

//        /// <summary>
//        /// Creates a new <c>MainCharacterFocusedLightShadowCasterPass</c> instance.
//        /// </summary>
//        /// <param name="evt">The <c>RenderPassEvent</c> to use.</param>
//        /// <seealso cref="RenderPassEvent"/>
//        public MainCharacterFocusedLightShadowCasterPass(RenderPassEvent evt)
//        {
//            base.profilingSampler = new ProfilingSampler(nameof(MainCharacterFocusedLightShadowCasterPass));
//            renderPassEvent = evt;

//            m_MainCharacterLightShadowMatrices = new Matrix4x4[k_MaxCharacters + 1];
//            m_CharacterSlices = new ShadowSliceData[k_MaxCharacters];
//            //m_CascadeSplitDistances = new Vector4[k_MaxCharacters];

//            MainCharacterLightShadowConstantBuffer._WorldToShadow = Shader.PropertyToID("_MainCharacterLightWorldToShadow");
//            MainCharacterLightShadowConstantBuffer._ShadowParams = Shader.PropertyToID("_MainCharacterLightShadowParams");
//            //MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
//            //MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
//            //MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
//            //MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
//            //MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
//            MainCharacterLightShadowConstantBuffer._ShadowOffset0 = Shader.PropertyToID("_MainCharacterLightShadowOffset0");
//            MainCharacterLightShadowConstantBuffer._ShadowOffset1 = Shader.PropertyToID("_MainCharacterLightShadowOffset1");
//            MainCharacterLightShadowConstantBuffer._ShadowmapSize = Shader.PropertyToID("_MainCharacterLightShadowmapSize");

//            m_MainCharacterLightShadowmapID = Shader.PropertyToID("_MainCharacterLightShadowmapTexture");
//            m_EmptyLightShadowmapTexture = ShadowUtils.AllocShadowRT(1, 1, k_ShadowmapBufferBits, 1, 0, name: "_EmptyLightShadowmapTexture");
//        }

//        /// <summary>
//        /// Cleans up resources used by the pass.
//        /// </summary>
//        public void Dispose()
//        {
//            m_MainCharacterLightShadowmapTexture?.Release();
//            m_EmptyLightShadowmapTexture?.Release();
//        }

//        private Vector3 GetPositionAboveCharacter(Vector3 characterPos, Vector3 lightDir, float targetHeight)
//        {
//            return characterPos + Vector3.Project(Vector3.up * targetHeight, lightDir);
//        }

//        /// <summary>
//        /// Sets up the pass.
//        /// </summary>
//        /// <param name="renderingData"></param>
//        /// <returns>True if the pass should be enqueued, otherwise false.</returns>
//        /// <seealso cref="RenderingData"/>
//        public bool Setup(ref RenderingData renderingData)
//        {
//            using var profScope = new ProfilingScope(null, m_ProfilingSetupSampler);

//            if (!renderingData.shadowData.supportsMainCharacterLightShadows)
//                return SetupForEmptyRendering(ref renderingData);

//            bool success = MainCharacterManager.manager && MainCharacterManager.manager.mainCharacterList.Count > 0;
//            Debug.Log(success);
//            if (!success)
//                return SetupForEmptyRendering(ref renderingData);

//            Clear();
//            int shadowLightIndex = renderingData.lightData.mainLightIndex;
//            if (shadowLightIndex == -1)
//                return SetupForEmptyRendering(ref renderingData);

//            VisibleLight shadowLight = renderingData.lightData.visibleLights[shadowLightIndex];
//            Light light = shadowLight.light;
//            if (light.shadows == LightShadows.None)
//                return SetupForEmptyRendering(ref renderingData);

//            if (shadowLight.lightType != LightType.Directional)
//            {
//                Debug.LogWarning("Only directional lights are supported as main light.");
//            }

//            Bounds bounds;
//            if (!renderingData.cullResults.GetShadowCasterBounds(shadowLightIndex, out bounds))
//                return SetupForEmptyRendering(ref renderingData);

//            //m_ShadowCasterCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;

//            int shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(renderingData.shadowData.mainLightShadowmapWidth,
//                renderingData.shadowData.mainLightShadowmapHeight, /*m_ShadowCasterCascadesCount*/k_MaxCharacters); //temp
//            renderTargetWidth = renderingData.shadowData.mainLightShadowmapWidth;
//            renderTargetHeight = //(m_ShadowCasterCascadesCount == 2) ?
//                //renderingData.shadowData.mainLightShadowmapHeight >> 1 :
//                renderingData.shadowData.mainLightShadowmapHeight;

//            for (int cascadeIndex = 0; cascadeIndex < /*m_ShadowCasterCascadesCount*/k_MaxCharacters; ++cascadeIndex)
//            {
//                 ShadowUtils.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData,
//                    shadowLightIndex, cascadeIndex, renderTargetWidth, renderTargetHeight, shadowResolution, light.shadowNearPlane,
//                    out _, out m_CharacterSlices[cascadeIndex]);


//                Vector4 lightPos = m_CharacterSlices[cascadeIndex].viewMatrix.GetColumn(3);
//                Vector3 lightDir = m_CharacterSlices[cascadeIndex].viewMatrix.GetColumn(2) * -1;//light.transform.forward * -1;
//                Vector3 charPos = MainCharacterManager.manager.mainCharacterList[0].transform.position;
//                Vector3 newLightPos = charPos; // GetPositionAboveCharacter(charPos, lightDir, lightPos.y-charPos.y);

//                m_CharacterSlices[cascadeIndex].viewMatrix.SetColumn(3,
//                    new Vector4(newLightPos.x+ MainCharacterManager.manager.offset.x, newLightPos.y + MainCharacterManager.manager.offset.y, newLightPos.z+ MainCharacterManager.manager.offset.z,
//                    lightPos.w));

//                Debug.Log(lightPos.y);

//                m_CharacterSlices[cascadeIndex].shadowTransform = ShadowUtils.GetShadowTransform(m_CharacterSlices[cascadeIndex].projectionMatrix, m_CharacterSlices[cascadeIndex].viewMatrix);
//            }

//            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_MainCharacterLightShadowmapTexture, renderTargetWidth, renderTargetHeight, k_ShadowmapBufferBits, name: "_MainCharacterLightShadowmapTexture");

//            m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
//            m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
//            m_CreateEmptyShadowmap = false;
//            useNativeRenderPass = true;
//            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyLightShadowmapTexture, 1, 1, k_ShadowmapBufferBits, name: "_EmptyLightShadowmapTexture");

//            return true;
//        }

//        bool SetupForEmptyRendering(ref RenderingData renderingData)
//        {
//            if (!renderingData.cameraData.renderer.stripShadowsOffVariants)
//                return false;

//            m_CreateEmptyShadowmap = true;
//            useNativeRenderPass = false;
//            ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyLightShadowmapTexture, 1, 1, k_ShadowmapBufferBits, name: "_EmptyLightShadowmapTexture");

//            return true;
//        }

//        /// <inheritdoc />
//        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
//        {
//            if (m_CreateEmptyShadowmap)
//                ConfigureTarget(m_EmptyLightShadowmapTexture);
//            else
//                ConfigureTarget(m_MainCharacterLightShadowmapTexture);
//            ConfigureClear(ClearFlag.All, Color.black);
//        }

//        /// <inheritdoc/>
//        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            if (m_CreateEmptyShadowmap)
//            {
//                SetEmptyMainCharacterLightCascadeShadowmap(ref context, ref renderingData);
//                renderingData.commandBuffer.SetGlobalTexture(m_MainCharacterLightShadowmapID, m_EmptyLightShadowmapTexture.nameID);

//                return;
//            }

//            RenderMainCharacterLightCascadeShadowmap(ref context, ref renderingData);
//            renderingData.commandBuffer.SetGlobalTexture(m_MainCharacterLightShadowmapID, m_MainCharacterLightShadowmapTexture.nameID);
//        }

//        void Clear()
//        {
//            for (int i = 0; i < m_MainCharacterLightShadowMatrices.Length; ++i)
//                m_MainCharacterLightShadowMatrices[i] = Matrix4x4.identity;

//            //for (int i = 0; i < m_CascadeSplitDistances.Length; ++i)
//            //    m_CascadeSplitDistances[i] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

//            for (int i = 0; i < m_CharacterSlices.Length; ++i)
//                m_CharacterSlices[i].Clear();
//        }

//        void SetEmptyMainCharacterLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            var cmd = renderingData.commandBuffer;
//            CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainCharacterLightShadows, true);
//            cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowParams,
//                new Vector4(1, 0, 1, 0));
//            cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowmapSize,
//                new Vector4(1f / m_EmptyLightShadowmapTexture.rt.width, 1f / m_EmptyLightShadowmapTexture.rt.height, m_EmptyLightShadowmapTexture.rt.width, m_EmptyLightShadowmapTexture.rt.height));
//            context.ExecuteCommandBuffer(cmd);
//            cmd.Clear();
//        }

//        void RenderMainCharacterLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
//        {
//            var cullResults = renderingData.cullResults;
//            var lightData = renderingData.lightData;
//            var shadowData = renderingData.shadowData;

//            int shadowLightIndex = lightData.mainLightIndex;
//            if (shadowLightIndex == -1)
//                return;

//            VisibleLight shadowLight = lightData.visibleLights[shadowLightIndex];

//            var cmd = renderingData.commandBuffer;
//            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MainCharacterLightShadow)))
//            {
//                var settings = new ShadowDrawingSettings(cullResults, shadowLightIndex, BatchCullingProjectionType.Orthographic);
//                settings.useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers;
//                // Need to start by setting the Camera position as that is not set for passes executed before normal rendering
//                cmd.SetGlobalVector(ShaderPropertyId.worldSpaceCameraPos, renderingData.cameraData.worldSpaceCameraPos);

//                for (int cascadeIndex = 0; cascadeIndex < /*m_ShadowCasterCascadesCount*/k_MaxCharacters; ++cascadeIndex)
//                {
//                    settings.splitData = m_CharacterSlices[cascadeIndex].splitData;

//                    //shadowLight.light.renderingLayerMask = m_MainCharacterLightShadowLayerMask;
//                    Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, shadowLightIndex, ref shadowData, m_CharacterSlices[cascadeIndex].projectionMatrix, m_CharacterSlices[cascadeIndex].resolution);
//                    ShadowUtils.SetupShadowCasterConstantBuffer(cmd, ref shadowLight, shadowBias);
//                    CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.CastingPunctualLightShadow, false);
//                    ShadowUtils.RenderShadowSlice(cmd, ref context, ref m_CharacterSlices[cascadeIndex],
//                        ref settings, m_CharacterSlices[cascadeIndex].projectionMatrix, m_CharacterSlices[cascadeIndex].viewMatrix);
//                }

//                shadowData.isKeywordSoftShadowsEnabled = shadowLight.light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows;
//                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainCharacterLightShadows, shadowData.mainLightShadowCascadesCount == 1);
//                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.MainCharacterLightShadowCascades, shadowData.mainLightShadowCascadesCount > 1);
//                CoreUtils.SetKeyword(cmd, ShaderKeywordStrings.SoftShadows, shadowData.isKeywordSoftShadowsEnabled);

//                SetupMainCharacterLightShadowReceiverConstants(cmd, ref shadowLight, ref shadowData);
//            }
//        }

//        void SetupMainCharacterLightShadowReceiverConstants(CommandBuffer cmd, ref VisibleLight shadowLight, ref ShadowData shadowData)
//        {
//            Light light = shadowLight.light;
//            bool softShadows = shadowLight.light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows;

//            int cascadeCount = k_MaxCharacters;//m_ShadowCasterCascadesCount;
//            for (int i = 0; i < cascadeCount; ++i)
//                m_MainCharacterLightShadowMatrices[i] = m_CharacterSlices[i].shadowTransform;

//            // We setup and additional a no-op WorldToShadow matrix in the last index
//            // because the ComputeCascadeIndex function in Shadows.hlsl can return an index
//            // out of bounds. (position not inside any cascade) and we want to avoid branching
//            Matrix4x4 noOpShadowMatrix = Matrix4x4.zero;
//            noOpShadowMatrix.m22 = (SystemInfo.usesReversedZBuffer) ? 1.0f : 0.0f;
//            for (int i = cascadeCount; i <= k_MaxCharacters; ++i)
//                m_MainCharacterLightShadowMatrices[i] = noOpShadowMatrix;

//            float invShadowAtlasWidth = 1.0f / renderTargetWidth;
//            float invShadowAtlasHeight = 1.0f / renderTargetHeight;
//            float invHalfShadowAtlasWidth = 0.5f * invShadowAtlasWidth;
//            float invHalfShadowAtlasHeight = 0.5f * invShadowAtlasHeight;
//            float softShadowsProp = ShadowUtils.SoftShadowQualityToShaderProperty(light, softShadows);

//            ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out float shadowFadeScale, out float shadowFadeBias);

//            cmd.SetGlobalMatrixArray(MainCharacterLightShadowConstantBuffer._WorldToShadow, m_MainCharacterLightShadowMatrices);
//            cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowParams,
//                new Vector4(light.shadowStrength, softShadowsProp, shadowFadeScale, shadowFadeBias));

//            //if (m_ShadowCasterCascadesCount > 1)
//            //{
//            //    cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres0,
//            //        m_CascadeSplitDistances[0]);
//            //    cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres1,
//            //        m_CascadeSplitDistances[1]);
//            //    cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres2,
//            //        m_CascadeSplitDistances[2]);
//            //    cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSpheres3,
//            //        m_CascadeSplitDistances[3]);
//            //    cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._CascadeShadowSplitSphereRadii, new Vector4(
//            //        m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w,
//            //        m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w,
//            //        m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w,
//            //        m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
//            //}

//            // Inside shader soft shadows are controlled through global keyword.
//            // If any additional light has soft shadows it will force soft shadows on main light too.
//            // As it is not trivial finding out which additional light has soft shadows, we will pass main light properties if soft shadows are supported.
//            // This workaround will be removed once we will support soft shadows per light.
//            if (shadowData.supportsSoftShadows)
//            {
//                cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowOffset0,
//                    new Vector4(-invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight,
//                        invHalfShadowAtlasWidth, -invHalfShadowAtlasHeight));
//                cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowOffset1,
//                    new Vector4(-invHalfShadowAtlasWidth, invHalfShadowAtlasHeight,
//                        invHalfShadowAtlasWidth, invHalfShadowAtlasHeight));

//                cmd.SetGlobalVector(MainCharacterLightShadowConstantBuffer._ShadowmapSize, new Vector4(invShadowAtlasWidth,
//                    invShadowAtlasHeight,
//                    renderTargetWidth, renderTargetHeight));
//            }
//        }
//        private class PassData
//        {
//            internal MainCharacterFocusedLightShadowCasterPass pass;
//            internal RenderGraph graph;

//            internal TextureHandle shadowmapTexture;
//            internal RenderingData renderingData;
//            internal int shadowmapID;

//            internal bool emptyShadowmap;
//        }

//        internal TextureHandle Render(RenderGraph graph, ref RenderingData renderingData)
//        {
//            TextureHandle shadowTexture;

//            using (var builder = graph.AddRenderPass<PassData>("Main Characters Light Shadowmap", out var passData, base.profilingSampler))
//            {
//                InitPassData(ref passData, ref renderingData, ref graph);

//                if (!m_CreateEmptyShadowmap)
//                {
//                    passData.shadowmapTexture = UniversalRenderer.CreateRenderGraphTexture(graph, m_MainCharacterLightShadowmapTexture.rt.descriptor, "Main Characters Shadowmap", true, ShadowUtils.m_ForceShadowPointSampling ? FilterMode.Point : FilterMode.Bilinear);
//                    builder.UseDepthBuffer(passData.shadowmapTexture, DepthAccess.Write);
//                }

//                // Need this as shadowmap is only used as Global Texture and not a buffer, so would get culled by RG
//                builder.AllowPassCulling(false);

//                builder.SetRenderFunc((PassData data, RenderGraphContext context) =>
//                {
//                    if (!data.emptyShadowmap)
//                        data.pass.RenderMainCharacterLightCascadeShadowmap(ref context.renderContext, ref data.renderingData);
//                });

//                shadowTexture = passData.shadowmapTexture;
//            }

//            using (var builder = graph.AddRenderPass<PassData>("Set Main Characters Shadow Globals", out var passData, base.profilingSampler))
//            {
//                InitPassData(ref passData, ref renderingData, ref graph);

//                passData.shadowmapTexture = shadowTexture;

//                if (shadowTexture.IsValid())
//                    builder.UseDepthBuffer(shadowTexture, DepthAccess.Read);

//                builder.AllowPassCulling(false);

//                builder.SetRenderFunc((PassData data, RenderGraphContext context) =>
//                {
//                    if (data.emptyShadowmap)
//                    {
//                        data.pass.SetEmptyMainCharacterLightCascadeShadowmap(ref context.renderContext, ref data.renderingData);
//                        data.shadowmapTexture = data.graph.defaultResources.defaultShadowTexture;
//                    }

//                    data.renderingData.commandBuffer.SetGlobalTexture(data.shadowmapID, data.shadowmapTexture);
//                });
//                return passData.shadowmapTexture;
//            }
//        }

//        void InitPassData(ref PassData passData, ref RenderingData renderingData, ref RenderGraph graph)
//        {
//            passData.pass = this;
//            passData.graph = graph;

//            passData.emptyShadowmap = m_CreateEmptyShadowmap;
//            passData.shadowmapID = m_MainCharacterLightShadowmapID;
//            passData.renderingData = renderingData;
//        }
//    };
}
