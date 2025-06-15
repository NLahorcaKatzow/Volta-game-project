using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string[] levelScenes;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private float transitionDelay = 2f;
    [SerializeField] private bool debugMode = true;
    
    [Header("Transition Effects")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease fadeEaseIn = Ease.InOutQuad;
    [SerializeField] private Ease fadeEaseOut = Ease.InOutQuad;
    
    [Header("Death UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private CanvasGroup deathCanvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private float deathUIFadeDuration = 0.5f;
    
    public static SceneManager Instance;
    private int currentLevelIndex = 0;
    private bool isTransitioning = false;
    private bool isDeathUIActive = false;
    
    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (debugMode)
                Debug.Log("SceneManager singleton created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            if (debugMode)
                Debug.Log("Duplicate SceneManager found, destroying...");
            Destroy(gameObject);
            return;
        }
        
        // Initialize level index based on current scene
        InitializeCurrentLevel();
    }
    
    void Start()
    {
        // Setup fade panel if not assigned
        if (fadePanel == null)
        {
            SetupFadePanel();
        }
        
        
        // Setup restart button event
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }
    
    private void InitializeCurrentLevel()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == currentScene)
            {
                currentLevelIndex = i;
                if (debugMode)
                    Debug.Log($"Current level index set to: {currentLevelIndex} (Scene: {currentScene})");
                return;
            }
        }
        
        if (debugMode)
            Debug.Log($"Current scene '{currentScene}' not found in level list. Starting from level 0.");
    }
    
    private void SetupFadePanel()
    {
        
        if (fadePanel == null)
        {
            // Create a simple fade panel if none exists
            GameObject fadeObject = new GameObject("FadePanel");
            Canvas canvas = fadeObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            fadePanel = fadeObject.AddComponent<CanvasGroup>();
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            
            // Add a black background
            UnityEngine.UI.Image image = fadeObject.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            
            DontDestroyOnLoad(fadeObject);
            
            if (debugMode)
                Debug.Log("Created default fade panel");
        }
    }
    
    
    
    private void OnRestartButtonClicked()
    {
        if (debugMode)
            Debug.Log("Restart button clicked - restarting current level");
        
        StartCoroutine(HideDeathUIAndRestart());
    }
    
    private IEnumerator HideDeathUIAndRestart()
    {
        if (deathCanvasGroup != null)
        {
            // Fade out death UI
            yield return deathCanvasGroup.DOFade(0f, deathUIFadeDuration).WaitForCompletion();
            deathCanvasGroup.blocksRaycasts = false;
            deathCanvasGroup.interactable = false;
            deathUI.SetActive(false);
        }
        
        isDeathUIActive = false;
        
        // Restart the current level
        RestartCurrentLevel();
    }
    
    public void ShowDeathUI()
    {
        if (isDeathUIActive || isTransitioning)
        {
            if (debugMode)
                Debug.Log("Death UI already active or transitioning, ignoring ShowDeathUI call");
            return;
        }
        
        StartCoroutine(ShowDeathUICoroutine());
    }
    
    private IEnumerator ShowDeathUICoroutine()
    {
        isDeathUIActive = true;
        
        if (debugMode)
            Debug.Log("Showing death UI");
        
        if (deathUI != null && deathCanvasGroup != null)
        {
            deathUI.SetActive(true);
            deathCanvasGroup.blocksRaycasts = true;
            deathCanvasGroup.interactable = true;
            
            // Fade in death UI
            yield return deathCanvasGroup.DOFade(1f, deathUIFadeDuration).WaitForCompletion();
            
            if (debugMode)
                Debug.Log("Death UI fully visible");
        }
    }
    
    public void HideDeathUI()
    {
        if (!isDeathUIActive)
        {
            if (debugMode)
                Debug.Log("Death UI not active, ignoring HideDeathUI call");
            return;
        }
        
        StartCoroutine(HideDeathUICoroutine());
    }
    
    private IEnumerator HideDeathUICoroutine()
    {
        if (debugMode)
            Debug.Log("Hiding death UI");
        
        if (deathCanvasGroup != null)
        {
            // Fade out death UI
            yield return deathCanvasGroup.DOFade(0f, deathUIFadeDuration).WaitForCompletion();
            deathCanvasGroup.blocksRaycasts = false;
            deathCanvasGroup.interactable = false;
            deathUI.SetActive(false);
        }
        
        isDeathUIActive = false;
        
        if (debugMode)
            Debug.Log("Death UI hidden");
    }
    
    public void LoadNextLevel()
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadNextLevel call");
            return;
        }
        
        int nextLevelIndex = currentLevelIndex + 1;
        
        if (nextLevelIndex < levelScenes.Length)
        {
            StartCoroutine(TransitionToLevel(nextLevelIndex));
        }
        else
        {
            if (debugMode)
                Debug.Log("No more levels available, going to main menu");
            StartCoroutine(TransitionToMainMenu());
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadLevel call");
            return;
        }
        
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            StartCoroutine(TransitionToLevel(levelIndex));
        }
        else
        {
            Debug.LogError($"Invalid level index: {levelIndex}. Valid range: 0-{levelScenes.Length - 1}");
        }
    }
    
    public void RestartCurrentLevel()
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring RestartCurrentLevel call");
            return;
        }
        
        // Hide death UI if it's active
        if (isDeathUIActive)
        {
            HideDeathUI();
        }
        
        StartCoroutine(TransitionToLevel(currentLevelIndex));
    }
    
    public void LoadMainMenu()
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadMainMenu call");
            return;
        }
        
        StartCoroutine(TransitionToMainMenu());
    }
    
    private IEnumerator TransitionToLevel(int levelIndex)
    {
        isTransitioning = true;
        
        if (debugMode)
            Debug.Log($"Transitioning to level {levelIndex}: {levelScenes[levelIndex]}");
        
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        // Fade out
        yield return FadeOut();
        
        // Load the scene
        currentLevelIndex = levelIndex;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelScenes[levelIndex]);
        
        // Wait a frame for scene to load
        yield return null;
        
        // Fade in
        yield return FadeIn();
        
        isTransitioning = false;
        
        if (debugMode)
            Debug.Log($"Successfully loaded level: {levelScenes[levelIndex]}");
    }
    
    private IEnumerator TransitionToMainMenu()
    {
        isTransitioning = true;
        
        if (debugMode)
            Debug.Log("Transitioning to main menu");
        
        // Wait for transition delay
        yield return new WaitForSeconds(transitionDelay);
        
        // Fade out
        yield return FadeOut();
        
        // Load main menu
        currentLevelIndex = 0; // Reset to first level
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        
        // Wait a frame for scene to load
        yield return null;
        
        // Fade in
        yield return FadeIn();
        
        isTransitioning = false;
        
        if (debugMode)
            Debug.Log("Successfully loaded main menu");
    }
    
    private IEnumerator FadeOut()
    {
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            fadePanel.interactable = false;
            
            // Use DOTween for smooth fade out
            Tween fadeTween = fadePanel.DOFade(1f, fadeDuration).SetEase(fadeEaseOut);
            yield return fadeTween.WaitForCompletion();
            
            if (debugMode)
                Debug.Log("Fade out completed");
        }
    }
    
    private IEnumerator FadeIn()
    {
        if (fadePanel != null)
        {
            // Use DOTween for smooth fade in
            Tween fadeTween = fadePanel.DOFade(0f, fadeDuration).SetEase(fadeEaseIn);
            yield return fadeTween.WaitForCompletion();
            
            fadePanel.blocksRaycasts = false;
            fadePanel.interactable = true;
            
            if (debugMode)
                Debug.Log("Fade in completed");
        }
    }
    
    // Public getters for external access
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
    
    public string GetCurrentLevelName()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levelScenes.Length)
            return levelScenes[currentLevelIndex];
        return "Unknown";
    }
    
    public int GetTotalLevels()
    {
        return levelScenes.Length;
    }
    
    public bool HasNextLevel()
    {
        return currentLevelIndex + 1 < levelScenes.Length;
    }
    
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    public bool IsDeathUIActive()
    {
        return isDeathUIActive;
    }
    
    // Method to be called by GoalController
    public void OnLevelCompleted()
    {
        if (debugMode)
            Debug.Log("Level completed! Preparing to load next level...");
        
        LoadNextLevel();
    }
    
    // Method to be called when player dies
    public void OnPlayerDeath()
    {
        if (debugMode)
            Debug.Log("Player died! Showing death UI...");
        
        ShowDeathUI();
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
