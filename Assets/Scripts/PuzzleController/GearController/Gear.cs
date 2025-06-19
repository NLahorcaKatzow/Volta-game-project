using UnityEngine;
using DG.Tweening;
using System;

public class Gear : MonoBehaviour
{
    [Header("Gear Configuration")]
    [SerializeField] private Direction originalDirection = Direction.Up;
    [SerializeField] private Direction finalDirection = Direction.Right;
    [SerializeField] private bool clockwiseRotation = true;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Ease rotationEase = Ease.InOutQuad;

    [Header("Activation Settings")]
    [SerializeField] private bool activateOnStart = false;
    [SerializeField] private bool debugMode = true;

    [Header("Visual Components")]
    [SerializeField] private Transform gearTransform;
    [SerializeField] private SpriteRenderer gearRenderer;

    // State variables
    private bool isRotating = false;
    private bool isAtFinalPosition = false;
    private Tween currentRotationTween;

    // Events
    public event Action OnRotationStarted;
    public event Action OnRotationCompleted;
    public event Action OnGearActivated;

    void Start()
    {
        // Get components if not assigned
        SetupComponents();

        // Set initial rotation
        SetInitialRotation();

        // Auto-activate if configured
        if (activateOnStart)
        {
            ActivateGear();
        }

        if (debugMode)
            Debug.Log($"Gear initialized - Original: {originalDirection}, Final: {finalDirection}, Clockwise: {clockwiseRotation}");
    }

    private void SetupComponents()
    {
        // Get gear transform if not assigned
        if (gearTransform == null)
            gearTransform = transform;

        // Get sprite renderer if not assigned  
        if (gearRenderer == null)
            gearRenderer = GetComponent<SpriteRenderer>();

        if (debugMode)
            Debug.Log("Gear components setup completed");
    }

    private void SetInitialRotation()
    {
        // Set initial rotation based on original direction
        float initialAngle = GetAngleFromDirection(originalDirection);
        gearTransform.rotation = Quaternion.Euler(0, 0, initialAngle);
        isAtFinalPosition = false;

        if (debugMode)
            Debug.Log($"Initial rotation set to {initialAngle} degrees for direction {originalDirection}");
    }

    /// <summary>
    /// Activates the gear rotation
    /// </summary>
    public void ActivateGear()
    {
        if (isRotating)
        {
            if (debugMode)
                Debug.Log("Gear is already rotating, force stop");
            StopRotation();
        }

        StartRotation();
        OnGearActivated?.Invoke();

        if (debugMode)
            Debug.Log("Gear activated");
    }

    /// <summary>
    /// Toggles the gear between original and final positions
    /// </summary>
    public void ToggleGear()
    {
        if (isRotating)
        {
            if (debugMode)
                Debug.Log("Gear is rotating, force stop");
            StopRotation();

        }

        StartRotation();
    }

    /// <summary>
    /// Resets the gear to its original position (instant)
    /// </summary>
    public void ResetGear()
    {
        // Stop any current rotation
        StopRotation();

        // Reset to original position
        SetInitialRotation();

        if (debugMode)
            Debug.Log("Gear reset to original position");
    }

    /// <summary>
    /// Smoothly returns the gear to its original position with DOTween animation
    /// </summary>
    public void ReturnToOriginal()
    {
        if (isRotating)
        {
            if (debugMode)
                Debug.Log("Gear is rotating, stopping current rotation to return to original");
            StopRotation();
        }

        // If already at original position, no need to rotate
        if (!isAtFinalPosition)
        {
            if (debugMode)
                Debug.Log("Gear is already at original position");
            return;
        }

        StartReturnRotation();

        if (debugMode)
            Debug.Log("Starting smooth return to original position");
    }

    private void StartRotation()
    {
        if (isRotating) return;

        isRotating = true;
        OnRotationStarted?.Invoke();

        // Calculate target direction and angulo
        Direction targetDirection = isAtFinalPosition ? originalDirection : finalDirection;
        float targetAngle = GetAngleFromDirection(targetDirection);
        float currentAngle = gearTransform.eulerAngles.z;

        // Calculate rotation angle considering clockwise/counterclockwise
        float rotationAngle = CalculateRotationAngle(currentAngle, targetAngle, false);

        if (debugMode)
            Debug.Log($"Starting rotation from {currentAngle}° to {targetAngle}° (rotation: {rotationAngle}°)");

        // Perform rotation with DOTween
        currentRotationTween = gearTransform.DORotate(
            new Vector3(0, 0, targetAngle),
            rotationSpeed,
            RotateMode.FastBeyond360
        )
        .SetEase(rotationEase)
        .OnComplete(() =>
        {
            OnRotationComplete(targetDirection);
        });
    }

