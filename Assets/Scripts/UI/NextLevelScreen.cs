using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextLevelScreen : MonoBehaviour {
    public Button nextLevel;
    public Button retryLevel;

    public void NextLevel() {
        UIManager.Instance.LoadNextScene();
    }

    public void RetryLevel() {
        UIManager.Instance.ReloadScene();
    }
}
