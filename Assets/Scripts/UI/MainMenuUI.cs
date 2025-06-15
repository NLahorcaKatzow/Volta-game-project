using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{

    [SerializeField] private Slider masterVolumeSlider;

    private void Start(){
        masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
    }


    public void PlayGame(){
        SceneManager.Instance.LoadNextLevel();
    }
    
    public void ChangeMasterVolume(float volume){
        AudioManager.Instance.SetMasterVolume(volume);
    }
}
