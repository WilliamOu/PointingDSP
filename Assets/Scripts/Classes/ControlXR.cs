// Description: This script contains functions used to start and stop the XR systems
// Starting XR for VR compatibility is not as stable as having it on by default and disabling it for desktop applications
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR;
using TMPro;
using Valve.VR;

public static class ControlXR
{
    public static IEnumerator StartVR()
    {
        // Force SteamVR to initialize
        /*SteamVR.Initialize(true);

        while (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            yield return null;*/

        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        yield return null;
    }

    public static IEnumerator StopVR()
    {
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        yield return null;
    }
}