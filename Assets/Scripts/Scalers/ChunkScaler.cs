// Description: This script is a custom material scaler for scaling chunk textures
// Invoked in the world generation loop in the world script
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkScaler : MonoBehaviour
{
    private float scale;
    private float heightScale;

    public void ScaleChunk(Chunk chunk)
    {
        scale = PersistentDataManager.Instance.Scale;
        heightScale = PersistentDataManager.Instance.HeightScale;

        // Adjust texture tiling
        Vector2 textureScale = chunk.meshRenderer.material.mainTextureScale;
        chunk.meshRenderer.material.mainTextureScale = new Vector2(textureScale.x * scale, textureScale.y * heightScale);
    }
}