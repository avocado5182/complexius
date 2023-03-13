using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour {
    public Gradient colorOverTime;
    public Health health;
    public Image bar;
    public TMP_Text healthText;

    // Update is called once per frame
    void Update() {
        bar.fillAmount = health.currHealth / health.maxHealth;
        bar.color = colorOverTime.Evaluate(health.currHealth / health.maxHealth);
        healthText.text = $"{health.currHealth} / {health.maxHealth}";
    }
}
