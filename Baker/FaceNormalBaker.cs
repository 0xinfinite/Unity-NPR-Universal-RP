using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum NormalFixType
{
    Sphere = 0,
    Capsule = 1
}

[ExecuteInEditMode()]
public class FaceNormalBaker : MonoBehaviour
{
    public SkinnedMeshRenderer faceRenderer;

    public Transform axis;

    public NormalFixType fixType;

    private void OnEnable()
    {
        if (axis == null || faceRenderer == null)
        {
            this.enabled = false;
            return;
        }

        FixFaceNormals();

        this.enabled = false;
    }

    public void FixFaceNormals()
    {
        Mesh newMesh = DuplicateMesh(faceRenderer.sharedMesh, faceRenderer.transform.InverseTransformPoint(axis.position));

      

        faceRenderer.sharedMesh = newMesh;
    }

    private void OnDrawGizmosSelected()
    {
        if (axis == null || faceRenderer == null)
        {
            return;
        }
        Mesh mesh = faceRenderer.sharedMesh;

        foreach(Vector3 vertex in mesh.vertices)
        {
            Gizmos.DrawLine(faceRenderer.transform.TransformPoint(vertex),
                faceRenderer.transform.TransformPoint((vertex-faceRenderer.transform.InverseTransformPoint(axis.position)).normalized));
        }
    }

    private Mesh DuplicateMesh(Mesh sourceMesh)
    {
        Mesh targetMesh = new Mesh();
        targetMesh.name = sourceMesh.name + "_Deformed";
        targetMesh.vertices = sourceMesh.vertices;
        targetMesh.normals = sourceMesh.normals;
        targetMesh.tangents = sourceMesh.tangents;
        targetMesh.triangles = sourceMesh.triangles;
        targetMesh.uv = sourceMesh.uv;
        targetMesh.colors = sourceMesh.colors;
        targetMesh.bindposes = sourceMesh.bindposes;
        targetMesh.boneWeights = sourceMesh.boneWeights;
        targetMesh.bounds = sourceMesh.bounds;

        targetMesh.subMeshCount = sourceMesh.subMeshCount;
        for (int i = 0; i < targetMesh.subMeshCount; ++i)
        {
            targetMesh.SetSubMesh(i, sourceMesh.GetSubMesh(i));
        }

        return targetMesh;
    }

    private Mesh DuplicateMesh(Mesh sourceMesh, Vector3 axisPos)
    {
        Mesh targetMesh = new Mesh();
        targetMesh.name = sourceMesh.name + "_Deformed";
        targetMesh.vertices = sourceMesh.vertices;
        targetMesh.normals = new Vector3[sourceMesh.normals.Length];
        for(int i = 0; i < sourceMesh.normals.Length; ++i)
        {
            targetMesh.normals[i] = (sourceMesh.vertices[i] - axisPos).normalized;
        }

        targetMesh.tangents = sourceMesh.tangents;
        targetMesh.triangles = sourceMesh.triangles;
        targetMesh.uv = sourceMesh.uv;
        targetMesh.colors = sourceMesh.colors;
        targetMesh.bindposes = sourceMesh.bindposes;
        targetMesh.boneWeights = sourceMesh.boneWeights;
        targetMesh.bounds = sourceMesh.bounds;

        targetMesh.subMeshCount = sourceMesh.subMeshCount;
        for (int i = 0; i < targetMesh.subMeshCount; ++i)
        {
            targetMesh.SetSubMesh(i, sourceMesh.GetSubMesh(i));
        }

        return targetMesh;
    }
}
