// Description: This script scales the material tiling of objects in the scene based on the inputted scale
// This is done so that the textures do not appear stretched when the scale is changed
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaler : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;

    private float scale;
    private float heightScale;

    void Start()
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = PersistentDataManager.Instance.HeightScale;

        if (objectRenderer == null)
        {
            Debug.LogError("Renderer not assigned to MaterialScaler.");
            return;
        }

        // Adjust texture tiling
        Vector2 textureScale = objectRenderer.material.mainTextureScale;
        objectRenderer.material.mainTextureScale = new Vector2(textureScale.x * scale, textureScale.y * heightScale);
    }
}