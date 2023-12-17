using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode()]
[RequireComponent (typeof(Camera))]
public class CustomDepthCamera : MonoBehaviour
{
    public CustomDepthCameraManager manager;

    public Camera shadowCamera;

    public FrustumSetting frustumSetting = new FrustumSetting(100f);

    public float bias = 0.1f;
    [Range(0,1)]public float falloffThreshold = 0.1f;

    public MeshRenderer quadRenderer;

    public Vector4 quadOffset;

    public int depthmapWidth = 256;
    public int depthmapHeight = 256;

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

        if( shadowCamera.targetTexture==null || shadowCamera.targetTexture.width != depthmapWidth || shadowCamera.targetTexture.height != depthmapHeight)
        {
            if (shadowCamera.targetTexture != null)
            {
                shadowCamera.targetTexture.Release();
                shadowCamera.targetTexture = null;
            }
            shadowCamera.targetTexture = new RenderTexture(depthmapWidth, depthmapHeight, 16, RenderTextureFormat.Depth);
        }
    }

    private void Start()
    {
        shadowCamera = GetComponent<Camera>();
    }


    // Start is called before the first frame update
    void OnEnable()
    {
        if (manager == null) return;

        if (!manager.customShadows.Contains(this))
        {
            manager.AddCustomDepth(this);
        }
        manager.SetShadowActive(this, true);

        if (quadRenderer!=null)
        quadRenderer.gameObject.SetActive(true);
        SetCameraToFrustumSetting(frustumSetting);
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
            return;

        if (manager != null)
        manager.SetShadowActive(this, false);

        if (quadRenderer != null)
            quadRenderer.gameObject.SetActive(false);
    }

    //private void OnDrawGizmosSelected()
    //{
    //    //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
    //    //Gizmos.DrawFrustum(Vector3.zero, shadowCamera.fieldOfView, shadowCamera.farClipPlane, shadowCamera.nearClipPlane, shadowCamera.aspect);
    //}

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

        if (manager == null) return;

        if (manager.customShadows.Contains(this))
        {
            //manager.customShadows.Remove(this);
            manager.RemoveCustomShadow(this);
            manager.OrientChildQuads();
            //manager.cameras.Remove(shadowCamera);
            
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
