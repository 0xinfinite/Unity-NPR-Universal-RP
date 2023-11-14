using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class CustomDepthFeature : ScriptableRendererFeature
{
    public CustomDepthPass customDepthPass;

    public RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingShadows;

    public string customDepthmapIdName = "_CustomDepthmapAtlas";
    public string customDepthMatricesIdName = "_CustomDepthMatrices";
    public string customDepthParamsIdName = "_CustomDepthParams";
    public string customDepthParams2IdName = "_CustomDepthParams2";
    public string customDepthPositionIdName = "_CustomDepthPositions";
    public string customDepthSizeIdName = "_CustomDepthmapSize";
    public string customDepthCountIdName = "_CustomDepthCount";
    public string customDepthOffset0IdName = "_CustomDepthOffset0";
    public string customDepthOffset1IdName = "_CustomDepthOffset1";

    public string customDepthKeywordName = "CUSTOM_DEPTH_ON";
    

    public RenderTexture _depthMap;
    public int _sliceRowCount;

    public bool clearPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        customDepthPass.SliceRowCount = _sliceRowCount;
        if(customDepthPass.depthMap == null)
        {
            customDepthPass.depthMap = _depthMap;
        }
            renderer.EnqueuePass(customDepthPass);   
    }

    public void ClearPass()
    {
        customDepthPass = new CustomDepthPass(_renderPassEvent);
        customDepthPass.depthMap = _depthMap;
        customDepthPass.customDepthmapIdName = customDepthmapIdName;
        customDepthPass.customDepthMatricesIdName = customDepthMatricesIdName;
        customDepthPass.customDepthParamsIdName = customDepthParamsIdName;
        customDepthPass.customDepthParams2IdName = customDepthParams2IdName;
        customDepthPass.customDepthPositionIdName = customDepthPositionIdName;
        customDepthPass.customDepthSizeIdName = customDepthSizeIdName;
        customDepthPass.customDepthCountIdName = customDepthCountIdName;
        customDepthPass.customDepthOffset0IdName = customDepthOffset0IdName;
        customDepthPass.customDepthOffset1IdName = customDepthOffset1IdName;

        customDepthPass.customDepthKeywordName = customDepthKeywordName;

        clearPass = false;
    }

    public override void Create()
    {
        if (customDepthPass == null || clearPass)
        {
            ClearPass();
}
    }
}

public partial class CustomDepthPass : ScriptableRenderPass
{
    const int maxDepthCount = 256;

    public string customDepthmapIdName;
    public string customDepthMatricesIdName;
    public string customDepthParamsIdName;
    public string customDepthParams2IdName;
    public string customDepthPositionIdName;
    public string customDepthSizeIdName;
    public string customDepthCountIdName;
    public string customDepthOffset0IdName;
    public string customDepthOffset1IdName;

    public string customDepthKeywordName;

    //public int customDepthmapId;
    //public int customDepthMatricesId;
    //public int customDepthParamsId;
    //public int customDepthParams2Id;
    //public int customDepthPositionId;
    //public int customDepthSizeId;
    //public int customDepthCountId;
    //public int customDepthOffset0Id;
    //public int customDepthOffset1Id;

    //public string customDepthKeyword;


    Matrix4x4[] customDepthMatrices;
    Vector4[] customDepthParams;
    Vector4[] customDepthParams2;
    Vector4[] customDepthPosition;
    bool customDepthOnlyMain;

    public RenderTexture depthMap;
    public int SliceRowCount;
    Dictionary<CustomDepthCamera, CustomDepthProperty> customDepths;

    struct CustomDepthProperty
    {
        public CustomDepthProperty(int _index, bool _isActive)
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

    public void AddCustomDepth(CustomDepthCamera shadow)
    {
        //Debug.Log("shadow id " + shadow.GetInstanceID());
        if (!customDepths.ContainsKey(shadow))
        {
            //Debug.Log("shadow id " + shadow.GetInstanceID()+" added");
            customDepths.Add(shadow, new CustomDepthProperty(customDepths.Count, shadow.enabled || shadow.gameObject.activeInHierarchy));
        }
        else
        {
            //Debug.Log("shadow id " + shadow.GetInstanceID()+" already in directory");
        }
        
    }

