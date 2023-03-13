using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryScreen : MonoBehaviour {
    public Button yes;
    public Button no;

    public void RetryLevel() {
        UIManager.Instance.ReloadScene();
    }

    public void GoBack() {
        UIManager.Instance.LoadScene(0); // menu scene
    }
}