using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GIPoint
{
    public Vector3 point;
    public int giIndex;

    public GIPoint(Transform tf)
    {
        point = tf.position;
        giIndex = 0;
    }
}

public class PointBlendManager : MonoBehaviour
{
    public Transform[] pointsTf;

    public GIPoint[] giPoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddGIPointsTransforms()
    {
        List<GIPoint> tempList = new List<GIPoint>();
        tempList.AddRange(giPoints);
        for(int i = 0; i < pointsTf.Length; ++i)
        {
            tempList.Add(new GIPoint(pointsTf[i]));
        }

        giPoints = tempList.ToArray();
        tempList.Clear();
    }
}
