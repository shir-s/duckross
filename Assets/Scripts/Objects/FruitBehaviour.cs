using UnityEngine;

public class Fruit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if(other.CompareTag("Player"))
        {
            string fruitTag = name.Replace("(Clone)", "").Trim();
            ObjectPoolManager.Instance.ReturnObjectToPool(fruitTag, gameObject);
        }
    }
}