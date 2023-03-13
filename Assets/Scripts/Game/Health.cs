using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    public float currHealth;
    public float maxHealth;

    void Start() {
        ResetHealth();
    }
    
    public void ResetHealth() {
        currHealth = maxHealth;
    }

    public bool IsDead() {
        return currHealth <= 0;
    }
}
