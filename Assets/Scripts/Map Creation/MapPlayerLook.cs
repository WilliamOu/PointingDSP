using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapPlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 3f;

    [SerializeField] private TMP_Text selectedBlockText;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform mapCamera;
    [SerializeField] private MapPlayer mapPlayer;

    [SerializeField] private Transform highlightBlock;
    [SerializeField] private Transform placeBlock;
    private World world;

    private byte selectedBlockIndex = 1;
    private float checkIncrement = 0.1f;
    private float reach = 8f;
    private float xRotation = 0f;
    private bool canDestroy = false;

    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    void Update()
    {
        if (mapPlayer.CurrentMode ==  MapPlayer.Mode.MapEdit)
        {
            // Time.deltaTime allows rotation speed to be independent of framerate
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            // We separate Y Axis rotation from the X Axis so we can "clamp" it
            // I.e prevent the player from turning their head upside down
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);

            VoxelManagement();
            PlaceCursorBlocks();

            if (mapPlayer.IsZoomed)
            {
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
            }
        }
    }

    private void VoxelManagement()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedBlockIndex++;
                WrapIndex();
                while (world.blocktypes[selectedBlockIndex].blockName == "%%DELETED_BLOCK%%")
                {
                    selectedBlockIndex++;
                    WrapIndex();
                }
            }
            else
            {
                selectedBlockIndex--;
                WrapIndex();
                while (world.blocktypes[selectedBlockIndex].blockName == "%%DELETED_BLOCK%%")
                {
                    selectedBlockIndex--;
                    WrapIndex();
                }
            }

            selectedBlockText.text = world.blocktypes[selectedBlockIndex].blockName + " Block";
        }

        if (highlightBlock.gameObject.activeSelf)
        {
            // Destroy block.
            if (Input.GetMouseButtonDown(0) && canDestroy)
            {
                Vector3 blockPosition = highlightBlock.position;
                world.GetChunkFromVector3(blockPosition).EditVoxel(blockPosition, 0);
                worldSave.RemoveBlock(blockPosition.x, blockPosition.y, blockPosition.z);
            }

            // Place block.
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 blockPosition = placeBlock.position;
                world.GetChunkFromVector3(blockPosition).EditVoxel(blockPosition, selectedBlockIndex);
                worldSave.AddBlock(blockPosition.x, blockPosition.y, blockPosition.z, selectedBlockIndex);
            }
        }
    }

    private void WrapIndex()
    {
        if (selectedBlockIndex > (byte)(world.blocktypes.Count - 1))
            selectedBlockIndex = 1;
        if (selectedBlockIndex < 1)
            selectedBlockIndex = (byte)(world.blocktypes.Count - 1);
    }

    private void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        Vector3 direction = mapCamera.forward;

        while (step < reach)
        {
            Vector3 pos = mapCamera.position + (mapCamera.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                if (!mapPlayer.IsZoomed)
                {
                    highlightBlock.gameObject.SetActive(true);
                    placeBlock.gameObject.SetActive(true);
                }

                canDestroy = true;

                return;
            }

            // Check for collision with the platform
            else if (Physics.Raycast(mapCamera.position, direction, out RaycastHit hit, reach))
            {
                if (hit.collider.CompareTag("Platform"))
                {
                    Vector3 hitPos = hit.point;
                    Vector3 platformPos = new Vector3(Mathf.FloorToInt(hitPos.x), Mathf.FloorToInt(hitPos.y), Mathf.FloorToInt(hitPos.z));

                    // Position the highlight block one unit lower
                    highlightBlock.position = platformPos - Vector3.up;
                    // Position the placement block one unit above the highlight block
                    placeBlock.position = highlightBlock.position + Vector3.up;

                    if (!mapPlayer.IsZoomed)
                    {
                        highlightBlock.gameObject.SetActive(true);
                        placeBlock.gameObject.SetActive(true);
                    }

                    canDestroy = false;

                    return;
                }
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }
}
