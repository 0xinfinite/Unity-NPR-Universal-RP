#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.Experimental.Rendering;
using System;
using Unity.VisualScripting;

public class StaticLightShadowBaker : MonoBehaviour
{

    private static StaticLightShadowBaker m_instance = null;

    //private static Shader shader;

    //public static Material material;

    public Material mat;

    private static List<GameObject> offGOList;

    [MenuItem("Tools/Shadows/Turn off Dynamic Game Objects", false)]
    public static void TurnOffDynamics()
    {
        offGOList = new List<GameObject>();

        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject g in go)
        {
            if (g.transform.parent != null || g.CompareTag("MainCamera"))
                continue;

            if (!g.isStatic && g.activeInHierarchy)
            {
                offGOList.Add(g);
                g.SetActive(false);
            }
        }

        Light[] allLights = GameObject.FindObjectsOfType<Light>();

        foreach (Light light in allLights)
        {
            //if (!light.gameObject.isStatic)
            //    light.gameObject.SetActive(false);

            if (!(light.type == LightType.Spot || light.type == LightType.Point) && light.gameObject.activeInHierarchy)
            {
                offGOList.Add(light.gameObject);
                light.gameObject.SetActive(false);
            }
        }
    }

    [MenuItem("Tools/Shadows/Re-Activate Dynamic Game Objects", false)]
    public static void ReactivateDynamics()
    {
        if (offGOList == null) return;

        foreach (GameObject g in offGOList)
        {
            g.SetActive(true);
        }

        offGOList.Clear();
        offGOList = null;

    }
    [MenuItem("Tools/Shadows/Disable Shadowcaster on Statics", false)]
    public static void DisableShadowcasterOnStatics()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject g in go)
        {
            if (g.isStatic)
            {
                if(g.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
        }

    }
    [MenuItem("Tools/Shadows/Enable Shadowcaster on Statics", false)]
    public static void EnableShadowcasterOnStatics()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject g in go)
        {
            if (g.isStatic)
            {
                if (g.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
                {
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                }
            }
        }

    }
}
#endif
