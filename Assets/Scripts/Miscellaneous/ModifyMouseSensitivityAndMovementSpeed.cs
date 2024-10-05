// Description: This script defines the modify mode that controls whether the player can modify the mouse sensitivity or movement speed
// Used for clarity
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModifyMouseSensitivityAndMovementSpeed : MonoBehaviour
{
    [SerializeField] private TMP_Text mouseSensitivityText;
    [SerializeField] private TMP_Text movementSpeedText;
    [SerializeField] private TMP_Text currentModeText;

    private MouseLook mouseLook;
    private PlayerMovement playerMovement;

    private enum ModifyMode
    {
        MouseSensitivity,
        MovementSpeed
    }

    private ModifyMode modify = ModifyMode.MouseSensitivity;

    void Start()
    {
        mouseLook = FindObjectOfType<MouseLook>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        UpdateMouseSensitivityAndMovementSpeed();
    }

    void UpdateMouseSensitivityAndMovementSpeed()
    {
        if (PersistentDataManager.Instance.Experimental && !PersistentDataManager.Instance.IsVR && PersistentDataManager.Instance.GameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                modify = ModifyMode.MouseSensitivity;
                UpdateCurrentModeText();
            }
            else if (Input.GetKeyDown(KeyCode.Period))
            {
                modify = ModifyMode.MovementSpeed;
                UpdateCurrentModeText();
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                AdjustSetting(true);
            }
            else if (scroll < 0)
            {
                AdjustSetting(false);
            }
        }
    }

    void AdjustSetting(bool increase)
    {
        if (modify == ModifyMode.MouseSensitivity)
        {
            mouseLook.AdjustSensitivity(increase);
            UpdateSensitivityText();
        }
        else if (modify == ModifyMode.MovementSpeed)
        {
            playerMovement.AdjustMovementSpeed(increase);
            UpdateMovementSpeedText();
        }
    }

    void UpdateSensitivityText()
    {
        if (mouseSensitivityText != null)
        {
            mouseSensitivityText.text = $"Mouse Sensitivity:\n{mouseLook.MouseSensitivity:F1}";
        }
    }

    void UpdateMovementSpeedText()
    {
        if (movementSpeedText != null)
        {
            movementSpeedText.text = $"Movement Speed:\n{playerMovement.Speed:F1}";
        }
    }

    void UpdateCurrentModeText()
    {
        if (currentModeText != null)
        {
            string modeText = modify == ModifyMode.MouseSensitivity ? "Modifying Mouse Sensitivity" : "Modifying Movement Speed";
            currentModeText.text = modeText;
        }
    }
}