using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class CachedShadowmapManager : MonoBehaviour
{
    public static CachedShadowmapManager manager;


    static int cachedShadowmapId = Shader.PropertyToID("_CachedShadowmapAtlas");
    //private GlobalKeyword cachedShadowKeyword;

    private Dictionary<int, int> lightNumDict;

    private Dictionary<Light,int> lightIdDict;

    private Dictionary<Light, CachedShadowmap> lightShadowDict;

    public Dictionary<Light,CachedShadowmap> LightShadowDict { get { return lightShadowDict; } }

    public Dictionary<Light,int> LightIdDict { get { return lightIdDict; } }

    public Dictionary<int, int> LightNumDict { get { return lightNumDict; } }

    [SerializeField] Texture2D cachedShadowmapAtlas;
    public Texture2D CachedShadowmapAtlas { get { return cachedShadowmapAtlas; } }

    [SerializeField]
    private int shadowmapCount;

    public int ShadowmapCount { get { return shadowmapCount; } }

    [SerializeField] private int sliceRowCount = 1;

    public int SliceRowCount { get { return sliceRowCount; } }

    public Vector4 offset;
    public bool toggle;

    private void Awake()
    {
        manager = this;

        InitComponent();
    }

    public void InitComponent()
    {
        lightIdDict = new Dictionary<Light, int>();
        lightNumDict = new Dictionary<int, int>();
        lightShadowDict = new Dictionary<Light, CachedShadowmap>();
        //cachedShadowKeyword = GlobalKeyword.Create("CACHED_SHADOW_ON");

        CachedShadowmap[] cachedShadows = GameObject.FindObjectsOfType<CachedShadowmap>();


        foreach (CachedShadowmap shadow in cachedShadows)
        {
            lightIdDict.Add(shadow._Light, shadow._Light.GetInstanceID());
            lightNumDict.Add(shadow._Light.GetInstanceID(), shadow.LightNum);
            lightShadowDict.Add(shadow._Light, shadow);
            //switch (shadow._Light.type)
            //{
            //    case LightType.Spot:
            //        shadowmapCount += 1;
            //        break;
            //    case LightType.Point:
            //        shadowmapCount += 6;
            //        break;
            //}


        }
    }

    //private void OnEnable()
    //{
        //if (lightNumDict != null && lightNumDict.Count > 0)
        //{
        //    Shader.SetKeyword(cachedShadowKeyword, true); 
        //}
    //}

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Shader.SetGlobalTexture(cachedShadowmapId, cachedShadowmapAtlas);

     //   SetCachedShadowmapMatrices();
    }

    //void SetCachedShadowmapMatrices()
    //{
        //Matrix4x4 sliceTransform;
        //for (int globalShadowSliceIndex = 0; globalShadowSliceIndex < shadowmapCount; ++globalShadowSliceIndex)
        //{
        //    int additionalLightIndex = m_ShadowSliceToAdditionalLightIndex[globalShadowSliceIndex];

        //    // We can skip the slice if strength is zero.
        //    if (Mathf.Approximately(m_AdditionalLightIndexToShadowParams[additionalLightIndex].x, 0.0f) || Mathf.Approximately(m_AdditionalLightIndexToShadowParams[additionalLightIndex].w, -1.0f))
        //        continue;

        //    int visibleLightIndex = m_AdditionalLightIndexToVisibleLightIndex[additionalLightIndex];
        //    int sortedShadowResolutionRequestFirstSliceIndex = m_VisibleLightIndexToSortedShadowResolutionRequestsFirstSliceIndex[visibleLightIndex];
        //    int perLightSliceIndex = m_GlobalShadowSliceIndexToPerLightShadowSliceIndex[globalShadowSliceIndex];
        //    var shadowResolutionRequest = m_SortedShadowResolutionRequests[sortedShadowResolutionRequestFirstSliceIndex + perLightSliceIndex];
        //    int sliceResolution = shadowResolutionRequest.allocatedResolution;

        //    sliceTransform = Matrix4x4.identity;
        //    sliceTransform.m00 = sliceResolution * oneOverAtlasWidth;
        //    sliceTransform.m11 = sliceResolution * oneOverAtlasHeight;

        //    m_AdditionalLightsShadowSlices[globalShadowSliceIndex].offsetX = shadowResolutionRequest.offsetX;
        //    m_AdditionalLightsShadowSlices[globalShadowSliceIndex].offsetY = shadowResolutionRequest.offsetY;
        //    m_AdditionalLightsShadowSlices[globalShadowSliceIndex].resolution = sliceResolution;

        //    sliceTransform.m03 = m_AdditionalLightsShadowSlices[globalShadowSliceIndex].offsetX * oneOverAtlasWidth;
        //    sliceTransform.m13 = m_AdditionalLightsShadowSlices[globalShadowSliceIndex].offsetY * oneOverAtlasHeight;

        //    // We bake scale and bias to each shadow map in the atlas in the matrix.
        //    // saves some instructions in shader.
        //    m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[globalShadowSliceIndex] = sliceTransform * m_AdditionalLightShadowSliceIndexTo_WorldShadowMatrix[globalShadowSliceIndex];
        //}
    //}

    //private void OnDisable()
    //{
    //    Shader.SetKeyword(cachedShadowKeyword, false);
    //}
}
