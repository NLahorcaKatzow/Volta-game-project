using UnityEngine;
using UnityEngine.Events;

namespace PuzzleController.LaserController
{
    /// <summary>
    /// Controls laser behavior including visual representation, collision detection, and game state management
    /// </summary>
    public class LaserController : MonoBehaviour
    {
        [Header("Laser Settings")]
        [SerializeField] private bool enable = true;
        [SerializeField] private Direction laserDirection = Direction.Right;
        [SerializeField] private bool useLocalDirection = false;
        [SerializeField] private LayerMask collisionMask = -1;
        [SerializeField] private float maxDistance = 100f;

        [Header("Visual Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Color laserColor = Color.red;
        [SerializeField] private float laserWidth = 0.1f;
        [SerializeField] private Material laserMaterial;

        [Header("Events")]
        [SerializeField] private UnityEvent OnPlayerHit;

        // Tags for collision detection
        private const string PLAYER_TAG = "Player";
        private const string WIRE_TAG = "Wire";
        private const string WALL_TAG = "Wall";

        private Transform laserOrigin;
        private bool isLaserActive;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeLaser();
            SetLaserEnable(enable);
            Debug.Log($"LaserController initialized - Enable: {enable}, Direction: {laserDirection}, Mode: {(useLocalDirection ? "Local" : "Global")}");
        }

        private void Update()
        {
            if (isLaserActive)
            {
                UpdateLaser();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize laser components and settings
        /// </summary>
        private void InitializeLaser()
        {
            laserOrigin = transform;

            // Setup LineRenderer if not assigned
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
            }

            ConfigureLineRenderer();
        }

        /// <summary>
        /// Configure LineRenderer visual properties
        /// </summary>
        private void ConfigureLineRenderer()
        {
            if (lineRenderer == null) return;

            lineRenderer.material = laserMaterial;
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            // Ensure laser renders in front
            lineRenderer.sortingOrder = 10;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enable or disable the laser system
        /// </summary>
        /// <param name="enableLaser">True to enable, false to disable</param>
        public void SetLaserEnable(bool enableLaser)
        {
            enable = enableLaser;
            isLaserActive = enableLaser;

            if (lineRenderer != null)
            {
                lineRenderer.enabled = enableLaser;
            }

            Debug.Log($"Laser {(enableLaser ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Toggle laser on/off
        /// </summary>
        public void ToggleLaser()
        {
            SetLaserEnable(!enable);
        }

        /// <summary>
        /// Set the laser shooting direction
        /// </summary>
        /// <param name="direction">Direction for laser to shoot</param>
        public void SetLaserDirection(Direction direction)
        {
            laserDirection = direction;
            Debug.Log($"Laser direction set to: {direction}");
        }

        /// <summary>
        /// Get the current laser direction
        /// </summary>
        /// <returns>Current laser direction</returns>
        public Direction GetLaserDirection()
        {
            return laserDirection;
        }

        /// <summary>
        /// Set whether to use local or global direction
        /// </summary>
        /// <param name="useLocal">True for local direction, false for global direction</param>
        public void SetUseLocalDirection(bool useLocal)
        {
            useLocalDirection = useLocal;
            Debug.Log($"Laser direction mode set to: {(useLocal ? "Local" : "Global")}");
        }

        /// <summary>
        /// Get whether laser is using local direction
        /// </summary>
        /// <returns>True if using local direction, false if using global</returns>
        public bool IsUsingLocalDirection()
        {
            return useLocalDirection;
        }

        /// <summary>
        /// Toggle between local and global direction modes
        /// </summary>
        public void ToggleDirectionMode()
        {
            SetUseLocalDirection(!useLocalDirection);
        }

        #endregion

        #region Laser Logic

        /// <summary>
        /// Get laser direction as Vector3, considering local vs global space
        /// </summary>
        /// <returns>Laser direction as Vector3</returns>
        private Vector3 GetLaserDirectionVector()
        {
            Vector3 direction = laserDirection.ToVector3();
            
            if (useLocalDirection)
            {
                // Transform direction from local space to world space
                direction = transform.TransformDirection(direction);
            }
            
            return direction;
        }

        /// <summary>
        /// Update laser raycast and visual representation
        /// </summary>
        private void UpdateLaser()
        {
            Vector3 startPosition = laserOrigin.position;
            Vector3 direction = GetLaserDirectionVector();

            // Perform raycast to detect collision
            RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, maxDistance, collisionMask);

            Vector3 endPosition;

            if (hit.collider != null)
            {
                endPosition = hit.point;
                HandleCollision(hit);
            }
            else
            {
                endPosition = startPosition + direction * maxDistance;
            }

            // Update visual representation
            UpdateLaserVisual(startPosition, endPosition);
        }

        /// <summary>
        /// Handle collision detection and response
        /// </summary>
        /// <param name="hit">Raycast hit information</param>
        private void HandleCollision(RaycastHit2D hit)
        {
            GameObject hitObject = hit.collider.gameObject;
            string hitTag = hitObject.tag;

            //Debug.Log($"Laser hit: {hitObject.name} with tag: {hitTag}");

            switch (hitTag)
            {
                case PLAYER_TAG:
                    HandlePlayerHit(hitObject);
                    break;

                case WIRE_TAG:
                    //Debug.Log("Laser blocked by wire");
                    break;

                case WALL_TAG:
                    //Debug.Log("Laser blocked by wall");
                    break;

                default:
                    //Debug.Log($"Laser hit unrecognized object: {hitObject.name}");
                    break;
            }
        }

        /// <summary>
        /// Handle player collision - trigger game over
        /// </summary>
        /// <param name="player">Player GameObject</param>
        private void HandlePlayerHit(GameObject player)
        {
            Debug.Log("Player hit by laser! Game Over!");

            // Trigger lose game event
            OnPlayerHit?.Invoke();

            // Try to find and call LoseGame method
            LoseGame();
        }

        /// <summary>
        /// Trigger game over state
        /// </summary>
        private void LoseGame()
        {
            SceneManager.Instance.OnPlayerDeath();

            Debug.Log("LoseGame triggered by laser");
        }

        /// <summary>
        /// Update laser visual representation
        /// </summary>
        /// <param name="startPos">Laser start position</param>
        /// <param name="endPos">Laser end position</param>
        private void UpdateLaserVisual(Vector3 startPos, Vector3 endPos)
        {
            if (lineRenderer == null) return;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }

        #endregion

        #region Debug and Gizmos

        private void OnDrawGizmos()
        {
            if (!isLaserActive) return;

            Vector3 start = transform.position;
            Vector3 direction = laserDirection.ToVector3();

            Gizmos.color = laserColor;
            Gizmos.DrawRay(start, direction * maxDistance);
        }

        #endregion
    }
}
