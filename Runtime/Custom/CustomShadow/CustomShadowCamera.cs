using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode()]
[RequireComponent (typeof(Camera))]
public class CustomShadowCamera : MonoBehaviour
{
    //public Shader depthDecoder;

    public Camera shadowCamera;

    public FrustumSetting frustumSetting = new FrustumSetting(100f);


    public float bias = 0.1f;
    [Range(0,1)]public float falloffThreshold = 0.1f;

    //public CommandBuffer buffer;

    public MeshRenderer quadRenderer;

    public Vector4 quadOffset;

    public int depthmapResolution = 256;

    [Range(0,1)]public float shadowStrength = 1;

    public bool softShadow = true;
    public SoftShadowQuality shadowQuality = SoftShadowQuality.Low;

    public void SetCameraToFrustumSetting(FrustumSetting setting)
    {
        shadowCamera.orthographic = setting.isOrthographic;
        if (setting.isOrthographic)
        {
            shadowCamera.orthographicSize = setting.orthoSize;
        }
        else
        {
            shadowCamera.fieldOfView = setting.fov;
        }
        shadowCamera.farClipPlane = setting.range;
        shadowCamera.nearClipPlane = setting.nearPlane;
    }

    private void Awake()
    {
        
        shadowCamera = GetComponent<Camera>();

        OnValidate();
    }

    void OnValidate()
    {
        if (shadowCamera == null)
        {
            shadowCamera = GetComponent<Camera>();
        }
            SetCameraToFrustumSetting(frustumSetting);

        //if (CustomShadowCameraManager.manager != null)
        //{
        //    if (CustomShadowCameraManager.manager.depthDecoder != null)
        //    {
        //        depthDecoder = CustomShadowCameraManager.manager.depthDecoder;
        //    }
        //}


        if( shadowCamera.targetTexture==null || shadowCamera.targetTexture.width != depthmapResolution)
        {
            if (shadowCamera.targetTexture != null)
            {
                shadowCamera.targetTexture.Release();
                shadowCamera.targetTexture = null;
            }
            shadowCamera.targetTexture = new RenderTexture(depthmapResolution, depthmapResolution, 16, RenderTextureFormat.Depth);
            //if (quadRenderer.sharedMaterial.shader == CustomShadowCameraManager.manager.depthDecoder)
            //{
            //    quadRenderer.sharedMaterial.SetTexture("_BaseMap", shadowCamera.targetTexture);
            //}
        }
    }

    private void Start()
    {
        shadowCamera = GetComponent<Camera>();
        //shadowCamera.enabled = false;

    }


    // Start is called before the first frame update
    void OnEnable()
    {
        if (CustomShadowCameraManager.manager == null) return;

        if (!CustomShadowCameraManager.manager.customShadows.Contains(this))
        {
            //CustomShadowCameraManager.manager.customShadows.Add(this);
            CustomShadowCameraManager.manager.AddCustomShadow(this);
            //CustomShadowCameraManager.manager.cameras.Add(shadowCamera);
        }
        CustomShadowCameraManager.manager.SetShadowActive(this, true);

        if (quadRenderer!=null)
        quadRenderer.gameObject.SetActive(true);
        SetCameraToFrustumSetting(frustumSetting);
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
            return;

        if (CustomShadowCameraManager.manager != null)
        CustomShadowCameraManager.manager.SetShadowActive(this, false);

        if (quadRenderer != null)
            quadRenderer.gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //Gizmos.DrawFrustum(Vector3.zero, shadowCamera.fieldOfView, shadowCamera.farClipPlane, shadowCamera.nearClipPlane, shadowCamera.aspect);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;
        if (quadRenderer!= null)
        {
            DestroyImmediate(quadRenderer.gameObject);
        }

        if (shadowCamera.targetTexture != null)
        {
            shadowCamera.targetTexture.Release();
            shadowCamera.targetTexture = null;
        }

        if (CustomShadowCameraManager.manager == null) return;

        if (CustomShadowCameraManager.manager.customShadows.Contains(this))
        {
            //CustomShadowCameraManager.manager.customShadows.Remove(this);
            CustomShadowCameraManager.manager.RemoveCustomShadow(this);
            CustomShadowCameraManager.manager.OrientChildQuads();
            //CustomShadowCameraManager.manager.cameras.Remove(shadowCamera);
            
        }

    }

    private void LateUpdate()
    {
        if (quadRenderer)
        {
            quadRenderer.sharedMaterial.SetTexture("_BaseMap", shadowCamera.targetTexture);
        }
    }
}
