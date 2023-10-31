using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        _light = GetComponent<Light>();
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
