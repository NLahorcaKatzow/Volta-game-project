using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PuzzleController.ButtonController
{
    /// <summary>
    /// Simple trigger controller that executes events when objects enter/exit
    /// </summary>
    public class ButtonController : MonoBehaviour
    {
        [Header("Basic Settings")]
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private bool debugMode = true;

        [Header("Collision Detection")]
        [SerializeField] private BoxCollider2D buttonCollider;
        [SerializeField] private LayerMask detectionMask = -1;
        [SerializeField] private string[] validTags = { "Player" };

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer buttonRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color triggeredColor = Color.yellow;
        [SerializeField] private bool useVisualFeedback = true;

        [Header("Events")]
        [SerializeField] private UnityEvent OnTriggerEnter;
        [SerializeField] private UnityEvent OnTriggerExit;

        // State variables
        private HashSet<string> validTagsSet = new HashSet<string>();
        private bool isTriggered = false;

        private HashSet<GameObject> objectsInTrigger = new HashSet<GameObject>();

        #region Unity Lifecycle

        void Awake()
        {
            InitializeButton();
        }

        void Start()
        {
            SetupVisualFeedback();

            if (debugMode)
                Debug.Log($"ButtonController initialized - Enabled: {isEnabled}");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize button components and settings
        /// </summary>
        private void InitializeButton()
        {
            // Get BoxCollider2D if not assigned
            if (buttonCollider == null)
            {
                buttonCollider = GetComponent<BoxCollider2D>();
                if (buttonCollider == null)
                {
                    buttonCollider = gameObject.AddComponent<BoxCollider2D>();
                    Debug.Log("Created BoxCollider2D for button");
                }
            }

            // Ensure collider is set as trigger
            buttonCollider.isTrigger = true;

            // Get SpriteRenderer if not assigned
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
            }

            // Convert tags array to HashSet for faster lookup
            validTagsSet.Clear();
            foreach (string tag in validTags)
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    validTagsSet.Add(tag);
                }
            }
        }

        /// <summary>
        /// Setup visual feedback system
        /// </summary>
        private void SetupVisualFeedback()
        {
            if (useVisualFeedback && buttonRenderer != null)
            {
                buttonRenderer.color = normalColor;
            }
        }

        #endregion

        #region Trigger Detection

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isEnabled) return;
            GameObject obj = other.gameObject;
            objectsInTrigger.Add(obj);
            if(objectsInTrigger.Count > 1) return;
            // Check if object is valid for detection
            if (!IsValidObject(obj)) return;

            if (debugMode)
                Debug.Log($"Object entered trigger: {obj.name} (Tag: {obj.tag})");

            // Update visual feedback
            if (!isTriggered)
            {
                isTriggered = true;
                UpdateVisualFeedback(true);
            }

            // Trigger event
            OnTriggerEnter?.Invoke();

            
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!isEnabled) return;
            GameObject obj = other.gameObject;
            objectsInTrigger.Remove(obj);

            if (objectsInTrigger.Count > 0) return;


            // Check if object is valid for detection
            if (!IsValidObject(obj)) return;

            if (debugMode)
                Debug.Log($"Object exited trigger: {obj.name} (Tag: {obj.tag})");

            // Update visual feedback
            if (isTriggered)
            {
                isTriggered = false;
                UpdateVisualFeedback(false);
            }

            // Trigger event
            OnTriggerExit?.Invoke();

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Update visual feedback based on trigger state
        /// </summary>
        /// <param name="triggered">Whether trigger is active</param>
        private void UpdateVisualFeedback(bool triggered)
        {
            if (!useVisualFeedback || buttonRenderer == null) return;

            buttonRenderer.color = triggered ? triggeredColor : normalColor;
        }

        /// <summary>
        /// Check if object is valid for trigger activation
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if object is valid</returns>
        private bool IsValidObject(GameObject obj)
        {
            if (obj == null) return false;

            // Check layer mask
            int objLayer = obj.layer;
            if ((detectionMask.value & (1 << objLayer)) == 0) return false;

            // Check if object has valid tag
            if (validTagsSet.Count > 0)
            {
                return validTagsSet.Contains(obj.tag);
            }

            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enable or disable the trigger
        /// </summary>
        /// <param name="enabled">True to enable, false to disable</param>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;

            if (!enabled && isTriggered)
            {
                // Reset visual feedback when disabled
                isTriggered = false;
                UpdateVisualFeedback(false);
            }

            if (debugMode)
                Debug.Log($"ButtonController {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Toggle enabled state
        /// </summary>
        public void Toggle()
        {
            SetEnabled(!isEnabled);
        }

        /// <summary>
        /// Check if trigger is enabled
        /// </summary>
        /// <returns>True if trigger is enabled</returns>
        public bool IsEnabled()
        {
            return isEnabled;
        }

        /// <summary>
        /// Check if trigger is currently active
        /// </summary>
        /// <returns>True if trigger is active</returns>
        public bool IsTriggered()
        {
            return isTriggered;
        }

        #endregion

        #region Debug

        void OnDrawGizmos()
        {
            if (buttonCollider != null)
            {
                Gizmos.color = isTriggered ? Color.green : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(buttonCollider.offset, buttonCollider.size);
            }
        }

        #endregion
    }
}
