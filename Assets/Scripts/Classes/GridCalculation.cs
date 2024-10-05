// Description: This script contains functions used to calculate the player's position on the grid
// Used when logging data in Learning, Retracing, and Wayfinding stages
using UnityEngine;

public static class GridCalculation
{
    public static Vector2 CalculateGridPosition(Vector3 position, float scale)
    {
        // Adjust position based on scale and grid size (5 units per grid at scale 1)
        float adjustedX = (position.x + 25 * scale) / (5 * scale);
        float adjustedZ = (position.z + 25 * scale) / (5 * scale);

        // Calculate grid indices (1-based)
        int gridX = Mathf.Clamp(Mathf.FloorToInt(adjustedX) + 1, 1, 11);
        int gridZ = Mathf.Clamp(Mathf.FloorToInt(adjustedZ) + 1, 1, 11);

        return new Vector2(gridX, gridZ);
    }

    public static string GridPositionToLabel(Vector2 gridPosition)
    {
        // Convert grid Y to letter (A-K)
        char rowLabel = (char)('A' + 11 - (int)gridPosition.y);

        // Combine row label and column number
        return $"{rowLabel}{(int)gridPosition.x}";
    }
}