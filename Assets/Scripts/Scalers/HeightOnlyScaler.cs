// Description: This script scales the map height for the training phase
// Allows for height scaling to be toggled on and off
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightOnlyScaler : MonoBehaviour
{
    private float heightScale;

    void Start()
    {
        heightScale = PersistentDataManager.Instance.HeightScale;

        transform.localScale = new Vector3(
            transform.localScale.x,
            transform.localScale.y * heightScale,
            transform.localScale.z
        );
    }
}