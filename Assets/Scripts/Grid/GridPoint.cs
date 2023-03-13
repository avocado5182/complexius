using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using org.matheval;
using TMPro;

public class GridPoint : MonoBehaviour {
    public Vector2 position;
    public Vector2 wsPosition;

    public SpriteRenderer sr;

    public Transform tooltip;
    public bool useTooltip = true;
    Vector2 oldPos;//complex

    public Vector2 tooltipOffset = Vector2.one * 0.8f;
    // Start is called before the first frame update
    void Start() {
        oldPos = position;
        // instantiate new tooltip, make it inactive
        if (useTooltip) {
            tooltip = UpdateTooltip();
            tooltip.gameObject.SetActive(false);
        }
    }

    void Update() {
        if (oldPos != position) {// basically, on position change
            oldPos = position;
            if (useTooltip) {
                // update tooltip
                tooltip = UpdateTooltip();
                tooltip.gameObject.SetActive(false);
            }
        }
    }

    public string ComplexPosToString(Vector2 pos) {
        // u + vi or u - vi
        return $"{pos.x} {(pos.y < 0 ? "-" : "+")} {(pos.y < 0 ? pos.y.ToString().Remove(0,1) : pos.y.ToString())}i";
    } 

    Transform UpdateTooltip() {
        // make new game object, set width & height based on pt position length (of .tostring)
        // set position to above or below or left or right based on:
        // is the def offset (+, +) above the screen? make it below
        // is the def offset        on the right of the screen? make it left 
        // is the def offset        on the left of the screen? make it right

        // instantiate tooltip obj and spriterenderer
        GameObject tooltipObj = new GameObject();
        RectTransform tooltipRT = tooltipObj.AddComponent<RectTransform>();
        tooltipRT.SetParent(transform);

        SpriteRenderer toolTipSr = tooltipObj.AddComponent<SpriteRenderer>();
        toolTipSr.sprite = UIManager.Instance.defaultSprite;
        
        // add text to tooltip
        GameObject textObj = new GameObject();
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.SetParent(tooltipObj.transform);
        textRT.pivot = Vector2.one * 0.5f; // (0.5, 0.5)
        textRT.localPosition = Vector3.zero;

        TextMeshPro txt = textObj.AddComponent<TextMeshPro>();
        txt.fontSize = 4f;
        txt.text = ComplexPosToString(position);
        txt.horizontalAlignment = HorizontalAlignmentOptions.Center;
        txt.verticalAlignment = VerticalAlignmentOptions.Middle;

        // position and scale the tooltip
        
        
        // float textWidth = txt.textBounds.size.x;
        // Debug.Log(textWidth);
        // float charWidth = textWidth / txt.text.Length;
        Vector2 padding = new Vector2(0.1f, 0.1f);
        // Vector3 scale = new Vector3(
        //     textWidth + padding.x,
        //     GridRenderer.Instance.lineDist + padding.y,
        //     1f
        // );
        // tooltipObj.transform.localScale = scale;
        
        // http://answers.unity.com/comments/1540206/view.html thanks
        // textComponent.text = ...; // set the text to something new
        // RectTransform parentRect = ...; // somehow get the parent RectTransform
        // parentRect.sizeDelta = new Vector2(textComponent.preferredWidth, textComponent.preferredHeight) + padding; // padding is a Vector2 width the padding you want to add around the text, in pixels

        Vector2 scale = new Vector2(txt.preferredWidth, txt.preferredHeight) + padding;
        toolTipSr.size = scale;
        toolTipSr.drawMode = SpriteDrawMode.Sliced;
        toolTipSr.color = Color.HSVToRGB(0, 0, 0.19f);
        tooltipRT.sizeDelta = scale;
        float coolNumber = 11f; // idk why this is 11
        tooltipRT.localScale = Vector2.one * coolNumber;
        textRT.sizeDelta = scale - padding;

        // makes "tooltipOffset" the bottom left of the tooltip
        // Vector2 offsettedPos = 
        //     transform.position + 
        //     (Vector3)(tooltipOffset) + 
        //     (Vector3)(scale / 2);
        Vector2 offsettedPos = (scale * (coolNumber / 2f)) + tooltipOffset;
        // Debug.Log($"{offsettedPos} ({transform.name})");
        // Debug.Log($"{offsettedPos.x + (scale.x / 2)}: {offsettedPos.x + (scale.x / 2) >= GridRenderer.Instance.screenWidth / 2}");
        bool outOfBounds = false;

        tooltipRT.localPosition = new Vector3(
                offsettedPos.x,
                offsettedPos.y,
                transform.localPosition.z);
        
        if (tooltipRT.position.x + (scale.x / 2) >= GridRenderer.Instance.screenWidth / 2) {
            tooltipRT.position = new Vector3(
                (GridRenderer.Instance.screenWidth / 2) - (scale.x) - (padding.x),
                tooltipRT.position.y,
                tooltipRT.position.z
            );
        } else if (tooltipRT.position.x - (scale.x / 2) <= -GridRenderer.Instance.screenWidth / 2) {
            tooltipRT.position = new Vector3(
                -(GridRenderer.Instance.screenWidth / 2) + (scale.x) + (padding.x), 
                tooltipRT.position.y,
                tooltipRT.position.z
            );
        } else if (tooltipRT.position.y + (scale.y / 2) >= GridRenderer.Instance.screenHeight / 2) {
            tooltipRT.position = new Vector3(
                tooltipRT.position.x,
                (GridRenderer.Instance.screenHeight / 2) - (scale.y) - (padding.y), 
                tooltipRT.position.z
            );
        } else if (tooltipRT.position.y - (scale.y / 2) <= -GridRenderer.Instance.screenHeight / 2) {
            tooltipRT.position = new Vector3(
                tooltipRT.position.x,
                -(GridRenderer.Instance.screenHeight / 2) + (scale.y) + (padding.y), 
                tooltipRT.position.z
            );
        }

        // return
        return tooltipRT;
    }

    public void OnStartHover() {
        if (useTooltip) {
            tooltip.gameObject.SetActive(true);
        }
    }

    public void OnEndHover() {
        if (useTooltip) {
            tooltip.gameObject.SetActive(false);
        }
    }

    void OnMouseEnter() {
        OnStartHover();
    }

    void OnMouseExit() {
        OnEndHover();
    }
}