    public void RemoveCustomDepth(CustomDepthCamera shadow)
    {
        if (customDepths.ContainsKey(shadow))
        {
            customDepths.Remove(shadow);
        }
    }

    public void ChangeStatusDepth(CustomDepthCamera shadow)
    {
        if (customDepths.ContainsKey(shadow))
        {
            customDepths[shadow].SetActive(shadow.enabled || shadow.gameObject.activeInHierarchy);
        }
    }
    public void ChangeStatusDepth(CustomDepthCamera shadow, bool _active)
    {
        if (customDepths.ContainsKey(shadow))
        {
            customDepths[shadow].SetActive(_active);
        }
    }


    public CustomDepthPass(RenderPassEvent evt)
    {
        base.profilingSampler = new ProfilingSampler(nameof(CustomDepthPass));
        renderPassEvent = evt;

        customDepthMatrices = new Matrix4x4[maxDepthCount];
        customDepthParams = new Vector4[maxDepthCount];
        customDepthParams2 = new Vector4[maxDepthCount];
        customDepthPosition = new Vector4[maxDepthCount];

        if (customDepths == null)
        {
            customDepths = new Dictionary<CustomDepthCamera, CustomDepthProperty>();//<CustomDepthCamera>();
        }

     //     customDepthmapId = Shader.PropertyToID(customDepthmapIdName);
     //customDepthMatricesId = Shader.PropertyToID(customDepthMatricesIdName);
     //customDepthParamsId = Shader.PropertyToID(customDepthParamsIdName);
     //customDepthParams2Id = Shader.PropertyToID(customDepthParams2IdName);
     //customDepthPositionId = Shader.PropertyToID(customDepthPositionIdName);
     //customDepthSizeId = Shader.PropertyToID(customDepthSizeIdName);
     //customDepthCountId = Shader.PropertyToID(customDepthCountIdName);
     //customDepthOffset0Id = Shader.PropertyToID(customDepthOffset0IdName);
     //customDepthOffset1Id = Shader.PropertyToID(customDepthOffset1IdName);

     //customDepthKeyword = customDepthKeywordName;
     
    //Debug.Log("Pass Created");
    //customDepthKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");
}

    //public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    //{
    //    //base.Configure(cmd, cameraTextureDescriptor);
    //    //ConfigureTarget(depthMap);

    //}


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        
        RenderCustomDepths(ref context, ref renderingData);

        ResetDepthParams();
    }

    public void RenderCustomDepths(ref ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = renderingData.commandBuffer;
        if (customDepths==null || SliceRowCount == 0)
        {
            Debug.Log("Disabled custom depth features");
            CoreUtils.SetKeyword(cmd, customDepthKeywordName, false);
        }
        else if (customDepths.Keys.Count > 0)
        {
            Debug.Log("custom depth key counts : " + customDepths.Keys.Count);
            SetCustomDepthMatricesAndParams(ref customDepthMatrices, ref customDepthParams,  ref customDepthPosition, ref customDepthParams2
              );

            cmd.SetGlobalTexture(customDepthmapIdName, depthMap);
            cmd.SetGlobalMatrixArray(customDepthMatricesIdName, customDepthMatrices);
            cmd.SetGlobalVectorArray(customDepthParamsIdName, customDepthParams);
            cmd.SetGlobalVectorArray(customDepthParams2IdName, customDepthParams2);
            cmd.SetGlobalVectorArray(customDepthPositionIdName, customDepthPosition);
            cmd.SetGlobalInteger(customDepthCountIdName, customDepths.Count);
            Vector2 oneDivDepthMapSize = Vector2.one / new Vector2(depthMap.width, depthMap.height);
            cmd.SetGlobalVector(customDepthSizeIdName, new Vector4(oneDivDepthMapSize.x, oneDivDepthMapSize.y, depthMap.width, depthMap.height));

            Vector2Int allocatedDepthAtlasSize = new Vector2Int(depthMap.width, depthMap.height);
            Vector2 invDepthAtlasSize = Vector2.one / allocatedDepthAtlasSize;
            Vector2 invHalfDepthAtlasSize = invDepthAtlasSize * 0.5f;

            cmd.SetGlobalVector(customDepthOffset0IdName,
                    new Vector4(-invHalfDepthAtlasSize.x, -invHalfDepthAtlasSize.y,
                        invHalfDepthAtlasSize.x, -invHalfDepthAtlasSize.y));
            cmd.SetGlobalVector(customDepthOffset1IdName,
                    new Vector4(-invHalfDepthAtlasSize.x, invHalfDepthAtlasSize.y,
                        invHalfDepthAtlasSize.x, invHalfDepthAtlasSize.y));

            //CoreUtils.SetKeywordName(cmd, ShaderKeywordNameStrings.SoftShadows, true);
            CoreUtils.SetKeyword(cmd, customDepthKeywordName, customDepthOnlyMain?false:true);
        }
        else
        {
            Debug.Log("Disabled custom depth features2");
            CoreUtils.SetKeyword(cmd, customDepthKeywordName, false);

        }
        
        
       

    }


