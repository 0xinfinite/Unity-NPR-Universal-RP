using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomDepthCameraManager))]
public class CustomDepthCameraManagerEditor : Editor
{
    CustomDepthCameraManager manager;

    private void OnEnable()
    {
        manager = target as CustomDepthCameraManager;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Add Custom Depth Camera"))
        {
            AddCustomDepth();
        }

        base.OnInspectorGUI();
    }

    public void AddCustomDepth()
    {
        manager.AddCustomDepth();
    }
}
