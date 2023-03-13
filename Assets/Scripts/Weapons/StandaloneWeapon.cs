using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandaloneWeapon : MonoBehaviour {
    public GameObject bulletPrefab;
    public Animator blackScreen;
    public Transform firePt;
    float lookTime = 2f;
    
    public IEnumerator ShootAtPlayer() {
        gameObject.SetActive(true);
        Debug.Log("start shoot");
        Vector2 v1 = transform.position;
        Vector2 v2 = PlayerMovement.Instance.playerGFX.position;

        Vector2 dir = v2 - v1;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 finalRot = new Vector3(
            0f,
            0f,
            angle + 90f
        );

        float t = 0;
        Vector3 currRot;

        yield return new WaitForSeconds(1f);
        
        Debug.Log("hi");
        while (t < lookTime) {
            currRot = Vector3.Lerp(Vector3.zero, finalRot, t / lookTime);
            transform.rotation = Quaternion.Euler(currRot);
            t += Time.deltaTime;
            yield return null;
        }
        
        currRot = finalRot;
        
        GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(
            firePt.position.x,
            firePt.position.y,
            -1.5f), Quaternion.Euler(currRot));

        bulletObj.GetComponent<Rigidbody2D>().velocity = transform.up * -30f;
        bulletObj.GetComponent<EndGameSequenceBullet>().blackScreen = blackScreen;
        bulletObj.GetComponent<EndGameSequenceBullet>().wpn = gameObject;
    }
}