    void SetCustomDepthMatricesAndParams(ref Matrix4x4[] matrices, ref Vector4[] shadowParams, ref Vector4[] shadowPos, ref Vector4[] shadowParams2)
    {
        int count = customDepths.Count;

        Matrix4x4 sliceTransform;

        float scaleOffset = 1.0f / (float)(SliceRowCount);
        foreach(CustomDepthCamera shadow in customDepths.Keys)
        {
            int index = customDepths[shadow].index;

            if(shadow == null)
            {
                ChangeStatusDepth(shadow, false);
                shadowParams[index] = Vector4.zero;
                continue;
            }

            if (!customDepths[shadow].isActive||shadow.shadowStrength == 0) {
                shadowParams[index] = Vector4.zero;
                continue; 
            }

            Camera shadowCam = shadow.shadowCamera;

            if(shadowCam == null)
            {
                ChangeStatusDepth(shadow, false);
                shadowParams[index] = Vector4.zero;
                continue;
            }
            shadowCam.ResetProjectionMatrix();
            shadowCam.ResetWorldToCameraMatrix();
            sliceTransform = Matrix4x4.identity;
            sliceTransform.m00 = scaleOffset;
            sliceTransform.m11 = scaleOffset;

            Vector2 offset = SetTileViewport(index, SliceRowCount);

            sliceTransform.m03 = offset.x* scaleOffset;
            sliceTransform.m13 = offset.y* scaleOffset;

            matrices[index] = sliceTransform * ShadowUtils.GetShadowTransform(shadowCam.projectionMatrix, shadowCam.worldToCameraMatrix);
            shadowParams[index] = new Vector4(shadow.shadowStrength, shadow.softShadow?
                (shadow.shadowQuality == SoftShadowQuality.UsePipelineSettings?3:(int)shadow.shadowQuality)
                :0//offset.x* scaleOffset, offset.y* scaleOffset /*shadow.softDepth?1:0*/
                , shadow.bias, shadow.falloffThreshold);
            //shadow.quadOffset = new Vector4(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);
            shadowParams2[index] = new Vector4(offset.x* scaleOffset, offset.x* scaleOffset + scaleOffset, offset.y* scaleOffset, offset.y* scaleOffset + scaleOffset);
            
            Vector3 pos = shadow.frustumSetting.isOrthographic ? -shadow.transform.forward : shadow.transform.position;
            shadowPos[index] = new Vector4(pos.x, pos.y, pos.z, shadow.frustumSetting.isOrthographic?0:1);
        }
    }


    Vector2 SetTileViewport(int index, int split)
    {
        Vector2 offset = new Vector2((index % split), (index / split));

        return offset;
    }

    void ResetDepthParams()
    {
        for (int i = 0; i < maxDepthCount; i++)
        {
            customDepthParams[i] = new Vector4(0, 0, 0, 0);
            customDepthParams2[i] = new Vector4(0, 0, 0, 0);
            customDepthPosition[i] = new Vector4(0, 0, 0, 0);
        }
    }

    //public bool Setup(ref RenderingData renderingData)
    //{
    //    //if (!CustomDepthCameraManager.manager) return false;

    //    if (!depthMap) return false;

    //    if (customDepths.Count <= 0) return false;

    //    return true;
    //}

    //public void Dispose()
    //{

    //}
}
