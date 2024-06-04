using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CustomPrefabPool : IPunPrefabPool
{
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabId);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found in Resources: " + prefabId);
            return null;
        }
        return Object.Instantiate(prefab, position, rotation);
    }

    public void Destroy(GameObject gameObject)
    {
        Object.Destroy(gameObject);
    }
}
