using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Windows.WebCam;
//using Nothke.Utils;

[System.Serializable]
public struct FrustumSetting
{
    public float range;
    public float nearPlane;
    public bool isOrthographic;
    public float fov;
    public float orthoSize;

    public FrustumSetting(float _range)
    {
        range = _range;
        nearPlane = 0.1f;
        isOrthographic = false;
        fov = 60;
        orthoSize = 5;
    }
}

[ExecuteInEditMode()]
[RequireComponent(typeof(Camera))]
public class AdditionalShadowCameraManager : MonoBehaviour
{
    public static AdditionalShadowCameraManager manager;

    public Camera captureCamera;

    public List<AdditionalShadowCamera> addtionalShadows = new List<AdditionalShadowCamera>();

    public RenderTexture depthMap;

    public Shader depthDecoder;

    public LayerMask layerMask = 268435456;

    private MeshRenderer previewQuad;

    [SerializeField]
    private int sliceRowCount = 1;
    public int SliceRowCount { get { return sliceRowCount; } }

    const int maxShadowCount = 256;

    public Vector4 offset;

    private GameObject tempQuad;

    // Start is called before the first frame update
    void Awake()
    {
        if (manager == null)
        {
            manager = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        gameObject.layer = LayerMask.NameToLayer("DepthDecoder");

        if(captureCamera == null)   
        captureCamera = GetComponent<Camera>();

        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");
        if (!TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData _))
        {
            gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(1);
        }
        addtionalShadows = new List<AdditionalShadowCamera>();
        //cameras = new List<Camera>();

        //if (depthMap == null)
        //{
        //    depthMap = new RenderTexture(2048, 2048, 16, RenderTextureFormat.Depth);
        //}

        //captureCamera.targetTexture = depthMap;
        captureCamera.depth = 2;
        captureCamera.clearFlags = CameraClearFlags.Nothing;
        captureCamera.farClipPlane = 1.0f;
        //foreach (AdditionalShadowCamera shadow in shadows)
        //{

        //    if (shadow.shadowCamera.targetTexture)
        //    {
        //        shadow.shadowCamera.targetTexture.Release();
        //        shadow.shadowCamera.targetTexture = null;
        //    }
        //        shadow.shadowCamera.targetTexture = new RenderTexture(depthMap.width / sliceRowCount, depthMap.height / sliceRowCount,
        //            16, RenderTextureFormat.Depth );
        //}


        //customShadowMatrices = new Matrix4x4[maxShadowCount];
        //customShadowParams = new Vector4[maxShadowCount];
        //customShadowKeyword = GlobalKeyword.Create("CUSTOM_SHADOW_ON");

        //light = FindAnyObjectByType<Light>();
        if (previewQuad == null)
        {
            tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gameObject.AddComponent<MeshFilter>();
            previewQuad = gameObject.AddComponent<MeshRenderer>();
           
        }

        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);

        addtionalShadows = new List<AdditionalShadowCamera>();

        AdditionalShadowCamera[] shadows = GameObject.FindObjectsOfType<AdditionalShadowCamera>();

        if (shadows.Length <= 0)
            return;

        addtionalShadows.AddRange(shadows);


        //OnEnable();
    }

    private void Start()
    {
        if (tempQuad != null)
        {
            GetComponent<MeshFilter>().sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
            DestroyImmediate(tempQuad);
        }
        this.enabled = true;
    }

    Vector2 SetTileViewport(int index, int split)
    {
        Vector2 offset = new Vector2((index % split), (index / split));

        return offset;
    }

    private void OnValidate()
    {
        sliceRowCount = GetRowCount();
        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");

        if (previewQuad == null)
        {
            if(TryGetComponent<MeshRenderer>(out previewQuad))
            {

            }
        }

        if(depthDecoder != null && depthMap != null && previewQuad !=null
            && (previewQuad.sharedMaterial == null|| previewQuad.sharedMaterials.Length<=0 || previewQuad.sharedMaterials[0]==null))
        {
            Material newMat = new Material(depthDecoder);
            newMat.SetTexture("_BaseMap", depthMap);
            previewQuad.sharedMaterial = newMat;
        }
    }

    private void OnEnable()
    {
        OrientChildQuads();
        captureCamera.cullingMask = layerMask;// LayerMask.NameToLayer("DepthDecoder");

    }

    public void OrientChildQuads()
    {
        sliceRowCount = GetRowCount();

        float scaleOffset = 1.0f / (float)SliceRowCount;
        for (int i = 0; i < addtionalShadows.Count; ++i)// (AdditionalShadowCamera shadow in addtionalShadows)
        {
            AdditionalShadowCamera shadow = addtionalShadows[i];

            //shadow.shadowCamera.enabled = true;
            Vector2 offset = SetTileViewport(i, sliceRowCount);
            //shadow.shadowCamera.rect = //new Rect(0, 0, 1, 1);//
            //                                                new Rect(offset.x * scaleOffset, offset.y * scaleOffset, scaleOffset, scaleOffset);

            shadow.quadRenderer.transform.localScale = Vector3.one * scaleOffset;
            shadow.quadRenderer.transform.localPosition = new Vector3(offset.x * scaleOffset + scaleOffset * 0.5f, offset.y * scaleOffset + scaleOffset * 0.5f, 0.5f);
        }

        captureCamera.projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 1);
    }

    private void OnDisable()
    {
        //depthMap.Release();
        //Shader.DisableKeyword(customShadowKeyword);
        captureCamera.ResetProjectionMatrix();
    }

    private void OnDestroy()
    {
        if (manager == this)
        {
            manager = null;
        }
    }

    public void AddCustomShadow()
    {
        if (depthDecoder==null)
        {
            Debug.Log("Depth Decoder Shader not assigned!");
            return;
        }

        GameObject addedGO = new GameObject();
        addedGO.name = "Custom Shadow";
        AdditionalShadowCamera addedCustomShadow = addedGO.AddComponent<AdditionalShadowCamera>();

        addtionalShadows.Add(addedCustomShadow);

        if (addedCustomShadow.quadRenderer == null)
        {
            GameObject quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            addedCustomShadow.quadRenderer = quadGO.GetComponent<MeshRenderer>();
            quadGO.layer = LayerMask.NameToLayer("DepthDecoder");
            
                quadGO.transform.parent = transform;
                quadGO.transform.localPosition = Vector3.zero;
                quadGO.transform.localRotation = Quaternion.identity;
                quadGO.transform.localScale = Vector3.one;
            
            Camera newCam = addedCustomShadow.GetComponent<Camera>();
            newCam.targetTexture = new RenderTexture(256, 256, 16, RenderTextureFormat.Depth);
            Material newMat = new Material(depthDecoder);
            addedCustomShadow.quadRenderer.sharedMaterial = newMat;
            newMat.SetTexture("_BaseMap", newCam.targetTexture);
            //addedCustomShadow.depthDecoder = depthDecoder;
            newCam.clearFlags = CameraClearFlags.Nothing;
            addedCustomShadow.enabled = true;
            addedGO.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
        }

        sliceRowCount = GetRowCount();
        OrientChildQuads();
    }

    int GetRowCount()
    {
        if (addtionalShadows == null) return 0;

        switch (addtionalShadows.Count)
        {
            case 1:
                return 1;
            case 2:
            case 3:
            case 4:
                return 2;
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return 3;
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
                return 4;
        }
        if (addtionalShadows.Count > 25) return 6;

        return 5;
    }
}
