using TMPro;
using UnityEngine;

public class MovementUGUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI movementText;
    [SerializeField] private PlayerController playerController;

    private void Awake(){
        if(playerController == null){
            playerController = FindFirstObjectByType<PlayerController>();
        }
        playerController.OnChangeMovementHistory += OnChangeMovementHistory;
        OnChangeMovementHistory(0);
    }
    
    private void OnDestroy(){
        playerController.OnChangeMovementHistory -= OnChangeMovementHistory;
    }

    private void OnChangeMovementHistory(int currentHistorySize)
    {
        movementText.text = $"Movs: \n{playerController.maxHistorySize} // {currentHistorySize}";
    }
}