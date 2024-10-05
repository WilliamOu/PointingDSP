// Description: This script handles the data logging for the different scenes
// Note that this script is deprecated because trying to make the data logging work for all the scenes involves unnecessarily convoluted logic as well as making things messier; to be utilized in the future
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    /*[SerializeField] private string csvFileName;
    [SerializeField] private string logHeader;
    [SerializeField] private int phaseNumber;

    private Vector3 playerPosition;
    private Vector3 playerRotation;
    private float playerRotationY;
    private Vector2 gridPosition;
    private string grid;

    private float logInterval = 0.1f;
    private float nextLogTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(IntervalLogging(csvFileName, ));
    }

    // Update is called once per frame
    void Update()
    {
        // I'll need to add logic here to call the data logging
    }

    string header = "TrialNumber,TrialName,GlobalTime,ElapsedTime,Starting,Target,ParticipantX,ParticipantZ,RotationX,RotationY,RotationZ,Grid,VirtualizerAngle";
    string header = "TrialNumber,TrialName,GlobalTime,ElapsedTime,Starting,Target,StartingAngle,TrueAngle,ParticipantX,ParticipantZ,RotationX,RotationY,RotationZ,Grid,Lap,VirtualizerAngle\n";
    string header = "GlobalTime,ParticipantX,ParticipantZ,RotationX,RotationY,RotationZ,Grid,VirtualizerAngle";
    string header = "GlobalTime,ParticipantX,ParticipantZ,RotationX,RotationY,RotationZ,Grid,Lap,VirtualizerAngle";
    string unifiedHeader = "GlobalTime,ElapsedTime,TrialNumber,TrialName,Starting,Target,StartingAngle,TrueAngle,ParticipantX,ParticipantZ,RotationX,RotationY,RotationZ,Grid,Lap,(Remaining VR Stuff...)\n";
    // Wayfinding
    private void IntervalLogging(string csvFileName, GameOjbect player, CVirtPlayerController cyberithVirtualizer = null, CSVData trialData = null)
    {
        if (PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            if (trialStarted)
            {
                trialTimer -= Time.deltaTime;
                elapsedTime += Time.deltaTime;
                UpdateTimerUI();

                if (Time.timeSinceLevelLoad >= nextLogTime)
                {
                    if (PersistentDataManager.Instance.IsVR)
                    {
                        playerPosition = cyberithVirtualizer.transform.position;
                        playerRotation = vrPlayerCamera.transform.eulerAngles;
                        playerRotationY = playerRotation.y;
                    }
                    else
                    {
                        playerPosition = player.transform.position;
                        playerRotation = player.transform.eulerAngles;
                        playerRotationY = player.transform.eulerAngles.y;
                    }

                    gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
                    grid = GridCalculation.GridPositionToLabel(gridPosition);

                    // Format the data line using the string builder, which is more efficient than string concatenation
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5}", playerPosition.x, playerPosition.z, playerRotationX, playerRotation.y, playerRotation.z, grid);

                    // Prepend Pointing Stage specific data
                    if (phaseNumber == 3) { sb.Insert(0, $"{trials[i].StartingAngle},{trials[i].TrueAngle},"); }

                    // Prepend shared data for Pointing and Wayfinding Stages or only GlobalTime
                    if (phaseNumber > 2) { sb.Insert(0, $"{Time.timeSinceLevelLoad:F2},{elapsedTime:F2},{trialCount},{currentTrial.Level},{currentTrial.Starting},{currentTrial.Target},"); }
                    else { sb.Insert(0, $"{Time.timeSinceLevelLoad:F2},"); }

                    // Append Current Lap for Learning Stage
                    if (phaseNumber == 1) { sb.AppendFormat(",{0}", currentLap); }

                    // Append VR data
                    if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0:F2},{1}", cyberithVirtualizer.GlobalOrientation.eulerAngles.y, gazeHandler.GetFullGazeInfo()); }
                    else { sb.AppendLine(); }

                    string printLine = sb.ToString();

                    // Append the data line to the file
                    PersistentDataManager.Instance.AppendToFile(csvFileName, printLine);

                    nextLogTime = Time.timeSinceLevelLoad + logInterval;
                }
            }
        }
    }

    private IEnumerator IntervalLogging(string csvFileName, GameOjbect player, CVirtPlayerController cyberithVirtualizer = null, int lap = null)
    {
        if (PersistentDataManager.Instance.GameStarted && !PersistentDataManager.Instance.GameEnded)
        {
            trialTimer -= Time.deltaTime;
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();

            if (Time.timeSinceLevelLoad >= nextLogTime)
            {
                if (PersistentDataManager.Instance.IsVR)
                {
                    playerPosition = cyberithVirtualizer.transform.position;
                    playerRotation = vrPlayerCamera.transform.eulerAngles;
                    playerRotationY = playerRotation.y;
                }
                else
                {
                    playerPosition = player.transform.position;
                    playerRotation = player.transform.eulerAngles;
                    playerRotationY = player.transform.eulerAngles.y;
                }

                Vector2 gridPosition = GridCalculation.CalculateGridPosition(playerPosition, PersistentDataManager.Instance.Scale);
                string grid = GridCalculation.GridPositionToLabel(gridPosition);

                // Format the data line using the string builder, which is more efficient than string concatenation
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2},{5}", playerPosition.x, playerPosition.z, playerRotationX, playerRotationY, playerRotation.z, grid);

                // Prepend Pointing Stage specific data
                if (phaseNumber == 3) { sb.Insert(0, $"{trials[i].StartingAngle},{trials[i].TrueAngle},"); }

                // Prepend shared data for Pointing and Wayfinding Stages or only GlobalTime
                if (phaseNumber > 2) { sb.Insert(0, $"{Time.timeSinceLevelLoad:F2},{elapsedTime:F2},{trialCount},{currentTrial.Level},{currentTrial.Starting},{currentTrial.Target},"); }
                else { sb.Insert(0, $"{Time.timeSinceLevelLoad:F2},"); }

                // Append Current Lap for Learning Stage
                if (phaseNumber == 1) { sb.AppendFormat(",{0}", currentLap); }

                // Append VR data
                if (PersistentDataManager.Instance.IsVR) { sb.AppendFormat(",{0:F2},{1}", cyberithVirtualizer.GlobalOrientation.eulerAngles.y, gazeHandler.GetFullGazeInfo()); }
                else { sb.AppendLine(); }

                string printLine = sb.ToString();

                // Append the data line to the file
                PersistentDataManager.Instance.AppendToFile(csvFileName, printLine);

                nextLogTime = Time.timeSinceLevelLoad + logInterval;
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null && PersistentDataManager.Instance.Experimental == true)
        {
            // Format the time to display it in seconds with 2 decimal places
            timerText.text = trialTimer.ToString("F0");
        }
    }*/
}
