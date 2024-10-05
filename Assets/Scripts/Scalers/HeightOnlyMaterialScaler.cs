// Description: This script scales the map height for the training phase
// Allows for height scaling to be toggled on and off
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightOnlyMaterialScaler : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;

    private float heightScale;

    void Start()
    {
        heightScale = PersistentDataManager.Instance.HeightScale;

        if (objectRenderer == null)
        {
            Debug.LogError("Renderer not assigned to MaterialScaler.");
            return;
        }

        // Adjust texture tiling
        Vector2 textureScale = objectRenderer.material.mainTextureScale;
        objectRenderer.material.mainTextureScale = new Vector2(textureScale.x, textureScale.y * heightScale);
    }
}