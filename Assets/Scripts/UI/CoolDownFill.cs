using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CoolDownFill : MonoBehaviour {
    public float fillHeight;
    public RectTransform ammoBG;
    public WeaponManager wMgr;
    RectTransform rt;
    public Canvas canvas;
    
    // Start is called before the first frame update
    void Start() {
        rt = GetComponent<RectTransform>();
        fillHeight = ammoBG.sizeDelta.y;
    }

    // Update is called once per frame
    void Update() {
        if (!wMgr.initedWeapon) return;
        float cd = wMgr.fireCD;
        float totalCd = wMgr.curr.fireCD;
        
        rt.offsetMax = new Vector2(
            rt.offsetMax.x,
            fillHeight * (Mathf.Clamp01(cd / totalCd) - 1)
        );
    }
}
