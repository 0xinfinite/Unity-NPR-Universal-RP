using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdditionalShadowCameraManager))]
public class AdditionalShadowCameraManagerEditor : Editor
{
    AdditionalShadowCameraManager manager;

    private void OnEnable()
    {
        manager = target as AdditionalShadowCameraManager;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Add Custom Shadow"))
        {
            AddCustomShadow();
        }

        base.OnInspectorGUI();
    }

    public void AddCustomShadow()
    {
        manager.AddCustomShadow();
    }
}
