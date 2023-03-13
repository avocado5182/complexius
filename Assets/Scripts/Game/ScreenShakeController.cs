using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ScreenShakeController : MonoBehaviour {
    public static ScreenShakeController Instance;

    void Awake() {
        if (Instance == null) Instance = this;
        startPos = transform.position;
    }

    Vector3 startPos;
    
    float shakeTime;
    float shakePower;
    float shakeFadeTime;

    void LateUpdate() {
        if (PlayerMovement.Instance.pausedGame) return;
        if (shakeTime > 0) {
            shakeTime -= Time.deltaTime;
            float xAmt = Random.Range(-1f, 1f) * shakePower;
            float yAmt = Random.Range(-1f, 1f) * shakePower;
            
            transform.position += new Vector3(xAmt, yAmt, 0);

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
        }
        else {
            float moveX = Mathf.MoveTowards(transform.position.x, startPos.x, shakeFadeTime * 3 * Time.deltaTime);
            float moveY = Mathf.MoveTowards(transform.position.y, startPos.y, shakeFadeTime * 3 * Time.deltaTime);

            transform.position = new Vector3(moveX, moveY, startPos.z);
        }
    }

    public void StartShake(float time, float power) {
        shakeTime = time;
        shakePower = power;
        
        shakeFadeTime = power / time;
    }
}
