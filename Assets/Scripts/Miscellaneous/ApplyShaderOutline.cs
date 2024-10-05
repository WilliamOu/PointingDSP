// Description: This script is used to apply an outline shader to an object using the imported quick outline shader asset
// Allows barriers and deadzones to be visualized in experimental mode
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: CURRENTLY NOT IN USE DUE TO POTENTIAL PERFORMANCE ISSUES
// WAIT FOR VR INTEGRATION BEFORE RE-IMPLEMENTING

// Require the Outline component, adding it if it doesn't exist
[RequireComponent(typeof(Outline))]
public class ApplyOutlineShader : MonoBehaviour
{
    private Outline outline;

    void Start()
    {
        // Get or add the Outline component
        outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        // Configure and apply the outline based on the experimental mode
        ConfigureOutline();
    }

    private void Update()
    {
        // Check the experimental mode each frame and enable/disable outline accordingly
        if (PersistentDataManager.Instance != null && !PersistentDataManager.Instance.Experimental)
        {
            SetOutlineEnabled(true);
        }
        else
        {
            SetOutlineEnabled(false);
        }
    }

    private void ConfigureOutline()
    {
        // Set the outline properties
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = Color.green; // Set the outline color
        outline.OutlineWidth = 5f; // Set the outline width
    }

    private void SetOutlineEnabled(bool enabled)
    {
        if (outline != null)
        {
            outline.enabled = enabled;
        }
    }
}
