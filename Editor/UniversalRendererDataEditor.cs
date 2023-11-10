using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    /// <summary>
    /// Editor script for a <c>UniversalRendererData</c> class.
    /// </summary>
    [CustomEditor(typeof(UniversalRendererData), true)]
    public class UniversalRendererDataEditor : ScriptableRendererDataEditor
    {
        private static class Styles
        {
            public static readonly GUIContent RendererTitle = EditorGUIUtility.TrTextContent("Universal Renderer", "Custom Universal Renderer for Universal RP.");
            public static readonly GUIContent PostProcessIncluded = EditorGUIUtility.TrTextContent("Enabled", "Enables the use of post processing effects within the scene. If disabled, Unity excludes post processing renderer Passes, shaders and textures from the build.");
            public static readonly GUIContent PostProcessLabel = EditorGUIUtility.TrTextContent("Data", "The asset containing references to shaders and Textures that the Renderer uses for post-processing.");
            public static readonly GUIContent FilteringSectionLabel = EditorGUIUtility.TrTextContent("Filtering", "Settings that controls and define which layers the renderer draws.");
            public static readonly GUIContent OpaqueMask = EditorGUIUtility.TrTextContent("Opaque Layer Mask", "Controls which opaque layers this renderer draws.");
            public static readonly GUIContent TransparentMask = EditorGUIUtility.TrTextContent("Transparent Layer Mask", "Controls which transparent layers this renderer draws.");

            public static readonly GUIContent RenderingSectionLabel = EditorGUIUtility.TrTextContent("Rendering", "Settings related to rendering and lighting.");
            public static readonly GUIContent RenderingModeLabel = EditorGUIUtility.TrTextContent("Rendering Path", "Select a rendering path.");
            public static readonly GUIContent DepthPrimingModeLabel = EditorGUIUtility.TrTextContent("Depth Priming Mode", "With depth priming enabled, Unity uses the depth buffer generated in the depth prepass to determine if a fragment should be rendered or skipped during the Base Camera opaque pass. Disabled: Unity does not perform depth priming. Auto: If there is a Render Pass that requires a depth prepass, Unity performs the depth prepass and depth priming. Forced: Unity performs the depth prepass and depth priming.");
            public static readonly GUIContent DepthPrimingModeInfo = EditorGUIUtility.TrTextContent("On Android, iOS, and Apple TV, Unity performs depth priming only in the Forced mode. On tiled GPUs, which are common to those platforms, depth priming might reduce performance when combined with MSAA.");
            public static readonly GUIContent CopyDepthModeLabel = EditorGUIUtility.TrTextContent("Depth Texture Mode", "Controls after which pass URP copies the scene depth. It has a significant impact on mobile devices bandwidth usage. It also allows to force a depth prepass to generate it.");
            public static readonly GUIContent RenderPassLabel = EditorGUIUtility.TrTextContent("Native RenderPass", "Enables URP to use RenderPass API. Has no effect on OpenGLES2");

            public static readonly GUIContent RenderPassSectionLabel = EditorGUIUtility.TrTextContent("RenderPass", "This section contains properties related to render passes.");
            public static readonly GUIContent ShadowsSectionLabel = EditorGUIUtility.TrTextContent("Shadows", "This section contains properties related to rendering shadows.");
            public static readonly GUIContent PostProcessingSectionLabel = EditorGUIUtility.TrTextContent("Post-processing", "This section contains properties related to rendering post-processing.");

            public static readonly GUIContent OverridesSectionLabel = EditorGUIUtility.TrTextContent("Overrides", "This section contains Render Pipeline properties that this Renderer overrides.");

            public static readonly GUIContent accurateGbufferNormalsLabel = EditorGUIUtility.TrTextContent("Accurate G-buffer normals", "Normals in G-buffer use octahedron encoding/decoding. This improves visual quality but might reduce performance.");
            public static readonly GUIContent defaultStencilStateLabel = EditorGUIUtility.TrTextContent("Default Stencil State", "Configure the stencil state for the opaque and transparent render passes.");
            public static readonly GUIContent shadowTransparentReceiveLabel = EditorGUIUtility.TrTextContent("Transparent Receive Shadows", "When disabled, none of the transparent objects will receive shadows.");
            public static readonly GUIContent invalidStencilOverride = EditorGUIUtility.TrTextContent("Error: When using the deferred rendering path, the Renderer requires the control over the 4 highest bits of the stencil buffer to store Material types. The current combination of the stencil override options prevents the Renderer from controlling the required bits. Try changing one of the options to Replace.");
            public static readonly GUIContent intermediateTextureMode = EditorGUIUtility.TrTextContent("Intermediate Texture", "Controls when URP renders via an intermediate texture.");
			
			// custom for added below
			public static readonly GUIContent DrawOpaqueLabel = EditorGUIUtility.TrTextContent("Draw Opaque", "Allow to draw Opaque Objects.");
            public static readonly GUIContent DrawTransparentLabel = EditorGUIUtility.TrTextContent("Draw Transparent", "Allow to draw Transparent Objects.");
            public static readonly GUIContent WarpMapLabel = EditorGUIUtility.TrTextContent("Warp Map", "Warp Map for Shading");
            public static readonly GUIContent WarpMapCountLabel = EditorGUIUtility.TrTextContent("Warp Map Count", "Warp Map Count for Shading");

			public static readonly GUIContent DistanceAttenuationMapAtlasLabel = EditorGUIUtility.TrTextContent("Distance Attenuation Map Atlas", "Distance attenuation map atlas for Punctual lights");
            public static readonly GUIContent BaseIndexOfDistanceAttenuationMapLabel = EditorGUIUtility.TrTextContent("Base Index of Distance Attenuation Map", "Set Basic Index Number of Distance attenuation map atlas for Punctual lights.(None will physically)");
            public static readonly GUIContent DistanceAttenuationMapCountLabel = EditorGUIUtility.TrTextContent("Distance Attenuation Map Count", "Count on Distance attenuation map atlas for Punctual lights");
            public static readonly GUIContent PunctionalLightFallOffStartLabel = EditorGUIUtility.TrTextContent("Punctional Light Fall-Off Start Range", "Set Range which punctional light fall off start.(0 will physically)");

        }

        SerializedProperty m_OpaqueLayerMask;
        SerializedProperty m_TransparentLayerMask;
        SerializedProperty m_RenderingMode;
        SerializedProperty m_DepthPrimingMode;
        SerializedProperty m_CopyDepthMode;
        SerializedProperty m_AccurateGbufferNormals;
        SerializedProperty m_UseNativeRenderPass;
        SerializedProperty m_DefaultStencilState;
        SerializedProperty m_PostProcessData;
        SerializedProperty m_Shaders;
        SerializedProperty m_ShadowTransparentReceiveProp;
        SerializedProperty m_IntermediateTextureMode;
		
		//added for custom below
		SerializedProperty m_DrawOpaque;
        SerializedProperty m_DrawTransparent;
        SerializedProperty m_WarpMapAtlas;
        SerializedProperty m_WarpMapCount;
        SerializedProperty m_DistanceAttenuationMapAtlas;
        SerializedProperty m_BaseIndexOfDistanceAttenuationMap;
        SerializedProperty m_DistanceAttenuationMapCount;
        SerializedProperty m_PunctionalLightFallOffStart;

        private void OnEnable()
        {
            m_OpaqueLayerMask = serializedObject.FindProperty("m_OpaqueLayerMask");
            m_TransparentLayerMask = serializedObject.FindProperty("m_TransparentLayerMask");
            m_RenderingMode = serializedObject.FindProperty("m_RenderingMode");
            m_DepthPrimingMode = serializedObject.FindProperty("m_DepthPrimingMode");
            m_CopyDepthMode = serializedObject.FindProperty("m_CopyDepthMode");
            m_AccurateGbufferNormals = serializedObject.FindProperty("m_AccurateGbufferNormals");
            m_UseNativeRenderPass = serializedObject.FindProperty("m_UseNativeRenderPass");
            m_DefaultStencilState = serializedObject.FindProperty("m_DefaultStencilState");
            m_PostProcessData = serializedObject.FindProperty("postProcessData");
            m_Shaders = serializedObject.FindProperty("shaders");
            m_ShadowTransparentReceiveProp = serializedObject.FindProperty("m_ShadowTransparentReceive");
            m_IntermediateTextureMode = serializedObject.FindProperty("m_IntermediateTextureMode");
			
			m_DrawOpaque = serializedObject.FindProperty("m_DrawOpaque");
            m_DrawTransparent = serializedObject.FindProperty("m_DrawTransparent");
            m_WarpMapAtlas = serializedObject.FindProperty("m_WarpMapAtlas");
            m_WarpMapCount = serializedObject.FindProperty("m_WarpMapCount");
            m_DistanceAttenuationMapAtlas = serializedObject.FindProperty("m_DistanceAttenuationMapAtlas");
            m_BaseIndexOfDistanceAttenuationMap = serializedObject.FindProperty("m_BaseIndexOfDistanceAttenuationMap");
            m_DistanceAttenuationMapCount = serializedObject.FindProperty("m_DistanceAttenuationMapCount");
            m_PunctionalLightFallOffStart = serializedObject.FindProperty("m_PunctionalLightFallOffStart");
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Styles.FilteringSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_OpaqueLayerMask, Styles.OpaqueMask);
            EditorGUILayout.PropertyField(m_TransparentLayerMask, Styles.TransparentMask);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Styles.RenderingSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_RenderingMode, Styles.RenderingModeLabel);
            if (m_RenderingMode.intValue == (int)RenderingMode.Deferred)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_AccurateGbufferNormals, Styles.accurateGbufferNormalsLabel, true);
                EditorGUI.indentLevel--;
            }

            if (m_RenderingMode.intValue == (int)RenderingMode.Forward || m_RenderingMode.intValue == (int)RenderingMode.ForwardPlus)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_DepthPrimingMode, Styles.DepthPrimingModeLabel);
                if (m_DepthPrimingMode.intValue != (int)DepthPrimingMode.Disabled)
                {
                    EditorGUILayout.HelpBox(Styles.DepthPrimingModeInfo.text, MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(m_CopyDepthMode, Styles.CopyDepthModeLabel);
            EditorGUILayout.PropertyField(m_DrawOpaque, Styles.DrawOpaqueLabel);
            EditorGUILayout.PropertyField(m_DrawTransparent, Styles.DrawTransparentLabel);


            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.RenderPassSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_UseNativeRenderPass, Styles.RenderPassLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(Styles.ShadowsSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_WarpMapAtlas, Styles.WarpMapLabel);
            EditorGUILayout.PropertyField(m_WarpMapCount, Styles.WarpMapCountLabel);
            EditorGUILayout.PropertyField(m_DistanceAttenuationMapAtlas, Styles.DistanceAttenuationMapAtlasLabel);
            EditorGUILayout.PropertyField(m_BaseIndexOfDistanceAttenuationMap, Styles.BaseIndexOfDistanceAttenuationMapLabel);
            EditorGUILayout.PropertyField(m_DistanceAttenuationMapCount, Styles.DistanceAttenuationMapCountLabel);
            EditorGUILayout.PropertyField(m_PunctionalLightFallOffStart, Styles.PunctionalLightFallOffStartLabel);
            EditorGUILayout.PropertyField(m_ShadowTransparentReceiveProp, Styles.shadowTransparentReceiveLabel);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Styles.PostProcessingSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            var postProcessIncluded = EditorGUILayout.Toggle(Styles.PostProcessIncluded, m_PostProcessData.objectReferenceValue != null);
            if (EditorGUI.EndChangeCheck())
            {
                m_PostProcessData.objectReferenceValue = postProcessIncluded ? PostProcessData.GetDefaultPostProcessData() : null;
            }
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_PostProcessData, Styles.PostProcessLabel);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Styles.OverridesSectionLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_DefaultStencilState, Styles.defaultStencilStateLabel, true);
            SerializedProperty overrideStencil = m_DefaultStencilState.FindPropertyRelative("overrideStencilState");

            if (overrideStencil.boolValue && m_RenderingMode.intValue == (int)RenderingMode.Deferred)
            {
                CompareFunction stencilFunction = (CompareFunction)m_DefaultStencilState.FindPropertyRelative("stencilCompareFunction").enumValueIndex;
                StencilOp stencilPass = (StencilOp)m_DefaultStencilState.FindPropertyRelative("passOperation").enumValueIndex;
                StencilOp stencilFail = (StencilOp)m_DefaultStencilState.FindPropertyRelative("failOperation").enumValueIndex;
                StencilOp stencilZFail = (StencilOp)m_DefaultStencilState.FindPropertyRelative("zFailOperation").enumValueIndex;
                bool invalidFunction = stencilFunction == CompareFunction.Disabled || stencilFunction == CompareFunction.Never;
                bool invalidOp = stencilPass != StencilOp.Replace && stencilFail != StencilOp.Replace && stencilZFail != StencilOp.Replace;

                if (invalidFunction || invalidOp)
                    EditorGUILayout.HelpBox(Styles.invalidStencilOverride.text, MessageType.Error, true);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Compatibility", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(m_IntermediateTextureMode, Styles.intermediateTextureMode);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI(); // Draw the base UI, contains ScriptableRenderFeatures list

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (EditorPrefs.GetBool("DeveloperMode"))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(m_Shaders, true);

                if (GUILayout.Button("Reload All"))
                {
                    var resources = target as UniversalRendererData;
                    resources.shaders = null;
                    ResourceReloader.ReloadAllNullIn(target, UniversalRenderPipelineAsset.packagePath);
                }
            }
        }
    }
}
