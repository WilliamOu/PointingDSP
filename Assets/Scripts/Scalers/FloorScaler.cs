// Description: This script scales the floor's texture tiling based on the scale of the model
// Since sometimes the height won't scale, the vertical tiling of materials won't be adjusted, thus the floor needs its own material scaler
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScaler : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;  

    private float scale;

    void Start()
    {
        scale = PersistentDataManager.Instance.Scale;

        if (objectRenderer == null)
        {
            objectRenderer = gameObject.GetComponent<MeshRenderer>();
        }
        if (objectRenderer == null)
        {
            Debug.LogError("Renderer cannot be found.");
            return;
        }

        // Adjust the floor's texture tiling
        Vector2 textureScale = objectRenderer.material.mainTextureScale;
        objectRenderer.material.mainTextureScale = new Vector2(textureScale.x * scale, textureScale.y * scale);
    }
}