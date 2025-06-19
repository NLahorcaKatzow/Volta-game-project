using UnityEngine;

public class Boundaries : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private Vector2 playerSize = new Vector2(0.8f, 0.8f);
    [SerializeField] private LayerMask obstacleLayer = -1;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    void Start()
    {
        Debug.Log("Robust Boundaries system initialized");
    }
    
    // Check if movement is allowed in a specific direction
    public bool CanMoveInDirection(Vector3 direction, Vector3 currentGridPosition)
    {
        // Calculate target position
        Vector3 targetGridPosition = currentGridPosition + direction;
        Vector3 targetWorldPosition = targetGridPosition * gridSize;
        
        // Check for obstacles at target position using overlap detection
        bool hasObstacle = Physics2D.OverlapBox(
            targetWorldPosition, 
            playerSize, 
            0f, 
            obstacleLayer
        );
        
        if (hasObstacle)
        {
            Debug.Log($"Movement blocked in direction {direction} - obstacle at target position {targetGridPosition}");
            return false;
        }
        
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = direction;
        float rayDistance = gridSize;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, obstacleLayer);
        
        if (hit.collider != null)
        {
            Debug.Log($"Movement blocked in direction {direction} - raycast hit obstacle {hit.collider.name}");
            return false;
        }
        
        Debug.Log($"Movement allowed in direction {direction}");
        return true;
    }
    
    // Alternative method for checking specific directions (for backward compatibility)
    public bool CanMoveUp(Vector3 currentGridPosition) => CanMoveInDirection(Vector3.up, currentGridPosition);
    public bool CanMoveDown(Vector3 currentGridPosition) => CanMoveInDirection(Vector3.down, currentGridPosition);
    public bool CanMoveLeft(Vector3 currentGridPosition) => CanMoveInDirection(Vector3.left, currentGridPosition);
    public bool CanMoveRight(Vector3 currentGridPosition) => CanMoveInDirection(Vector3.right, currentGridPosition);
    
    // Update grid size from player controller
    public void SetGridSize(float newGridSize)
    {
        gridSize = newGridSize;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw current position
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, playerSize);
        
        // Draw potential movement positions
        Gizmos.color = Color.yellow;
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        
        foreach (Vector3 direction in directions)
        {
            Vector3 checkPosition = transform.position + (direction * gridSize);
            Gizmos.DrawWireCube(checkPosition, playerSize * 0.8f);
        }
    }
} 