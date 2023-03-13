using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    void Awake() {
        if (Instance == null) Instance = this;
    }

    [Header("Canvas Groups (In-Game)")] 
    public List<CanvasGroup> inGameCanvasGroups;
    public int currGroup;

    [Header("UI Elements (In-Game)")] 
    public WeaponSlot weaponSlotPrefab;
    public Transform weaponSlotContainer;
    public bool useWeaponSlots;
    [Space]
    public Image aKey;
    public TMP_Text aKeyText;
    public Image dKey;
    public TMP_Text dKeyText;
    public Image leftMouseImage;
    public Image spaceKey;
    public TMP_Text spaceKeyText;
    [Space]
    public Image wKey;
    public TMP_Text wKeyText;
    public Image sKey;
    public TMP_Text sKeyText;
    [Space]
    public float keyFadeTime = 0.01f;
    public Sprite defaultSprite;
    [Space] 
    public Animator endgameBoxAnimator;


    // Start is called before the first frame update
    void Start() {
        // endgameBoxAnimator.Play("egb-start");
        if (LevelManager.Instance != null) {
            if (LevelManager.Instance.currentLevel >= 1) {
                ShowHUD(false);
            }
        }
    }

    #region Menus

    public void Show(CanvasGroup menu, bool fade = false) {
        foreach (CanvasGroup cg in inGameCanvasGroups) {
            UpdateMenu(cg, false, false);
        }
        
        UpdateMenu(menu, true, fade);
    }

    public void HideAll(bool fade = false) {
        foreach (CanvasGroup cg in inGameCanvasGroups) {
            UpdateMenu(cg, false, (fade && cg.alpha == 1f)); // fades none except open menus
        }
    }

    public void ShowHUD(bool fade = false) {
        CanvasGroup HUD = inGameCanvasGroups.FirstOrDefault(cg => cg.name.Contains("HUD"));
        Show(HUD, fade);
    }

    public void ShowRetryScreen(bool fade = false) {
        CanvasGroup retryScreen = inGameCanvasGroups.FirstOrDefault(cg => cg.name.Contains("RetryScreen"));
        Show(retryScreen, fade);
    }

    public void ShowNextLevelScreen(bool fade = false) {
        CanvasGroup nextLevelScreen = inGameCanvasGroups.FirstOrDefault(cg => cg.name.Contains("NextLevel"));
        Show(nextLevelScreen, fade);
    }

    public void ShowSettingsScreen(bool fade = false) {
        CanvasGroup settingsScreen = inGameCanvasGroups.FirstOrDefault(cg => cg.name.Contains("Settings"));
        Show(settingsScreen, fade);
        PlayerMovement.Instance.pausedGame = true;
    }

    public void ShowEndGameBox(bool fade = false) {
        CanvasGroup endgameScreen = inGameCanvasGroups.FirstOrDefault(cg => cg.name.Contains("Endgame"));
        Show(endgameScreen, fade);
    }

    public void UpdateMenu(CanvasGroup menu, bool show=true, bool fade=true, float fadeTime = 0.2f) {
        if (!fade) {
            menu.GetComponentInChildren<Transform>().gameObject.SetActive(show);
            menu.alpha = Convert.ToSingle(show);
            menu.interactable = show;
            menu.blocksRaycasts = show;
        }
        else {
            menu.GetComponentInChildren<Transform>().gameObject.SetActive(true);
            StartCoroutine(Fade(menu, show, fadeTime));
        }

        inGameCanvasGroups.FirstOrDefault(group => group.name.Contains("Settings")).gameObject.SetActive(true);
        if (show) currGroup = inGameCanvasGroups.IndexOf(menu);
    }

    public IEnumerator Fade(CanvasGroup menu, bool inOrOut, float t) { // fading in is true, fading out is false
        // init menu properties
        menu.alpha = Convert.ToSingle(!inOrOut);
        menu.interactable = inOrOut;
        menu.blocksRaycasts = !inOrOut;
        
        // fade in or out
        int fadeIterations = 20;
        for (float i = 0; i <= 1; i += (1f / fadeIterations)) {
            menu.alpha = (inOrOut ? i : 1 - i);
            yield return new WaitForSeconds(t / fadeIterations);
        }
        menu.alpha = Convert.ToSingle(inOrOut);
        menu.blocksRaycasts = inOrOut;
    }   
    #endregion
    
    #region Scenes
    
    public void LoadNextScene() {
        int index = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        if (SceneManager.GetActiveScene().buildIndex == LevelManager.Instance.lastLevel) {
            BGMusic.Instance.currSong = 1;
            BGMusic.Instance.PlaySong(1);
        } else if (BGMusic.Instance.currSong == 1) {
            BGMusic.Instance.currSong = 0;
            BGMusic.Instance.PlaySong(0);
        }
        LoadScene(index);
    }

    public void GoToMenuScene() {
        LoadScene(0);
    }

    public void GoToLevelScene(int level) {
        LoadScene(Mathf.Min(level, SceneManager.sceneCountInBuildSettings));
    }
    
    public void LoadScene(int sceneIndex) {
        if (sceneIndex == LevelManager.Instance.lastLevel) {
            BGMusic.Instance.currSong = 1;
            BGMusic.Instance.PlaySong(1);
        } else if (BGMusic.Instance.currSong == 1) {
            BGMusic.Instance.currSong = 0;
            BGMusic.Instance.PlaySong(0);
        }
        
        SceneManager.LoadScene(sceneIndex);
        Time.timeScale = 1f;
    }
    
    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }

    public void ReloadScene() {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    #endregion

    #region In-Game UI

    int fadeIterations = 8;
    public IEnumerator FadeGraphic(MaskableGraphic graphic, float t, float startAlpha, float endAlpha, bool changeAlphaOrColor = false) {
        if (changeAlphaOrColor) { // change alpha, not color
            graphic.color = new Color32(255, 255, 255, (byte)((startAlpha * 256) - 1));
            Color finalColor = graphic.color;
            finalColor = new Color32(255, 255, 255, (byte)((startAlpha * 256) - 1));
            
            for (float i = startAlpha; (startAlpha > endAlpha) ? i >= endAlpha : i <= endAlpha ; i += ((endAlpha - startAlpha) / fadeIterations)) {
                Color currColor = new Color32(255, 255, 255, (byte)((i * 256) - 1));
                graphic.color = currColor;
                yield return new WaitForSeconds(t / fadeIterations);
            }

            graphic.color = finalColor;
        }
        else { // change color, not alpha
            // init menu properties
            graphic.color = Color.HSVToRGB(0, 0, startAlpha);
            Color finalColor = Color.HSVToRGB(0, 0, endAlpha);

            // fade in or out
            for (float i = startAlpha; (startAlpha > endAlpha) ? i >= endAlpha : i <= endAlpha ; i += ((endAlpha - startAlpha) / fadeIterations)) {
                Color currColor = Color.HSVToRGB(0, 0, i);
                graphic.color = currColor;
                yield return new WaitForSeconds(t / fadeIterations);
            }

            graphic.color = finalColor;
        }
    }

    public void FadeKeyDown(Image keyImage, TMP_Text keyText, float t) {
        // StartCoroutine(FadeImageIn(keyImage, t));
        // StartCoroutine(FadeTextIn(keyText, t));
        StartCoroutine(FadeGraphic(keyImage, t, 1f, 0.5f));
        StartCoroutine(FadeGraphic(keyText, t, 1f, 0.5f));
    }   
    
    public void FadeKeyUp(Image keyImage, TMP_Text keyText, float t) {
        // StartCoroutine(FadeImageOut(keyImage, t));
        // StartCoroutine(FadeTextOut(keyText, t));
        StartCoroutine(FadeGraphic(keyImage, t, 0.5f, 1f));
        StartCoroutine(FadeGraphic(keyText, t, 0.5f, 1f));
    }

    #endregion
    
    public void QuitApplication() {
        Application.Quit();
    }
}
