// Description: This scene controls whether or not the game will show shadows
// It works by disabling the main light source from casting shadows
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowControl : MonoBehaviour
{
    private Light directionalLight;

    void Start()
    {
        // Get the Light component attached to this GameObject
        directionalLight = GetComponent<Light>();

        // Set the shadow type based on the useShadows setting
        UpdateShadowType();
    }

    void UpdateShadowType()
    {
        if (PersistentDataManager.Instance != null && directionalLight != null)
        {
            if (PersistentDataManager.Instance.UseShadows)
            {
                // Enable soft shadows
                directionalLight.shadows = LightShadows.Soft;
            }
            else
            {
                // Disable shadows
                directionalLight.shadows = LightShadows.None;
            }
        }
    }
}
