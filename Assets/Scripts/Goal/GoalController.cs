using DG.Tweening;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private Direction requiredDirection = Direction.Up;
    [SerializeField] private bool debugMode = true;
    
    /*[Header("Visual Feedback")]
    [SerializeField] private Color correctAlignmentColor = Color.green;
    [SerializeField] private Color incorrectAlignmentColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private SpriteRenderer goalRenderer;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip goalCompleteSound;
    [SerializeField] private AudioClip alignmentSound;*/
    
    private bool goalCompleted = false;
    private PlayerController playerController;
    //private Color originalColor;
    
    void Start()
    {
        /*// Setup goal renderer
        if (goalRenderer == null)
            goalRenderer = GetComponent<SpriteRenderer>();
        
        if (goalRenderer != null)
            originalColor = goalRenderer.color;
        
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();*/
        
        Debug.Log($"Goal initialized - Required direction: {requiredDirection}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (goalCompleted) return;
        
        if (other.CompareTag("Player"))
        {
            playerController = other.GetComponent<PlayerController>();
            
            if (playerController != null)
            {
                CheckGoalAlignment();
            }
            else
            {
                Debug.LogError("Player object does not have PlayerController component!");
            }
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (goalCompleted) return;
        
        if (other.CompareTag("Player") && playerController != null)
        {
            CheckGoalAlignment();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Reset tag to default when player leaves
            gameObject.tag = "Untagged";
            //ResetVisualFeedback();
            playerController = null;
            
            if (debugMode)
                Debug.Log("Player left goal area - tag reset to Untagged");
        }
    }
    
    private void CheckGoalAlignment()
    {
        if (playerController == null) return;
        
        Direction playerCurrentDirection = playerController.GetCurrentFacingDirection();
        Direction playerPreviousDirection = GetPlayerPreviousDirection();
        Direction requiredGoalDirection = requiredDirection;
        
        bool isAligned = IsDirectionAligned(playerCurrentDirection, playerPreviousDirection, requiredGoalDirection);
        
        if (debugMode)
        {
            Debug.Log($"Player current: {playerCurrentDirection}, Player previous: {playerPreviousDirection}, Goal requires: {requiredGoalDirection}, Aligned: {isAligned}");
        }
        
        // Update tag based on alignment
        if (isAligned)
        {
            gameObject.tag = "Goal";
            gameObject.layer = LayerMask.NameToLayer("Default");
            //SetVisualFeedback(correctAlignmentColor);
            GoalFinish();
        }
        else
        {
            gameObject.tag = "Obstacle";
            gameObject.layer = LayerMask.NameToLayer("Wall");
            //SetVisualFeedback(incorrectAlignmentColor);
            //PlayAlignmentFeedback();
        }
    }
    
    private bool IsDirectionAligned(Direction playerCurrentDirection, Direction playerPreviousDirection, Direction goalRequiredDirection)
    {
        // Get the direction player should be moving to enter the goal (opposite to goal's pointing direction)
        Direction requiredPlayerDirection = GetOppositeDirection(goalRequiredDirection);
        
        // Check if player is moving consistently (current and previous directions are the same)
        bool isMovingConsistently = playerCurrentDirection == playerPreviousDirection;
        
        // Check if player is moving in the correct direction (opposite to where goal points)
        bool isMovingCorrectDirection = playerCurrentDirection == requiredPlayerDirection;
        
        bool isAligned = isMovingConsistently && isMovingCorrectDirection;
        
        if (debugMode)
        {
            Debug.Log($"Goal points: {goalRequiredDirection}, Player should move: {requiredPlayerDirection}");
            Debug.Log($"Player current: {playerCurrentDirection}, Player previous: {playerPreviousDirection}");
            Debug.Log($"Moving consistently: {isMovingConsistently}, Moving correct direction: {isMovingCorrectDirection}, Aligned: {isAligned}");
        }
        
        return isAligned;
    }
    
    private Direction GetPlayerPreviousDirection()
    {
        if (playerController == null) return Direction.Up;
        
        // Get the movement history to determine previous direction
        int historyCount = playerController.GetHistoryCount();
        
        if (historyCount < 3)
        {
            if (debugMode)
                Debug.Log("Insufficient movement history to determine previous direction");
            return playerController.GetCurrentFacingDirection(); // Default to current direction
        }
        
        // Get the direction from the third-to-last to the second-to-last position
        Vector3 previousMovementVector = playerController.GetOriginalMovementDirection(historyCount - 2);
        Direction previousDirection = previousMovementVector.ToDirection();
        
        if (debugMode)
            Debug.Log($"Previous movement direction calculated: {previousDirection}");
        
        return previousDirection;
    }
    
    private Direction GetOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.Up;
        }
    }
    
    private void GoalFinish()
    {
        
        goalCompleted = true;
        
        Debug.Log("GOAL COMPLETED! Player successfully aligned and entered the goal!");
        
        /*// Play completion sound
        if (audioSource != null && goalCompleteSound != null)
        {
            audioSource.PlayOneShot(goalCompleteSound);
        }
        
        // Visual feedback
        SetVisualFeedback(correctAlignmentColor);*/
        
        // Here you can add additional goal completion logic:
        // - Load next level
        // - Show victory UI
        // - Unlock achievements
        // - etc.
        
        OnGoalCompleted();
    }
    
    /*private void PlayAlignmentFeedback()
    {
        if (audioSource != null && alignmentSound != null)
        {
            // Play alignment sound with reduced volume to avoid spam
            if (!audioSource.isPlaying)
            {
                audioSource.volume = 0.3f;
                audioSource.PlayOneShot(alignmentSound);
            }
        }
    }
    
    private void SetVisualFeedback(Color color)
    {
        if (goalRenderer != null)
        {
            goalRenderer.color = color;
        }
    }
    
    private void ResetVisualFeedback()
    {
        if (goalRenderer != null)
        {
            goalRenderer.color = originalColor;
        }
    }*/
    
    // Public method that can be overridden or extended
    protected virtual void OnGoalCompleted()
    {
        // Override this method in derived classes for custom goal completion behavior
        playerController.enabled = false;
        playerController.GetWireController().SetTrailVisibility(false);
        playerController.transform.DOMove(transform.position, 1f).SetEase(Ease.OutQuint).OnComplete(() => 
        {
            //TODO: sonido de switch
            
            // Trigger scene transition through SceneManager singleton
            SceneManager.Instance.OnLevelCompleted();
        });
    }
    
    // Public methods for external access
    public bool IsGoalCompleted()
    {
        return goalCompleted;
    }
    
    public Direction GetRequiredDirection()
    {
        return requiredDirection;
    }
    
    public void SetRequiredDirection(Direction newDirection)
    {
        requiredDirection = newDirection;
        Debug.Log($"Goal direction changed to: {requiredDirection}");
    }
    
    public void ResetGoal()
    {
        goalCompleted = false;
        //ResetVisualFeedback();
        gameObject.SetActive(true);
        Debug.Log("Goal reset");
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Draw goal area
            Gizmos.color = goalCompleted ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            
            // Draw required direction arrow
            Vector3 directionVector = requiredDirection.ToVector3();
            Vector3 arrowStart = transform.position;
            Vector3 arrowEnd = arrowStart + (directionVector * 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            Gizmos.DrawSphere(arrowEnd, 0.1f);
        }
    }
}
