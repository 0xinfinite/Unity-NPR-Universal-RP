using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class MaterialBlitPass : ScriptableRenderPass
{
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler("MaterialBlit");
    Material m_Material;
    RTHandle m_colorTarget;

    ScriptableRendererFeature m_feature;

    //float m_Intensity;

    public MaterialBlitPass(Material material)
    {
        m_Material = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RTHandle colorHandle/*, float intensity*/, ScriptableRendererFeature feature)
    {
        m_colorTarget = colorHandle;
        //m_Intensity = intensity;
        m_feature = feature;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(m_colorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        if (m_Material == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            //m_Material.SetFloat("_Intensity", m_Intensity);
            Blitter.BlitTexture(cmd, m_colorTarget, m_colorTarget, m_Material, 0);

            //Texture2D tex = TextureConversion.RenderTextureToTexture2D(m_colorTarget);//((RenderTexture)mat.GetTexture("_DepthTexture"));
            //                                                                   //yield return new WaitForSeconds(0.1f);

            //byte[] bytes = tex.EncodeToPNG();

            //string path = EditorSceneManager.GetActiveScene().path.Replace(EditorSceneManager.GetActiveScene().name + ".unity", "") + EditorSceneManager.GetActiveScene().name + "/Cached Shadowmap 0.png";// + index.ToString() + ".png";

            //path = Application.dataPath.Replace("Assets", "") + path;


            //System.IO.File.WriteAllBytes(path, bytes);

        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);

        //m_feature.SetActive(false);
    }
}
