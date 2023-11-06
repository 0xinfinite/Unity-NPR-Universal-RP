using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode()]
[RequireComponent (typeof(Camera))]
public class AdditionalShadowCamera : MonoBehaviour
{
    public Camera shadowCamera;

    public FrustumSetting frustumSetting = new FrustumSetting(100f);

    public bool softShadow;

    public float bias;
    [Range(0,1)]public float falloffThreshold = 0.5f;

    //public CommandBuffer buffer;

    public MeshRenderer tempRenderer;

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

    private void Start()
    {
        shadowCamera = GetComponent<Camera>();
        //shadowCamera.enabled = false;

    }

    private void OnValidate()
    {
        SetCameraToFrustumSetting(frustumSetting);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (AdditionalShadowCameraManager.manager == null) return;

        if (!AdditionalShadowCameraManager.manager.addtionalShadows.Contains(this))
        {
            AdditionalShadowCameraManager.manager.addtionalShadows.Add(this);
            //AdditionalShadowCameraManager.manager.cameras.Add(shadowCamera);
        }

        SetCameraToFrustumSetting(frustumSetting);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawFrustum(Vector3.zero, shadowCamera.fieldOfView, shadowCamera.farClipPlane, shadowCamera.nearClipPlane, shadowCamera.aspect);
    }

    private void OnDisable()
    {
        if (AdditionalShadowCameraManager.manager == null) return;

        if (AdditionalShadowCameraManager.manager.addtionalShadows.Contains(this))
        {
            AdditionalShadowCameraManager.manager.addtionalShadows.Remove(this);
            //AdditionalShadowCameraManager.manager.cameras.Remove(shadowCamera);
        }
    }

    private void LateUpdate()
    {
        if (tempRenderer)
        {
            tempRenderer.sharedMaterial.SetTexture("_BaseMap", shadowCamera.targetTexture);
        }
    }
}
