using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Segment : MonoBehaviour
{
    public abstract void GenerateObstacles();
    
    public float GetZLength()
    {
        return transform.localScale.z;
    }


    // Automatically calculate the segment's width (x-axis length).
    public float GetXLength()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return 0f;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds.size.x;
    }

    public abstract void ReturnObstacle(GameObject obstacle);
}

