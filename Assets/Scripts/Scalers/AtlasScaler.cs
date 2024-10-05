// Description: This script is a custom material scaler specifically for the texture atlas
// Invoked via the world script, immediately following the loading of the texture atlas
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlasScaler : MonoBehaviour
{
    private float scale;
    private float heightScale;

    public void ScaleAtlasMaterial(Material material)
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = PersistentDataManager.Instance.HeightScale;

        if (material == null)
        {
            Debug.LogError("Material passed to ScaleAtlasMaterial is null.");
            return;
        }

        // Adjust texture tiling
        Vector2 textureScale = material.mainTextureScale;
        material.mainTextureScale = new Vector2(textureScale.x * scale, textureScale.y * heightScale);
    }
}