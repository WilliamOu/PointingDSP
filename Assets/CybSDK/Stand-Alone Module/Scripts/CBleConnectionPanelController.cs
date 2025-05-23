using UnityEngine;
using UnityEngine.UI;
using CybSDK;
using System.Collections.Generic;

[RequireComponent(typeof(CBleVirtDevice))]
public class CBleConnectionPanelController : MonoBehaviour
{
    public Button PairedDeviceButtonPrefab;

    private GameObject ConnectionObjectsHolder;
    private GameObject CalibrationObjectsHolder;
    private GameObject SelectionObjectsHolder;
    private GameObject ConnectionFailedObjectsHolder;

    private List<GameObject> panels;

    private Button RetryBtn;

    private CBleVirtDevice virtDevice;

    private Text StatusText;
    private Text SelectionTitleText;
    private Text ConnectionTitleText;
    private Text ConnectionFailedTitleText;

    private string savedVirtSamName;

    private bool _calibrationInstructionsRequired = false;

    private void Awake()
    {
        virtDevice = GetComponent<CBleVirtDevice>();
        virtDevice.OnCBleVirtDeviceCallback += OnCBleVirtDeviceCallback;

        ConnectionObjectsHolder = gameObject.transform.Find("ConnectionObjectsHolder").gameObject;
        CalibrationObjectsHolder = gameObject.transform.Find("CalibrationInstructionsObjectsHolder").gameObject;
        SelectionObjectsHolder = gameObject.transform.Find("SelectionObjectsHolder").gameObject;
        ConnectionFailedObjectsHolder = gameObject.transform.Find("ConnectionFailedObjectsHolder").gameObject;
        RetryBtn = ConnectionFailedObjectsHolder.transform.Find("RetryBtn").GetComponent<Button>();
        RetryBtn.onClick.AddListener(() => virtDevice.ConnectToSavedVirt());

        StatusText = ConnectionObjectsHolder.transform.Find("StatusText").GetComponent<Text>();
        SelectionTitleText = SelectionObjectsHolder.transform.Find("TitleText").GetComponent<Text>();
        ConnectionTitleText = ConnectionObjectsHolder.transform.Find("TitleText").GetComponent<Text>();
        ConnectionFailedTitleText = ConnectionFailedObjectsHolder.transform.Find("TitleText").GetComponent<Text>();

        panels = new List<GameObject> { ConnectionObjectsHolder, CalibrationObjectsHolder, SelectionObjectsHolder, ConnectionFailedObjectsHolder };
    }

    private void Start()
    {
        SwitchToPanel(ConnectionFailedObjectsHolder);
    }

    private void OnCBleVirtDeviceCallback(CBleVirtDevice.States state)
    {
        switch (state)
        {
            case CBleVirtDevice.States.UNKNOWN:
                StatusText.text = "Unknown";
                break;
            case CBleVirtDevice.States.SELECTING:
                // If the saved Virtualizer bluetooth name is not found in the list of paired devices,
                // the user can select a new name from the list of paired bluetooth devices.
                SwitchToPanel(SelectionObjectsHolder);

                savedVirtSamName = virtDevice.GetSavedVirtSamName();
                string[] pairedDevices = virtDevice.GetPairedDevices();
                if (pairedDevices.Length == 1)
                {
                    SelectionTitleText.text = "This headset is not paired with any device. Please, pair first with the Virtualizer and restart this app";
                }
                else
                {
                    SelectionTitleText.text = "The VirtSAM with the name '" + savedVirtSamName + "' could not be found in your paired devices. Please, select another device or pair again:";
                }

                // Generate the interactable list of paired bluetooth devices
                for (int i = 1; i < pairedDevices.Length; i++)
                {
                    Button newPairedDeviceButton = Instantiate(PairedDeviceButtonPrefab) as Button;
                    newPairedDeviceButton.transform.SetParent(SelectionObjectsHolder.transform, false);
                    newPairedDeviceButton.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 50 - (i - 1) * 70, 0);
                    newPairedDeviceButton.GetComponentInChildren<Text>().text = pairedDevices[i];
                    int index = i;
                    newPairedDeviceButton.onClick.AddListener(delegate { virtDevice.ConnectToAndSave(pairedDevices[index]); });
                }
                break;
            case CBleVirtDevice.States.CONNECTING:
                SwitchToPanel(ConnectionObjectsHolder);
                StatusText.text = "Connecting";

                savedVirtSamName = virtDevice.GetSavedVirtSamName();
                ConnectionTitleText.text = "Setting up Virtualizer connection with '" + savedVirtSamName + "', please wait...";
                break;
            case CBleVirtDevice.States.CONNECTION_FAILED:
                gameObject.SetActive(true);
                SwitchToPanel(ConnectionFailedObjectsHolder);

                if (virtDevice.IsBluetoothPermissionGranted())
                {
                    RetryBtn.onClick.RemoveAllListeners();
                    RetryBtn.onClick.AddListener(() => virtDevice.ConnectToSavedVirt());
                    ConnectionFailedTitleText.text = "Connection with '" + savedVirtSamName + "' failed!";
                }
                else
                {
                    RetryBtn.onClick.RemoveAllListeners();
                    RetryBtn.onClick.AddListener(() => virtDevice.RequestBluetoothPermission());
                    ConnectionFailedTitleText.text = "Bluetooth permission is required to connect with the Virtualizer.\nPlease, allow this in the 'Nearby Devices' permissions settings or retry.";
                }
                break;
            case CBleVirtDevice.States.CONNECTED:
                StatusText.text = "Connected";
                break;
            case CBleVirtDevice.States.INITIALIZING_DATA:
                // The HMD is retrieving and subscribing to the data from the Virtualizer connected by Bluetooth.
                StatusText.text = "Initializing Data";
                break;
            case CBleVirtDevice.States.ACTIVE:
                StatusText.text = "Setup Successful";
                if (_calibrationInstructionsRequired)
                    SwitchToPanel(CalibrationObjectsHolder);
                else // If not required, the setup is finished
                    gameObject.GetComponent<Canvas>().enabled = false;
                break;
        }
    }

    /// <summary>
    /// Sets if instructions for calibration are required. Only needed if there is an issue with the calibration.
    /// </summary>
    public void SetCalibrationInstructionsRequired(bool calibrationInstructionsRequired)
    {
        _calibrationInstructionsRequired = calibrationInstructionsRequired;
    }

    private void SwitchToPanel(GameObject panel)
    {
        // Iterate through all panels and switch off all panels except the one that is passed as an argument
        foreach (GameObject p in panels)
        {
            if (panel.Equals(p))
            {
                p.SetActive(true);
            }
            else
            {
                p.SetActive(false);
            }
        }
    }
}
