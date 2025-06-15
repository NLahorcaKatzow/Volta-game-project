using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WireController : MonoBehaviour
{
    [Header("Wire Trail Settings")]
    [SerializeField] private LineRenderer trailLine;
    [SerializeField] private Color trailColor = Color.cyan;
    [SerializeField] private float trailWidth = 0.1f;
    [SerializeField] private Material trailMaterial;
    
    [Header("Integration")]
    [SerializeField] private PlayerController playerController;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showTrail = true;
    [SerializeField] private Vector3 trailOffset = new Vector3(0, 0, 0.1f); // Slight Z offset to avoid z-fighting
    
    private List<Vector3> trailPositions = new List<Vector3>();
    private bool wasRewinding = false;
    private bool wasFastRewinding = false;
    
    void Start()
    {
        // Get PlayerController if not assigned
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        // Setup LineRenderer
        SetupLineRenderer();
        
        // Initialize trail with player's starting position
        InitializeTrail();
        
        Debug.Log("WireController initialized successfully");
    }
    
    void Update()
    {
        if (playerController == null || !showTrail) return;
        
        // Check if rewind state changed
        bool isCurrentlyRewinding = playerController.IsMoving() && (playerController.CanRewind() == false);
        bool isCurrentlyFastRewinding = playerController.IsFastRewinding();
        
        // Update trail based on player's movement history
        UpdateTrail();
        
        // Track rewind state changes
        wasRewinding = isCurrentlyRewinding;
        wasFastRewinding = isCurrentlyFastRewinding;
    }
    
    private void SetupLineRenderer()
    {
        if (trailLine == null)
        {
            // Create LineRenderer if not assigned
            trailLine = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        trailLine.material.color = trailColor;
        trailLine.startWidth = trailWidth;
        trailLine.endWidth = trailWidth;
        trailLine.useWorldSpace = true;
        trailLine.sortingOrder = 1; // Render above other sprites
        
        if (trailMaterial != null)
            trailLine.material = trailMaterial;
        
        Debug.Log("LineRenderer configured");
    }
    
    private void InitializeTrail()
    {
        if (playerController == null) return;
        
        // Get initial position from player controller
        Vector3 startPosition = playerController.GetCurrentGridPosition() * GetGridSize() + trailOffset;
        trailPositions.Clear();
        trailPositions.Add(startPosition);
        
        UpdateLineRenderer();
        Debug.Log("Trail initialized with starting position: " + startPosition);
    }
    
    private void UpdateTrail()
    {
        if (playerController == null) return;
        
        // Get current movement history from player controller
        List<Vector3> currentHistory = playerController.GetMovementHistory();
        
        // Convert grid positions to world positions with offset
        List<Vector3> worldPositions = new List<Vector3>();
        foreach (Vector3 gridPos in currentHistory)
        {
            Vector3 worldPos = gridPos * GetGridSize() + trailOffset;
            worldPositions.Add(worldPos);
        }
        
        // Update trail positions if history changed
        if (!ArePositionsEqual(trailPositions, worldPositions))
        {
            trailPositions = new List<Vector3>(worldPositions);
            UpdateLineRenderer();
            
            // Log trail update
            if (playerController.IsFastRewinding())
            {
                Debug.Log("Fast rewind: Trail updated with " + trailPositions.Count + " positions");
            }
            else if (trailPositions.Count < worldPositions.Count)
            {
                Debug.Log("Trail extended to " + trailPositions.Count + " positions");
            }
            else if (trailPositions.Count > worldPositions.Count)
            {
                Debug.Log("Trail rewound to " + trailPositions.Count + " positions");
            }
        }
    }
    
    private void UpdateLineRenderer()
    {
        if (trailLine == null || trailPositions.Count == 0) return;
        
        trailLine.positionCount = trailPositions.Count;
        trailLine.SetPositions(trailPositions.ToArray());
        trailLine.enabled = showTrail;
    }
    
    private bool ArePositionsEqual(List<Vector3> list1, List<Vector3> list2)
    {
        if (list1.Count != list2.Count) return false;
        
        for (int i = 0; i < list1.Count; i++)
        {
            if (Vector3.Distance(list1[i], list2[i]) > 0.01f)
                return false;
        }
        return true;
    }
    
    private float GetGridSize()
    {
        if (playerController != null)
            return playerController.GetGridSize();
        return 1f; // Default fallback
    }
    
    // Public methods for external control
    public void SetTrailVisibility(bool visible)
    {
        showTrail = visible;
        if (trailLine != null)
            trailLine.enabled = visible;
    }
    
    public void SetTrailColor(Color color)
    {
        trailColor = color;
        if (trailLine != null)
            trailLine.material.color = color;
    }
    
    public void SetTrailWidth(float width)
    {
        trailWidth = width;
        if (trailLine != null)
        {
            trailLine.startWidth = width;
            trailLine.endWidth = width;
        }
    }
    
    public void ClearTrail()
    {
        trailPositions.Clear();
        if (trailLine != null)
        {
            trailLine.positionCount = 0;
        }
        Debug.Log("Trail cleared");
    }
    
    public void ResetTrail()
    {
        InitializeTrail();
        Debug.Log("Trail reset");
    }
    
    // Debug information
    public int GetTrailLength()
    {
        return trailPositions.Count;
    }
    
    public Vector3[] GetTrailPositions()
    {
        return trailPositions.ToArray();
    }
} 