using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelScreenGrid : MonoBehaviour {
    public GameObject lvlBtnPrefab;
    
    // Start is called before the first frame update
    void Start() {
        int i = 0;
        while (i < LevelManager.Instance.lastLevel) {
            int indx = i;
            GameObject btnPrefab = Instantiate(lvlBtnPrefab, transform);
            Button btn = btnPrefab.GetComponent<Button>();
            btn.onClick.AddListener(() => { UIManager.Instance.LoadScene(indx + 1); });
            btn.GetComponentInChildren<TMP_Text>().text = $"Level {indx + 1}";

            i++;
        }
    }
}
