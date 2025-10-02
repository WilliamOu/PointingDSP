using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MapPlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 3f;

    [SerializeField] private TMP_Text selectedBlockText;
    [SerializeField] private WorldSave worldSave;
    [SerializeField] private World world;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform mapCamera;
    [SerializeField] private MapPlayer mapPlayer;

    [SerializeField] private Transform highlightBlock;
    [SerializeField] private Transform placeBlock;

    private byte selectedBlockIndex = 1;
    private float checkIncrement = 0.1f;
    private float reach = 8f;
    private float xRotation = 0f;
    private bool canDestroy = false;

    [SerializeField, Range(0f, 0.5f)] private float gizmoBorder = 0.1f;
    [SerializeField] private bool gizmoPivotAtMinCorner = true;

    private enum DragMode { None, Place, Destroy }
    private DragMode dragMode = DragMode.None;

    private Vector3Int dragStartCell;
    private Vector3Int dragEndCell;

    private int WorldW => VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth;
    private int WorldH => VoxelData.ChunkHeight;

    private Vector3Int ClampToWorld(Vector3Int c) =>
        new Vector3Int(
            Mathf.Clamp(c.x, 0, WorldW - 1),
            Mathf.Clamp(c.y, 0, WorldH - 1),
            Mathf.Clamp(c.z, 0, WorldW - 1)
        );

    private void SetRegionGizmo(Transform gizmo, Vector3Int a, Vector3Int b, bool clampVisual)
    {
        if (clampVisual) { a = ClampToWorld(a); b = ClampToWorld(b); }

        var min = Vector3Int.Min(a, b);
        var max = Vector3Int.Max(a, b);
        var size = (max - min) + Vector3Int.one; // voxel-inclusive

        // Inflate the gizmo slightly to avoid coplanar z-fighting.
        // This keeps a constant "border" thickness regardless of selection size.
        Vector3 inflatedScale = (Vector3)size + new Vector3(gizmoBorder, gizmoBorder, gizmoBorder);

        if (gizmoPivotAtMinCorner)
        {
            // Expand symmetrically: move by -border/2 so the inflate is centered about the original box
            Vector3 offset = new Vector3(gizmoBorder * 0.5f, gizmoBorder * 0.5f, gizmoBorder * 0.5f);
            gizmo.position = (Vector3)min - offset;
            gizmo.localScale = inflatedScale;
        }
        else
        {
            // Pivot centered: compute the center in voxel space and inflate around it
            Vector3 center = (Vector3)min + (Vector3)size * 0.5f;
            // If your unit cube mesh is centered at (0,0,0) with size 1, subtract (0.5,0.5,0.5) to align to voxel grid
            gizmo.position = center - new Vector3(0.5f, 0.5f, 0.5f);
            gizmo.localScale = inflatedScale;
        }

        gizmo.gameObject.SetActive(true);
    }


    private void HideGizmos()
    {
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    void Update()
    {
        if (mapPlayer.CurrentMode != MapPlayer.Mode.MapEdit)
        {
            HideGizmos();
            dragMode = DragMode.None;
            return;
        }

        // Look controls
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);

        VoxelHotbarScroll();      // just the scroll wheel part
        HandleDragSelection();    // new unified place/delete selection

        if (mapPlayer.IsZoomed) HideGizmos();
    }

    private void VoxelHotbarScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        if (scroll > 0) { selectedBlockIndex++; WrapIndex(); while (world.blocktypes[selectedBlockIndex].blockName == "%%DELETED_BLOCK%%") { selectedBlockIndex++; WrapIndex(); } }
        else { selectedBlockIndex--; WrapIndex(); while (world.blocktypes[selectedBlockIndex].blockName == "%%DELETED_BLOCK%%") { selectedBlockIndex--; WrapIndex(); } }

        selectedBlockText.text = world.blocktypes[selectedBlockIndex].blockName + " Block";
    }

    private void HandleDragSelection()
    {
        Vector3 origin = mapCamera.position;
        Vector3 dir = mapCamera.forward;

        // --- Begin drag ---
        if (dragMode == DragMode.None)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                bool clickedLeft = Input.GetMouseButtonDown(0);
                bool clickedRight = !clickedLeft;

                // Use DDA to pick initial cell
                if (VoxelRaycastDDA(origin, dir, reach, out var hitCell, out var placeCell, out _))
                {
                    if (clickedLeft) { dragMode = DragMode.Destroy; dragStartCell = hitCell; }
                    if (clickedRight) { dragMode = DragMode.Place; dragStartCell = placeCell; }
                }
                // Platform fallback (kept from your original logic)
                else if (Physics.Raycast(origin, dir, out RaycastHit hit, reach) && hit.collider.CompareTag("Platform"))
                {
                    Vector3 platformPos = new Vector3(
                        Mathf.FloorToInt(hit.point.x),
                        Mathf.FloorToInt(hit.point.y),
                        Mathf.FloorToInt(hit.point.z)
                    );

                    if (clickedLeft) { dragMode = DragMode.Destroy; dragStartCell = Vector3Int.FloorToInt(platformPos - Vector3.up); }
                    if (clickedRight) { dragMode = DragMode.Place; dragStartCell = Vector3Int.FloorToInt(platformPos); } // place was highlight+up => platform
                }
                else
                {
                    return; // no valid start
                }

                dragStartCell = ClampToWorld(dragStartCell);
                dragEndCell = dragStartCell;

                // Show just the relevant gizmo; no auto-highlighting otherwise.
                if (dragMode == DragMode.Destroy)
                {
                    placeBlock.gameObject.SetActive(false);
                    SetRegionGizmo(highlightBlock, dragStartCell, dragStartCell, /*clampVisual*/ false);
                }
                else // Place
                {
                    highlightBlock.gameObject.SetActive(false);
                    SetRegionGizmo(placeBlock, dragStartCell, dragStartCell, /*clampVisual*/ true); // placement visual is truncated to bounds
                }
            }
            return;
        }

        // Compute tip with the new rules
        bool hitSurface = TryGetDragTipCell(origin, dir, reach, dragMode, out var tipCell);
        dragEndCell = tipCell;

        if (dragMode == DragMode.Destroy)
        {
            // Deletion visual: can extend past bounds
            SetRegionGizmo(highlightBlock, dragStartCell, dragEndCell, /*clampVisual*/ false);

            if (Input.GetMouseButtonUp(0))
            {
                var min = Vector3Int.Min(ClampToWorld(dragStartCell), ClampToWorld(dragEndCell));
                var max = Vector3Int.Max(ClampToWorld(dragStartCell), ClampToWorld(dragEndCell));

                world.FastBatchEdit((Vector3)min, (Vector3)max, 0, /*spawn=*/false);
                HideGizmos();
                dragMode = DragMode.None;
            }
        }
        else // Place
        {
            // Placement visual: always truncate to world bounds (even if floating)
            var clampedTip = ClampToWorld(dragEndCell);
            SetRegionGizmo(placeBlock, dragStartCell, clampedTip, /*clampVisual*/ true);

            if (Input.GetMouseButtonUp(1))
            {
                var min = Vector3Int.Min(ClampToWorld(dragStartCell), clampedTip);
                var max = Vector3Int.Max(ClampToWorld(dragStartCell), clampedTip);

                world.FastBatchEdit((Vector3)min, (Vector3)max, (byte)selectedBlockIndex, /*spawn=*/true);
                HideGizmos();
                dragMode = DragMode.None;
            }
        }

        // Optional cancel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideGizmos();
            dragMode = DragMode.None;
        }
    }

    private bool TryGetDragTipCell(Vector3 origin, Vector3 dir, float reach, DragMode mode, out Vector3Int tipCell)
    {
        // 1) Voxels via DDA
        if (VoxelRaycastDDA(origin, dir, reach, out var hitCell, out var placeCell, out _))
        {
            tipCell = (mode == DragMode.Destroy) ? hitCell : placeCell;
            return true; // valid surface
        }

        // 2) Platform fallback
        if (Physics.Raycast(origin, dir, out RaycastHit hit, reach) && hit.collider.CompareTag("Platform"))
        {
            var platformPos = new Vector3Int(
                Mathf.FloorToInt(hit.point.x),
                Mathf.FloorToInt(hit.point.y),
                Mathf.FloorToInt(hit.point.z)
            );

            tipCell = (mode == DragMode.Destroy)
                ? platformPos + Vector3Int.down   // delete highlight one below platform block
                : platformPos;                    // place directly on the platform block
            return true; // valid surface
        }

        // 3) Nothing hit: "floating" at max reach
        tipCell = Vector3Int.FloorToInt(origin + dir * reach);
        return false; // not a surface (floating)
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
        Vector3 origin = mapCamera.position;
        Vector3 dir = mapCamera.forward;

        // 1) Precise voxel traversal
        if (VoxelRaycastDDA(origin, dir, reach, out var hitCell, out var placeCell, out var _))
        {
            highlightBlock.position = new Vector3(hitCell.x, hitCell.y, hitCell.z);
            placeBlock.position = new Vector3(placeCell.x, placeCell.y, placeCell.z);

            if (!mapPlayer.IsZoomed)
            {
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
            }

            canDestroy = true;
            return;
        }

        // 2) Fallback: platform hit (unchanged behavior)
        if (Physics.Raycast(origin, dir, out RaycastHit hit, reach) && hit.collider.CompareTag("Platform"))
        {
            Vector3 hitPos = hit.point;
            Vector3 platformPos = new Vector3(
                Mathf.FloorToInt(hitPos.x),
                Mathf.FloorToInt(hitPos.y),
                Mathf.FloorToInt(hitPos.z)
            );

            // Highlight one unit lower; place one unit above highlight (as before)
            highlightBlock.position = platformPos - Vector3.up;
            placeBlock.position = highlightBlock.position + Vector3.up;

            if (!mapPlayer.IsZoomed)
            {
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
            }

            canDestroy = false;
            return;
        }

        // 3) Nothing hit
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    // Walk the voxel grid with Amanatides & Woo DDA.
    // Returns true if we hit a solid voxel within reach.
    // hitCell  = voxel to destroy (red)
    // placeCell = adjacent empty voxel where we'd place (green)
    // hitNormal = face normal of the hit (optional but handy)
    private bool VoxelRaycastDDA(
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        out Vector3Int hitCell,
        out Vector3Int placeCell,
        out Vector3 hitNormal)
    {
        hitCell = default;
        placeCell = default;
        hitNormal = Vector3.zero;

        if (direction.sqrMagnitude < 1e-8f) return false;
        direction.Normalize();

        // World bounds (0-indexed, non-negative; adjust if your world supports negatives)
        int worldW = VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth;
        int worldH = VoxelData.ChunkHeight;

        // Start cell
        Vector3Int cell = new Vector3Int(
            Mathf.FloorToInt(origin.x),
            Mathf.FloorToInt(origin.y),
            Mathf.FloorToInt(origin.z)
        );

        // Early-out if starting outside vertical range
        if (cell.y < 0 || cell.y >= worldH) return false;

        // DDA setup
        Vector3 sign = new Vector3(
            direction.x >= 0 ? 1f : -1f,
            direction.y >= 0 ? 1f : -1f,
            direction.z >= 0 ? 1f : -1f
        );

        // Distance (in world units along ray) to the next grid plane on each axis
        float Epsilon = 1e-8f;
        float invDx = Mathf.Abs(direction.x) > Epsilon ? 1f / Mathf.Abs(direction.x) : float.PositiveInfinity;
        float invDy = Mathf.Abs(direction.y) > Epsilon ? 1f / Mathf.Abs(direction.y) : float.PositiveInfinity;
        float invDz = Mathf.Abs(direction.z) > Epsilon ? 1f / Mathf.Abs(direction.z) : float.PositiveInfinity;

        // Compute first boundary distances (tMax) on each axis
        float nextX = cell.x + (sign.x > 0 ? 1f : 0f);
        float nextY = cell.y + (sign.y > 0 ? 1f : 0f);
        float nextZ = cell.z + (sign.z > 0 ? 1f : 0f);

        float tMaxX = (Mathf.Abs(direction.x) > Epsilon) ? (nextX - origin.x) / direction.x : float.PositiveInfinity;
        float tMaxY = (Mathf.Abs(direction.y) > Epsilon) ? (nextY - origin.y) / direction.y : float.PositiveInfinity;
        float tMaxZ = (Mathf.Abs(direction.z) > Epsilon) ? (nextZ - origin.z) / direction.z : float.PositiveInfinity;

        // How far to move to cross one voxel on that axis
        float tDeltaX = invDx;
        float tDeltaY = invDy;
        float tDeltaZ = invDz;

        // Track the previous empty cell to use as "place" position
        Vector3Int prevCell = cell;
        float traveled = 0f;
        int lastAxis = -1; // 0=x,1=y,2=z

        // Helper to check occupancy: sample at voxel center
        bool IsSolid(Vector3Int c)
        {
            if (c.x < 0 || c.x >= worldW || c.z < 0 || c.z >= worldW || c.y < 0 || c.y >= worldH) return false;
            Vector3 p = new Vector3(c.x + 0.5f, c.y + 0.5f, c.z + 0.5f);
            return world.CheckForVoxel(p);
        }

        // If the starting cell is solid (e.g., camera inside a block), advance once so we don't place inside ourselves
        if (IsSolid(cell))
        {
            // step once along the smallest tMax to exit the solid
            if (tMaxX < tMaxY && tMaxX < tMaxZ) { prevCell = cell; cell.x += (int)sign.x; traveled = tMaxX; tMaxX += tDeltaX; lastAxis = 0; }
            else if (tMaxY < tMaxZ) { prevCell = cell; cell.y += (int)sign.y; traveled = tMaxY; tMaxY += tDeltaY; lastAxis = 1; }
            else { prevCell = cell; cell.z += (int)sign.z; traveled = tMaxZ; tMaxZ += tDeltaZ; lastAxis = 2; }
        }

        while (traveled <= maxDistance)
        {
            // Solid? We hit.
            if (IsSolid(cell))
            {
                hitCell = cell;
                placeCell = prevCell;

                // Face normal is opposite of the last step direction
                switch (lastAxis)
                {
                    case 0: hitNormal = (sign.x > 0) ? Vector3.left : Vector3.right; break;
                    case 1: hitNormal = (sign.y > 0) ? Vector3.down : Vector3.up; break;
                    case 2: hitNormal = (sign.z > 0) ? Vector3.back : Vector3.forward; break;
                    default: hitNormal = Vector3.zero; break;
                }
                return true;
            }

            // Step to the next voxel boundary
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                prevCell = cell;
                cell.x += (int)sign.x;
                traveled = tMaxX;
                tMaxX += tDeltaX;
                lastAxis = 0;
            }
            else if (tMaxY < tMaxZ)
            {
                prevCell = cell;
                cell.y += (int)sign.y;
                traveled = tMaxY;
                tMaxY += tDeltaY;
                lastAxis = 1;
            }
            else
            {
                prevCell = cell;
                cell.z += (int)sign.z;
                traveled = tMaxZ;
                tMaxZ += tDeltaZ;
                lastAxis = 2;
            }

            // Out of world horizontally or vertically? stop.
            if (cell.x < 0 || cell.x >= worldW || cell.z < 0 || cell.z >= worldW || cell.y < 0 || cell.y >= worldH)
                return false;
        }

        return false;
    }
}
