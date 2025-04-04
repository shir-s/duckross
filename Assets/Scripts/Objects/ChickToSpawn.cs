using Pools;
using Segments;
using UnityEngine;

namespace Objects
{
    public class ChickToSpawn : MonoBehaviour
    {
        private Segment spawnerSegment;
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            if(other.CompareTag("Player"))
            {
                string chickTag = name.Replace("(Clone)", "").Trim();
                spawnerSegment.ReturnOject(gameObject);
            }
        }
        
        public void SetSpawner(Segment segment)
        {
            spawnerSegment = segment;
        }
    }
}