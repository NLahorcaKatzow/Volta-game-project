using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float gridSize = 1f;
    
    [Header("Components")]
    [SerializeField] private Boundaries boundaries;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Movement History")]
    [SerializeField] private bool enableHistory = true;
    public int maxHistorySize = 50;
    public event Action<int> OnChangeMovementHistory;
    [SerializeField] private float holdTimeThreshold = 0.5f; // Time to hold before fast rewind
    [SerializeField] private float fastRewindSpeed = 0.15f; // Speed for fast rewind
    [SerializeField] private WireController wireController;
    
    [Header("Trace Settings")]
    [SerializeField] private GameObject tracePrefab;
    [SerializeField] private Transform traceParent;
    [SerializeField] private bool createDefaultTrace = true;
    
    private Vector3 currentGridPosition;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool canMove = true;
    
    // Movement history system - now using dictionary with trace GameObjects
    private List<Vector3> movementHistory = new List<Vector3>(); // Keep for order
    private Dictionary<Vector3, GameObject> traceHistory = new Dictionary<Vector3, GameObject>(); // Position -> Trace GameObject
    [SerializeField]private bool isRewinding = false;
    
    // Fast rewind system
    private bool isHoldingRightClick = false;
    private float rightClickHoldTime = 0f;
    private bool isFastRewinding = false;
    
    // Current facing direction
    private Direction currentFacingDirection = Direction.Up;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize grid position based on current world position
        currentGridPosition = new Vector3(
            Mathf.Round(transform.position.x / gridSize),
            Mathf.Round(transform.position.y / gridSize),
            0
        );
        
        // Snap to grid
        transform.position = currentGridPosition * gridSize;
        
        // Get boundaries component if not assigned
        if (boundaries == null)
            boundaries = GetComponent<Boundaries>();
            
        // Get sprite renderer if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Sync grid size with boundaries
        if (boundaries != null)
            boundaries.SetGridSize(gridSize);
        
        // Initialize movement history with starting position
        if (enableHistory)
        {
            movementHistory.Clear();
            traceHistory.Clear();
            movementHistory.Add(currentGridPosition);
            // Don't create trace for starting position
            Debug.Log("Movement history initialized with starting position: " + currentGridPosition);
        }
        
        Debug.Log("PlayerController initialized at grid position: " + currentGridPosition);
    }

    // Update is called once per frame
    void Update()
    {
        // Only process input if not currently moving
        if (!isMoving)
        {
            HandleInput();
        }
    }
    
    private void HandleInput()
    {
        
        // Handle right-click rewind system
        HandleRewindInput();
        
        // Only process movement input if not rewinding
        if (isRewinding || isFastRewinding)
            return;
            
        if (!canMove) return;
            
        Vector3 moveDirection = Vector3.zero;
        
        // Check for WASD input
        if (Input.GetKeyDown(KeyCode.W))
        {
            moveDirection = Direction.Up.ToVector3();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moveDirection = Direction.Down.ToVector3();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moveDirection = Direction.Left.ToVector3();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moveDirection = Direction.Right.ToVector3();
        }
        
        // Attempt to move if a direction was pressed
        if (moveDirection != Vector3.zero)
        {
            TryMove(moveDirection);
        }
    }
    
    private void TryMove(Vector3 direction)
    {
        // Check if movement is allowed in the desired direction
        if (!CanMoveInDirection(direction))
        {
            Debug.Log("Movement blocked in direction: " + direction);
            return;
        }
        
        // Calculate new grid position
        Vector3 newGridPosition = currentGridPosition + direction;
        Vector3 newWorldPosition = newGridPosition * gridSize;
        
        // Set facing direction
        SetFacingDirection(direction);
        
        // Start movement
        isMoving = true;
        currentGridPosition = newGridPosition;
        
        // Add position to history if not rewinding
        if (enableHistory && !isRewinding)
        {
            AddPositionToHistory(newGridPosition);
        }
        
        //Debug.Log("Moving to grid position: " + newGridPosition);
        
        // Use DOTween to animate movement
        transform.DOMove(newWorldPosition, moveSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                isMoving = false;
                isRewinding = false; // Reset rewind flag
                //Debug.Log("Movement completed");
            });
    }
    
    private bool CanMoveInDirection(Vector3 direction)
    {
        if (boundaries == null)
        {
            Debug.LogError("Boundaries component not found - allowing movement");
            return true;
        }
        
        // Calculate target position
        Vector3 targetPosition = currentGridPosition + direction;
        
        // Check if target position is in movement history (unless rewinding)
        if (enableHistory && !isRewinding && IsPositionInHistory(targetPosition))
        {
            //Debug.Log("Movement blocked - position already visited: " + targetPosition);
            return false;
        }
        
        // Use the robust collision detection system
        return boundaries.CanMoveInDirection(direction, currentGridPosition);
    }
    
    private void SetFacingDirection(Vector3 direction)
    {
        if (spriteRenderer == null) return;
        
        // Convert Vector3 to Direction enum and store
        currentFacingDirection = direction.ToDirection();
        
        // Reset flip in case it was set before
        spriteRenderer.flipX = false;
        
        // Rotate sprite based on movement direction
        if (currentFacingDirection == Direction.Up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("Player facing up");
        }
        else if (currentFacingDirection == Direction.Down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180);
            Debug.Log("Player facing down");
        }
        else if (currentFacingDirection == Direction.Left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90);
            Debug.Log("Player facing left");
        }
        else if (currentFacingDirection == Direction.Right)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90);
            Debug.Log("Player facing right");
        }
    }
    
    // Public getter for current grid position
    public Vector3 GetCurrentGridPosition()
    {
        return currentGridPosition;
    }
    
    // Public getter to check if player is currently moving
    public bool IsMoving()
    {
        return isMoving;
    }
    
    // Movement history methods
    private void HandleRewindInput()
    {
        if (!enableHistory) {
        StopFastRewind();
        return;
        }
        
        // Track right-click hold state
        if (Input.GetMouseButtonDown(1))
        {
            isHoldingRightClick = true;
            rightClickHoldTime = 0f;
            
            // Immediate single rewind on click
            TryRewind();
        }
        else if (Input.GetMouseButton(1) && isHoldingRightClick)
        {
            // Update hold time
            rightClickHoldTime += Time.deltaTime;
            
            // Start fast rewind after threshold
            if (rightClickHoldTime >= holdTimeThreshold && !isFastRewinding && !isRewinding)
            {
                StartFastRewind();
            }
        }
        else if (Input.GetMouseButtonUp(1))
        {
            // Stop fast rewind when button released
            StopFastRewind();
            isHoldingRightClick = false;
            rightClickHoldTime = 0f;
        }
    }
    
    private void TryRewind()
    {
        if (movementHistory.Count <= 1)
        {
            Debug.Log("Cannot rewind - no previous positions available");
            return;
        }
        
        // Get the position we're rewinding from (current position)
        Vector3 currentPosition = movementHistory[movementHistory.Count - 1];
        
        // Remove current position from history
        movementHistory.RemoveAt(movementHistory.Count - 1);
        
        // Destroy trace GameObject at current position
        DestroyTraceAtPosition(currentPosition);
        
        OnChangeMovementHistory?.Invoke(movementHistory.Count - 1);
        
        // Get previous position
        Vector3 previousPosition = movementHistory[movementHistory.Count - 1];
        Vector3 previousWorldPosition = previousPosition * gridSize;
        
        // Calculate the direction the player was facing when they reached the previous position
        Vector3 originalDirection = GetOriginalMovementDirection(movementHistory.Count - 1);
        SetFacingDirection(originalDirection);
        
        // Start rewind movement
        canMove = true;
        isMoving = true;
        isRewinding = true;
        currentGridPosition = previousPosition;
        
        Debug.Log("Rewinding to position: " + previousPosition);
        Debug.Log("History count after rewind: " + movementHistory.Count);
        
        // Use DOTween to animate rewind movement
        float rewindDuration = isFastRewinding ? fastRewindSpeed : moveSpeed * 0.7f;
        transform.DOMove(previousWorldPosition, rewindDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                isMoving = false;
                isRewinding = false;
                
                // Continue fast rewind only if still holding button and more history available
                if (isFastRewinding && isHoldingRightClick && movementHistory.Count > 1)
                {
                    TryRewind();
                }
                else
                {
                    // Stop fast rewind if button was released
                    if (isFastRewinding && !isHoldingRightClick)
                    {
                        isFastRewinding = false;
                        Debug.Log("Fast rewind stopped - button released");
                    }
                    isFastRewinding = false;
                    Debug.Log("Rewind completed");
                }
            });
    }
    
    private void StartFastRewind()
    {
        if (movementHistory.Count <= 1) return;
        
        isFastRewinding = true;
        Debug.Log("Started fast rewind mode");
        
        // If not currently rewinding, start the fast rewind chain
        if (!isRewinding)
        {
            TryRewind();
        }
    }
    
    private void StopFastRewind()
    {
        if (isFastRewinding)
        {
            isFastRewinding = false;
            
            // Kill any ongoing movement tween to stop immediately
            transform.DOKill();
            
            // Reset movement state properly
            isMoving = false;
            isRewinding = false;
            canMove = true; // Ensure player can move again
            
            // Snap to the current grid position to avoid floating point issues
            transform.position = currentGridPosition * gridSize;
            
            Debug.Log("Stopped fast rewind mode - player can move again");
        }
    }
    
    private bool IsPositionInHistory(Vector3 position)
    {
        return movementHistory.Contains(position);
    }
    
    private void AddPositionToHistory(Vector3 position)
    {
        movementHistory.Add(position);
        
        // Create trace GameObject for this position
        StartCoroutine(CreateTraceAtPosition(position));
        
        // Limit history size
        if (movementHistory.Count > maxHistorySize)
        {
            canMove = false; //no se puede mover
            Debug.LogError("History size limit reached");
        }
        
        Debug.Log("Added position to history: " + position + " (Total: " + movementHistory.Count + ")");
        OnChangeMovementHistory?.Invoke(movementHistory.Count - 1);
    }
    
    private IEnumerator CreateTraceAtPosition(Vector3 gridPosition)
    {
        if (tracePrefab == null)
        {
            Debug.LogError("No trace prefab assigned - cannot create trace");
            yield break;
        }
        
        // Convert grid position to world position
        Vector3 worldPosition = gridPosition * gridSize;
        
        yield return new WaitForSeconds(0.1f);
        // Instantiate trace GameObject
        GameObject trace = Instantiate(tracePrefab, worldPosition, Quaternion.identity);
        trace.SetActive(true);
        //trace.transform.parent = traceParent;
        // Add to trace dictionary
        traceHistory[gridPosition] = trace;
        
        Debug.Log($"Created trace at position: {gridPosition} (World: {worldPosition})");
        yield break;
    }
    
    private void DestroyTraceAtPosition(Vector3 gridPosition)
    {
        if (traceHistory.TryGetValue(gridPosition, out GameObject trace))
        {
            if (trace != null)
            {
                Destroy(trace);
                Debug.Log($"Destroyed trace at position: {gridPosition}");
            }
            
            // Remove from dictionary
            traceHistory.Remove(gridPosition);
        }
        else
        {
            Debug.LogWarning($"No trace found at position: {gridPosition}");
        }
    }
    
    // Public methods for debugging and external access
    public List<Vector3> GetMovementHistory()
    {
        return new List<Vector3>(movementHistory); // Return copy to prevent external modification
    }
    
    public Dictionary<Vector3, GameObject> GetTraceHistory()
    {
        return new Dictionary<Vector3, GameObject>(traceHistory); // Return copy to prevent external modification
    }
    
    public int GetHistoryCount()
    {
        return movementHistory.Count;
    }
    
    public int GetTraceCount()
    {
        return traceHistory.Count;
    }
    
    public void ClearAllTraces()
    {
        foreach (var kvp in traceHistory)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        traceHistory.Clear();
        Debug.Log("All traces cleared");
    }
    
    public void SetTracePrefab(GameObject newTracePrefab)
    {
        tracePrefab = newTracePrefab;
        Debug.Log("Trace prefab updated");
    }
    
    // Calculate the original movement direction when player reached a specific position in history
    public Vector3 GetOriginalMovementDirection(int historyIndex)
    {
        // Need at least 2 positions to calculate direction
        if (movementHistory.Count < 2 || historyIndex <= 0)
        {
            Debug.LogError("Cannot calculate original direction - insufficient history");
            return Direction.Up.ToVector3(); // Default direction
        }
        
        // Calculate direction from previous position to current position in history
        Vector3 fromPosition = movementHistory[historyIndex - 1];
        Vector3 toPosition = movementHistory[historyIndex];
        Vector3 direction = toPosition - fromPosition;
        
        // Normalize to get unit direction (should be one of UP, DOWN, LEFT, RIGHT)
        direction = direction.normalized;
        
        // Round to ensure we get exact grid directions
        direction.x = Mathf.Round(direction.x);
        direction.y = Mathf.Round(direction.y);
        direction.z = 0; // Ensure Z is 0 for 2D movement
        
        Debug.Log($"Original movement direction at index {historyIndex}: {direction} (from {fromPosition} to {toPosition})");
        return direction;
    }
    
    public WireController GetWireController()
    {
        return wireController;
    }
    
    public bool CanRewind()
    {
        return enableHistory && movementHistory.Count > 1 && !isMoving && !isFastRewinding;
    }
    
    public bool IsFastRewinding()
    {
        return isFastRewinding;
    }
    
    public float GetHoldTime()
    {
        return rightClickHoldTime;
    }
    
    public float GetGridSize()
    {
        return gridSize;
    }
    
    // Get current facing direction
    public Direction GetCurrentFacingDirection()
    {
        return currentFacingDirection;
    }
    
    // Get current facing direction as Vector3 for backward compatibility
    public Vector3 GetCurrentFacingDirectionVector()
    {
        return currentFacingDirection.ToVector3();
    }
}
