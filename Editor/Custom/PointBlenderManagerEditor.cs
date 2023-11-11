using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PointBlendManager))]
public class PointBlenderManagerEditor : Editor
{
    PointBlendManager manager;

    private void OnEnable()
    {
        manager = (PointBlendManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Add GI Points From Transforms"))
        {
            manager.AddGIPointsTransforms();
        }
    }
}
