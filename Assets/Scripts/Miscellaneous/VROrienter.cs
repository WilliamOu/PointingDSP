// Description: Spawns a VR Pillar and forces the player to look at it since you can't set the VR camera's look direction after teleporting due to it correlating to real-world look direction
// Also includes Requires the player to walk to a certain location for Roomscale VR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class VROrienter : MonoBehaviour
{
    [SerializeField] private GameObject objectToLookAt;
    [SerializeField] private GameObject objectToWalkTo;
    private GameObject objectInstance;
    private Camera vrCamera;
    private CVirtPlayerController cyberithVirtualizer;
    private TMP_Text vrUIText;
    private bool headAlignedPreviously = false;
    private bool virtualizerAlignedPreviously = false;
    private bool isOrientationComplete = false;
    private bool isArrivalComplete = false;

    void Awake()
    {
        vrCamera = FindObjectOfType<Camera>();
        if (vrCamera == null)
        {
            Debug.LogError("VR Camera not found in the scene.");
            return;
        }
        if (PersistentDataManager.Instance.IsVR)
        {
            cyberithVirtualizer = FindObjectOfType<CVirtPlayerController>();
            if (cyberithVirtualizer == null && !PersistentDataManager.Instance.IsRoomscale)
            {
                Debug.LogError("Cyberith Virtualizer not found in the scene.");
                return;
            }
            FindUI();
        }
    }

    public IEnumerator BeginOrientation(string text, float startX, float startY, float lookX, float lookY, bool useHeightOffset)
    {
        if (PersistentDataManager.Instance.IsRoomscale)
        {
            StartCoroutine(WalkToLocation(startX, startY, useHeightOffset));
            yield return new WaitUntil(() => isArrivalComplete);
        }
        StartCoroutine(OrientVRPlayer(text, lookX, lookY, useHeightOffset));
        yield return new WaitUntil(() => isOrientationComplete);
    }

    public IEnumerator BeginOrientation(CSVData trial)
    {
        if (PersistentDataManager.Instance.IsRoomscale)
        {
            StartCoroutine(WalkToLocationTrial(trial));
            yield return new WaitUntil(() => isArrivalComplete);
        }
        StartCoroutine(OrientVRPlayer(trial, true));
        yield return new WaitUntil(() => isOrientationComplete);
    }

    // Training, Learning, and Retracing Stages
    public IEnumerator OrientVRPlayer(string text, float positionX, float positionZ, bool useHeightOffset)
    {
        // Makes the VR player look at a certain object in the scene in order to orient them in the correct direction
        StartCoroutine(OrientPlayer(positionX, positionZ, useHeightOffset));
        vrUIText.text = "Look at the red pillar";
        yield return new WaitUntil(() => isOrientationComplete);
        PersistentDataManager.Instance.PlayerIsOriented = true;
        vrUIText.text = $"{text}";
    }

    // Pointing and Wayfinding Stages
    public IEnumerator OrientVRPlayer(CSVData trial, bool useHeightOffset)
    {
        // Makes the VR player look at a certain object in the scene in order to orient them in the correct direction
        GameObject startingObject = GameObject.Find(trial.Starting);
        Vector3 startingPosition = startingObject.transform.position;
        StartCoroutine(OrientPlayer(startingPosition.x, startingPosition.z, useHeightOffset));
        vrUIText.text = "Look at the red pillar";
        yield return new WaitUntil(() => isOrientationComplete);
    }

    public IEnumerator OrientPlayer(float x, float z, bool useHeightOffset)
    {
        isOrientationComplete = false;
        // Sets the camera to cull (not show) everything except the UI and the VROrient pillars which are on the VROrient layer
        int vrOrientLayer = LayerMask.NameToLayer("VROrient");
        int uiLayer = LayerMask.NameToLayer("UI");
        vrCamera.cullingMask |= (1 << vrOrientLayer) | (1 << uiLayer);
        Vector3 spawnPosition = new Vector3(x, 3.5f, z);
        if (useHeightOffset) { spawnPosition.y += PersistentDataManager.Instance.SpawnPosition.y; }
        objectInstance = Instantiate(objectToLookAt, spawnPosition, Quaternion.identity);
        if (PersistentDataManager.Instance.Map != "Default Map") {
            objectInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            objectInstance.transform.position = new Vector3(objectInstance.transform.position.x, objectInstance.transform.position.y - 1.75f, objectInstance.transform.position.z);
        }
        objectInstance.layer = LayerMask.NameToLayer("VROrient");
        yield return new WaitUntil(() => IsPlayerLookingAtPillar());
        vrUIText.text = "";
        // A slight delay before the scene starts makes the transition less abrupt
        yield return new WaitForSeconds(PersistentDataManager.Instance.VROrienterPillarResponsiveness);
        Destroy(objectInstance);
        // Set the camera's culling mask to its original state
        vrCamera.cullingMask = ~0;
        // Lets the scene manager know that it can continue
        isOrientationComplete = true;
    }

    private bool IsPlayerLookingAtPillar()
    {
        // Head alignment check
        Vector3 headDirectionToPillar = objectInstance.transform.position - vrCamera.transform.position;
        float headAngle = Vector3.Angle(vrCamera.transform.forward, headDirectionToPillar);
        bool headAligned = (headAngle < PersistentDataManager.Instance.VROrienterRequiredHeadAngle);
        if (!PersistentDataManager.Instance.IsRoomscale)
        {
            // Direct yaw comparison between virtualizer and pillar
            Vector3 directionToPillar = (objectInstance.transform.position - cyberithVirtualizer.transform.position).normalized;
            float pillarYaw = Quaternion.LookRotation(directionToPillar).eulerAngles.y;
            float virtualizerYaw = cyberithVirtualizer.GlobalOrientation.eulerAngles.y;
            float yawDifference = Mathf.Abs(virtualizerYaw - pillarYaw);
            if (yawDifference > 180)
            {
                yawDifference = 360 - yawDifference;
            }
            // Debug.DrawLine(cyberithVirtualizer.transform.position, cyberithVirtualizer.transform.position + virtualizerForwardVector * 5, Color.red, 2f);
            // Debug.DrawLine(cyberithVirtualizer.transform.position, objectInstance.transform.position, Color.blue, 2f);
            // Update UI and previous alignment states
            bool virtualizerAligned = (yawDifference < PersistentDataManager.Instance.VROrienterRequiredVirtualizerAngle);
            UpdateUIAndPreviousState(headAligned, virtualizerAligned);
            return headAligned && virtualizerAligned;
        }
        return headAligned;
    }

    private void UpdateUIAndPreviousState(bool headAligned, bool virtualizerAligned)
    {
        if (vrUIText != null)
        {
            if (headAligned != headAlignedPreviously || virtualizerAligned != virtualizerAlignedPreviously)
            {
                if (headAligned && !virtualizerAligned)
                {
                    vrUIText.text = "Look at the red pillar\nPlease turn your body to face the pillar as well";
                }
                else if (!headAligned && virtualizerAligned)
                {
                    vrUIText.text = "Look at the red pillar\nPlease turn your head to face the pillar as well";
                }
                else
                {
                    vrUIText.text = "Look at the red pillar";
                }
                headAlignedPreviously = headAligned;
                virtualizerAlignedPreviously = virtualizerAligned;
            }
        }
        else
        {
            FindUI();
        }
    }

    // Pointing and Wayfinding Stages
    public IEnumerator WalkToLocationTrial(CSVData trial)
    {
        float startingX = trial.StartingX;
        float startingZ = trial.StartingZ;
        StartCoroutine(WalkToLocation(startingX * PersistentDataManager.Instance.Scale, startingZ * PersistentDataManager.Instance.Scale, true));
        yield return new WaitUntil(() => isArrivalComplete);
    }

    public IEnumerator WalkToLocation(float x, float z, bool useHeightOffset)
    {
        vrUIText.text = "Walk to the red waypoint";
        isArrivalComplete = false;
        // Sets the camera to cull (not show) everything except the UI and the VROrient pillars which are on the VROrient layer
        int vrOrientLayer = LayerMask.NameToLayer("VROrient");
        int uiLayer = LayerMask.NameToLayer("UI");
        vrCamera.cullingMask |= (1 << vrOrientLayer) | (1 << uiLayer);
        Vector3 spawnPosition = new Vector3(x, 2.5f, z);
        if (useHeightOffset) { spawnPosition.y += PersistentDataManager.Instance.SpawnPosition.y; }
        objectInstance = Instantiate(objectToWalkTo, spawnPosition, Quaternion.identity);
        if (PersistentDataManager.Instance.Map != "Default Map")
        {
            objectInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            objectInstance.transform.position = new Vector3(objectInstance.transform.position.x, objectInstance.transform.position.y - 1.25f, objectInstance.transform.position.z);
        }
        objectInstance.layer = LayerMask.NameToLayer("VROrient");
        yield return new WaitUntil(() => IsPlayerCloseToPillar());
        // Adjust to increase or decrease the responsiveness of the VR pillar after looking at it
        yield return new WaitForSeconds(0.5f);
        Destroy(objectInstance);
        // Set the camera's culling mask to its original state
        vrCamera.cullingMask = ~0;
        // Lets the scene manager know that it can continue
        isArrivalComplete = true;
    }

    private bool IsPlayerCloseToPillar()
    {
        if (objectInstance == null)
        {
            Debug.LogError("objectInstance is not set. Cannot determine the player's proximity to the pillar.");
            return false;
        }

        // Use only the x and z coordinates for the distance calculation to ignore the height difference
        Vector3 playerPosition = new Vector3(vrCamera.transform.position.x, 0f, vrCamera.transform.position.z);
        Vector3 pillarPosition = new Vector3(objectInstance.transform.position.x, 0f, objectInstance.transform.position.z);
        float distanceToPillar = Vector3.Distance(playerPosition, pillarPosition);

        return distanceToPillar <= PersistentDataManager.Instance.VROrienterRequiredProximity;
    }

    private void FindUI()
    {
        // Find and assign the VR UI text component
        GameObject vrUITextObject = GameObject.Find("VR UI Text");
        if (vrUITextObject != null)
        {
            vrUIText = vrUITextObject.GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogError("VR UI Text object not found in the scene.");
        }
    }
}