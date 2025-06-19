using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private Vector3 trailOffset = new Vector3(0, 0, 0.1f);
    
    private List<Vector3> trailPositions = new List<Vector3>();
    
    void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        SetupLineRenderer();
        InitializeTrail();
    }
    
    void Update()
    {
        if (playerController == null || !showTrail) return;
        UpdateTrail();
    }
    
    private void SetupLineRenderer()
    {
        if (trailLine == null)
        {
            trailLine = gameObject.AddComponent<LineRenderer>();
        }
        
        trailLine.material.color = trailColor;
        trailLine.startWidth = trailWidth;
        trailLine.endWidth = trailWidth;
        trailLine.useWorldSpace = true;
        trailLine.sortingOrder = 1;
        
        if (trailMaterial != null)
            trailLine.material = trailMaterial;
    }
    
    private void InitializeTrail()
    {
        if (playerController == null) return;
        
        Vector3 startPosition = playerController.GetCurrentGridPosition() * GetGridSize() + trailOffset;
        trailPositions.Clear();
        trailPositions.Add(startPosition);
        
        UpdateLineRenderer();
    }
    
    private void UpdateTrail()
    {
        if (playerController == null) return;
        
        List<Vector3> currentHistory = playerController.GetMovementHistory();
        
        List<Vector3> worldPositions = new List<Vector3>();
        foreach (Vector3 gridPos in currentHistory)
        {
            Vector3 worldPos = gridPos * GetGridSize() + trailOffset;
            worldPositions.Add(worldPos);
        }
        
        if (!ArePositionsEqual(trailPositions, worldPositions))
        {
            trailPositions = new List<Vector3>(worldPositions);
            UpdateLineRenderer();
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
        return 1f;
    }
    
    // Only method called externally
    public void SetTrailVisibility(bool visible)
    {
        showTrail = visible;
        if (trailLine != null)
            trailLine.enabled = visible;
    }
} 