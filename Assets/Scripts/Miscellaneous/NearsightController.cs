// Description: Manages the implementation of fog
// Adjust the transition time (time it takes for the fog to settle in) and the fog density (how much the participant can see) as seen fit
using System.Collections;
using UnityEngine;

public class NearsightController : MonoBehaviour
{
    // Word of caution: It is highly ill-advised to change the fog color
    // Modify at your own risk, or well, I guess amusement
    [SerializeField] private Color nearsightColor = Color.black;

    private Color originalBackgroundColor;
    private CameraClearFlags originalClearFlags;
    private bool isInitialized = false;

    private void InitializeDefaults()
    {
        if (!isInitialized)
        {
            // Store the original camera settings
            originalClearFlags = PlayerManager.Instance.PlayerCamera.clearFlags;
            originalBackgroundColor = PlayerManager.Instance.PlayerCamera.backgroundColor;
            RenderSettings.fogColor = nearsightColor;
            isInitialized = true;
        }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInFogAndSkybox());
    }

    public void FadeOut()
    {
        FadeOutFogAndSkybox();
    }

    public IEnumerator FadeInFogAndSkybox()
    {
        if (PersistentDataManager.Instance.Nearsight)
        {
            InitializeDefaults();

            // Set the camera clear flags to solid color
            PlayerManager.Instance.PlayerCamera.clearFlags = CameraClearFlags.SolidColor;

            float currentDuration = 0f;
            while (currentDuration < PersistentDataManager.Instance.NearsightTransitionTime)
            {
                // Increase elapsed time
                currentDuration += Time.deltaTime;

                // Calculate the current fraction of the total transition time
                float fraction = currentDuration / PersistentDataManager.Instance.NearsightTransitionTime;

                // Update fog density
                RenderSettings.fogDensity = Mathf.Lerp(0, PersistentDataManager.Instance.NearsightTargetFogDensity, fraction);

                // Update camera background color to fade to nearsight color
                PlayerManager.Instance.PlayerCamera.backgroundColor = Color.Lerp(originalBackgroundColor, nearsightColor, fraction);

                yield return null;
            }

            // Ensure the final values are set correctly
            RenderSettings.fogDensity = PersistentDataManager.Instance.NearsightTargetFogDensity;
            PlayerManager.Instance.PlayerCamera.backgroundColor = nearsightColor;
        }
    }

    public void FadeOutFogAndSkybox()
    {
        if (PersistentDataManager.Instance.Nearsight)
        {
            InitializeDefaults();

            // Reset fog density instantly
            RenderSettings.fogDensity = 0;

            // Reset camera settings to original values
            PlayerManager.Instance.PlayerCamera.clearFlags = originalClearFlags;
            PlayerManager.Instance.PlayerCamera.backgroundColor = originalBackgroundColor;
        }
    }
}