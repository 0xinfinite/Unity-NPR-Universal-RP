using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomShadowCameraManager))]
public class CustomShadowCameraManagerEditor : Editor
{
    CustomShadowCameraManager manager;

    private void OnEnable()
    {
        manager = target as CustomShadowCameraManager;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Add Custom Shadow"))
        {
            AddCustomShadow();
        }
        if (GUILayout.Button("Assign Custom Shadow"))
        {
            AssignCustomShadows();
        }

        base.OnInspectorGUI();
    }

    public void AddCustomShadow()
    {
        manager.AddCustomShadow();
    }

    public void AssignCustomShadows()
    {
        manager.AssignCustomShadows();
    }
}
