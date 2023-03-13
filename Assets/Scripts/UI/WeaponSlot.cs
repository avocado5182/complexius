using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour {
    public Image bgImage;
    public Image weaponImage;
    public Image selectedLine;

    void SetGraphicAlpha(MaskableGraphic graphic, float alpha) {
        graphic.color = new Color(
            graphic.color.r,
            graphic.color.g,
            graphic.color.b,
            alpha
        );
    }
    
    public void ShowWithAlpha(float alpha) {
        SetGraphicAlpha(bgImage, alpha);
        SetGraphicAlpha(weaponImage, 1f - alpha);
        SetGraphicAlpha(selectedLine, 1f - alpha);
    }
}
