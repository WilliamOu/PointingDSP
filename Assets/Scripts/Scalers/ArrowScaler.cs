// Description: This script is specifically used for the arrows in the learning stage
// Only scales the box collider; the arrow itself stays the same size visually
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScaler : MonoBehaviour
{
    private float scale;

    void Start()
    {
        scale = PersistentDataManager.Instance.Scale;

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.size = new Vector3(
                boxCollider.size.x * scale,
                boxCollider.size.y,
                boxCollider.size.z * scale
            );
        }
        else
        {
            Debug.LogWarning("BoxCollider not found on " + gameObject.name);
        }
    }
}
