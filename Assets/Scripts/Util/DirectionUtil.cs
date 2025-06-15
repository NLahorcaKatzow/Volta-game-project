using UnityEngine;

/// <summary>
/// Direction enum for grid-based movement and orientation
/// </summary>
[System.Serializable]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Utility class to convert between Direction enum and Vector3
/// </summary>
public static class DirectionExtensions
{
    /// <summary>
    /// Convert Direction enum to Vector3
    /// </summary>
    /// <param name="direction">Direction to convert</param>
    /// <returns>Vector3 representation of the direction</returns>
    public static Vector3 ToVector3(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Down:
                return Vector3.down;
            case Direction.Left:
                return Vector3.left;
            case Direction.Right:
                return Vector3.right;
            default:
                return Vector3.up;
        }
    }
    
    /// <summary>
    /// Convert Vector3 to Direction enum
    /// </summary>
    /// <param name="vector">Vector3 to convert</param>
    /// <returns>Direction enum representation</returns>
    public static Direction ToDirection(this Vector3 vector)
    {
        // Normalize and round to handle floating point precision
        Vector3 normalized = vector.normalized;
        normalized.x = Mathf.Round(normalized.x);
        normalized.y = Mathf.Round(normalized.y);
        normalized.z = 0;
        
        if (normalized == Vector3.up)
            return Direction.Up;
        else if (normalized == Vector3.down)
            return Direction.Down;
        else if (normalized == Vector3.left)
            return Direction.Left;
        else if (normalized == Vector3.right)
            return Direction.Right;
        else
            return Direction.Up; // Default fallback
    }
    
    /// <summary>
    /// Get the string name of the direction
    /// </summary>
    /// <param name="direction">Direction to get name for</param>
    /// <returns>String name of the direction</returns>
    public static string GetDirectionName(this Direction direction)
    {
        return direction.ToString();
    }
} 