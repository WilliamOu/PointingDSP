// Description: This script scales the map based on the inputted scale
// Allows for height scaling to be toggled on and off
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapScaler : MonoBehaviour
{
    private float scale;
    private float heightScale;

    void Start()
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = PersistentDataManager.Instance.HeightScale;

        transform.localScale = new Vector3(
            transform.localScale.x * scale,
            transform.localScale.y * heightScale,
            transform.localScale.z * scale
        );
    }
}
