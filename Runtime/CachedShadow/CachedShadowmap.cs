using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode()]
public class CachedShadowmap : MonoBehaviour
{
    //static int cachedShadowmapId = Shader.PropertyToID("_CachedShadowmap");
    //static int cachedShadowId = Shader.PropertyToID("_CachedShadow");
//    static int cachedShadowOffsetId = Shader.PropertyToID("_CachedShadowOffset");

//    [SerializeField] Vector4 offsets;

   // [SerializeField]
   // private Texture2D shadowmap;

    private Light _light;
    public Light _Light { get {
            if(_light == null)
            {
                _light = GetComponent<Light>();
            }
            return _light; } }

    public CachedShadowmapManager manager { get { return CachedShadowmapManager.manager; } }

    [SerializeField]
    private int lightNum;

    public int LightNum { get { return lightNum; } }

//    private Matrix4x4[] shadowTransforms;

    public Matrix4x4 GetShadowTransform(CubemapFace face)
    {
        //if (shadowTransforms == null)
        //{
        //    Awake();
        //}

        Matrix4x4 shaderMat = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(_light.spotAngle, 1, _light.shadowNearPlane, _light.range), true);
        if (_light.type == LightType.Spot)
        {
            return ShadowUtils.GetShadowTransform(shaderMat, transform.localToWorldMatrix);
        }
        switch (face)
        {
            case CubemapFace.PositiveX:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.right, Vector3.up), Vector3.one));
                break;
            case CubemapFace.NegativeX:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.left, Vector3.up), Vector3.one));
                
            case CubemapFace.PositiveY:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.up, Vector3.forward), Vector3.one));
                
            case CubemapFace.NegativeY:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.down, Vector3.forward), Vector3.one));
                
            case CubemapFace.PositiveZ:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.forward, Vector3.up), Vector3.one));
                
            case CubemapFace.NegativeZ:
                return ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.back, Vector3.up), Vector3.one));
                
        }
        return Matrix4x4.identity;
    }

    private void Awake()
    {
        _light = GetComponent<Light>();

        
        //Matrix4x4 shaderMat = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(_light.spotAngle, 1, _light.shadowNearPlane, _light.range), true);
        //if (_light.type == LightType.Spot)
        //{
        //    shadowTransforms = new Matrix4x4[1];
        //    shadowTransforms[0] = ShadowUtils.GetShadowTransform(shaderMat, transform.localToWorldMatrix);
        //}
        //else
        //{
        //    shadowTransforms = new Matrix4x4[6];
        //    shadowTransforms[0] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.right , Vector3.up) , Vector3.one));
        //    shadowTransforms[1] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.left , Vector3.up), Vector3.one));
        //    shadowTransforms[2] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.up , Vector3.forward), Vector3.one));
        //    shadowTransforms[3] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.down , Vector3.forward), Vector3.one));
        //    shadowTransforms[4] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.forward , Vector3.up), Vector3.one));
        //    shadowTransforms[5] = ShadowUtils.GetShadowTransform(shaderMat, Matrix4x4.TRS(transform.position, Quaternion.LookRotation(Vector3.back , Vector3.up), Vector3.one));
        //}
    }


    // Update is called once per frame
    //void LateUpdate()
    //{
       // Shader.SetGlobalTexture(cachedShadowmapId, shadowmap);
       // Matrix4x4 shaderMat = GL.GetGPUProjectionMatrix(Matrix4x4.Perspective(_light.spotAngle, 1, _light.shadowNearPlane, _light.range), true);
       // Shader.SetGlobalMatrix(cachedShadowId, ShadowUtils.GetShadowTransform( shaderMat, transform.localToWorldMatrix ) );
        //Shader.SetGlobalVector(cachedShadowOffsetId, offsets);
    //}
}
