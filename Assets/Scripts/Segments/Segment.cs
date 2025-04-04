using UnityEngine;

namespace Segments
{
    public abstract class Segment : MonoBehaviour
    {
        public abstract void GenerateObject();
    
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

        public abstract void ReturnOject(GameObject obj);
    }
}

