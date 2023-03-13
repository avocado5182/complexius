using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsScreen : MonoBehaviour {
    public AudioMixer mixer;
    // CanvasGroup oldCg;
    bool isOpen = false;
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !isOpen) {
            isOpen = true;
            // oldCg = UIManager.Instance.inGameCanvasGroups[UIManager.Instance.currGroup];
            UIManager.Instance.ShowSettingsScreen();
        } else if (Input.GetKeyDown(KeyCode.Escape) && isOpen) {
            isOpen = false;
            // UIManager.Instance.Show(oldCg, false);
            UIManager.Instance.ShowHUD(false);
            PlayerMovement.Instance.pausedGame = false;
        }
        
    }
    
    public void OnVolumeChange(float volume) {
        mixer.SetFloat("volume", volume);
    }

    public void BackToMenu() {
        UIManager.Instance.LoadScene(0);
    }

    public void ResumeGame() {
        UIManager.Instance.ShowHUD(false);
        PlayerMovement.Instance.pausedGame = false;
    }
}
