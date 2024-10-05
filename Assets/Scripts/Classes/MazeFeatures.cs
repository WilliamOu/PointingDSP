// Description: This script initializes additional features in the maze
// Options are set in the Title Screen
using UnityEngine;

public static class MazeFeatures
{
    public static void Initialize()
    {
        LoadAdditionalWalls();
        UpdateShadowType();
        //AllowTeleport();
    }

    private static void LoadAdditionalWalls()
    {
        GameObject walls = GameObject.Find("Additional Walls");

        if (walls)
        {
            if (PersistentDataManager.Instance.AdditionalWallsForLegacyMap == true)
            {
                walls.SetActive(true);
            }
            else
            {
                walls.SetActive(false);
            }
        }
    }

    private static void UpdateShadowType()
    {
        GameObject light = GameObject.Find("Directional Light");
        Light directionalLight = light.GetComponentInChildren<Light>();

        if (directionalLight)
        {
            if (PersistentDataManager.Instance.UseShadows)
            {
                directionalLight.shadows = LightShadows.Soft;
            }
            else
            {
                directionalLight.shadows = LightShadows.None;
            }
        }
    }

    // This method is currently deprecated as SteamVR's teleportation functionality is prone to breaking between scene transitions
    /*private static void AllowTeleport()
    {
        GameObject teleporting = GameObject.Find("Teleporting");
        GameObject teleportArea = GameObject.Find("TeleportArea");
        if (teleporting && teleportArea)
        {
            if (PersistentDataManager.Instance.Experimental && PersistentDataManager.Instance.IsVR)
            {
                teleporting.SetActive(true);
                teleportArea.SetActive(true);
            }
            else
            {
                teleporting.SetActive(false);
                teleportArea.SetActive(false);
            }
        }
    }*/
}