    private void StartReturnRotation()
    {
        if (isRotating) return;

        isRotating = true;
        OnRotationStarted?.Invoke();

        // Always target the original direction
        Direction targetDirection = originalDirection;
        float targetAngle = GetAngleFromDirection(targetDirection);
        float currentAngle = gearTransform.eulerAngles.z;

        // Calculate rotation angle considering clockwise/counterclockwise (reversed direction)
        float rotationAngle = CalculateRotationAngle(currentAngle, targetAngle, true);

        if (debugMode)
            Debug.Log($"Starting return rotation from {currentAngle}° to {targetAngle}° (rotation: {rotationAngle}°) - REVERSE DIRECTION");

        // Perform rotation with DOTween
        currentRotationTween = gearTransform.DORotate(
            new Vector3(0, 0, targetAngle),
            rotationSpeed,
            RotateMode.FastBeyond360
        )
        .SetEase(rotationEase)
        .OnComplete(() =>
        {
            OnReturnRotationComplete(targetDirection);
        });
    }

    private float CalculateRotationAngle(float fromAngle, float toAngle, bool useReverseDirection = false)
    {
        // Normalize angles to 0-360 range
        fromAngle = NormalizeAngle(fromAngle);
        toAngle = NormalizeAngle(toAngle);

        float clockwiseAngle = toAngle - fromAngle;
        if (clockwiseAngle < 0) clockwiseAngle += 360f;

        float counterclockwiseAngle = fromAngle - toAngle;
        if (counterclockwiseAngle < 0) counterclockwiseAngle += 360f;

        // Determine effective rotation direction (reverse if needed)
        bool effectiveClockwise = useReverseDirection ? !clockwiseRotation : clockwiseRotation;

        // Choose rotation direction based on configuration
        if (effectiveClockwise)
        {
            return clockwiseAngle;
        }
        else
        {
            return -counterclockwiseAngle;
        }
    }

    private float NormalizeAngle(float angle)
    {
        while (angle < 0) angle += 360f;
        while (angle >= 360f) angle -= 360f;
        return angle;
    }

    private void OnRotationComplete(Direction targetDirection)
    {
        isRotating = false;
        isAtFinalPosition = (targetDirection == finalDirection);
        currentRotationTween = null;

        OnRotationCompleted?.Invoke();

        if (debugMode)
            Debug.Log($"Rotation completed - Now at {(isAtFinalPosition ? "final" : "original")} position ({targetDirection})");
    }

    private void OnReturnRotationComplete(Direction targetDirection)
    {
        isRotating = false;
        isAtFinalPosition = false; // Always at original position after return
        currentRotationTween = null;

        OnRotationCompleted?.Invoke();

        if (debugMode)
            Debug.Log($"Return rotation completed - Now at original position ({targetDirection})");
    }

    private void StopRotation()
    {
        if (currentRotationTween != null)
        {
            currentRotationTween.Kill();
            currentRotationTween = null;
        }
        isRotating = false;

        if (debugMode)
            Debug.Log("Rotation stopped");
    }

    private float GetAngleFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return 0f;
            case Direction.Right:
                return -90f;  // Clockwise from Up
            case Direction.Down:
                return 180f;  // Or -180f
            case Direction.Left:
                return 90f;   // Counter-clockwise from Up
            default:
                return 0f;
        }
    }

    // Public getters for external access
    public bool IsRotating => isRotating;
    public bool IsAtFinalPosition => isAtFinalPosition;
    public Direction OriginalDirection => originalDirection;
    public Direction FinalDirection => finalDirection;
    public bool IsClockwiseRotation => clockwiseRotation;
    public float RotationSpeed => rotationSpeed;

    // Public setters for runtime configuration
    public void SetDirections(Direction original, Direction final)
    {
        if (isRotating)
        {
            Debug.LogWarning("Cannot change directions while gear is rotating");
            return;
        }

        originalDirection = original;
        finalDirection = final;
        SetInitialRotation();

        if (debugMode)
            Debug.Log($"Directions updated - Original: {original}, Final: {final}");
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0.1f, speed); // Minimum speed limit

        if (debugMode)
            Debug.Log($"Rotation speed set to {rotationSpeed}");
    }

    public void SetClockwiseRotation(bool clockwise)
    {
        clockwiseRotation = clockwise;

        if (debugMode)
            Debug.Log($"Rotation direction set to {(clockwise ? "clockwise" : "counterclockwise")}");
    }

    void OnDestroy()
    {
        // Clean up tween
        StopRotation();
    }
}
