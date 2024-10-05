// Description: This script disables the VR crosshair
// VR crosshair is only used in the pointing scene to help the player more accurately determine what they're looking at
using UnityEngine;
using UnityEngine.UI;

public class DotCrosshair : MonoBehaviour
{
    private GameObject crosshair;

    private void Start()
    {
        crosshair = (!PersistentDataManager.Instance.IsVR) ? GameObject.Find("Reticle") : GameObject.Find("Dot Crosshair");
    }

    public void Activate()
    {
        if (!crosshair) { crosshair = (!PersistentDataManager.Instance.IsVR) ? GameObject.Find("Reticle") : GameObject.Find("Dot Crosshair"); }

        if (crosshair != null) { crosshair.SetActive(true); }
        else { Debug.LogError("Dot Crosshair object not found in Activate!"); }
    }

    public void Deactivate()
    {
        if (!crosshair) { crosshair = (!PersistentDataManager.Instance.IsVR) ? GameObject.Find("Reticle") : GameObject.Find("Dot Crosshair"); }

        if (crosshair != null) { crosshair.SetActive(false); }
        else { Debug.LogError("Dot Crosshair object not found in Deactivate!"); }
    }
}