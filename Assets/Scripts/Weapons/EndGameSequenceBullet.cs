using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameSequenceBullet : MonoBehaviour {
    public Animator blackScreen;
    public GameObject wpn;
    
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Debug.Log("player hit oops");
            Destroy(PlayerMovement.Instance.playerGFX.gameObject);
            GetComponent<SpriteRenderer>().enabled = false;
            ScreenShakeController.Instance.StartShake(0.5f, 0.25f);

            StartCoroutine(nameof(FadeOutScreen));
        }
    }

    IEnumerator FadeOutScreen() {
        yield return new WaitForSeconds(2f);
        
        UIManager.Instance.HideAll();
        wpn.SetActive(false);
        
        blackScreen.gameObject.SetActive(true);
        blackScreen.SetTrigger("FadeIn");
        
        yield return new WaitForSeconds(2f);

        UIManager.Instance.LoadNextScene();
        // Destroy(gameObject);
    }
}
