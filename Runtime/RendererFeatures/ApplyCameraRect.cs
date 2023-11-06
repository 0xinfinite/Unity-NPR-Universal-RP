using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ApplyCameraRect : ScriptableRendererFeature
{
    private ApplyCameraRectPass pass;

    public RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingShadows;

    public RenderTexture _targetTexture;

    public override void Create()
    {
        pass = new ApplyCameraRectPass(_renderPassEvent, _targetTexture);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}

public class ApplyCameraRectPass : ScriptableRenderPass
{
    public RenderTexture targetTexture;

    public ApplyCameraRectPass(RenderPassEvent _evt, RenderTexture _rt)
    {
        renderPassEvent = _evt;
        targetTexture = _rt;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        Rect rect = renderingData.cameraData.camera.rect;

        rect.x = targetTexture.width * rect.x;
        rect.y = targetTexture.height * rect.y;
        rect.width = targetTexture.width * rect.width;
        rect.height = targetTexture.height * rect.height;

        cmd.EnableScissorRect(rect);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        //cmd.DisableScissorRect();
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        //renderingData.cameraData.clearDepth = false;
        renderingData.cameraData.camera.clearFlags = CameraClearFlags.Depth;
    }
}
