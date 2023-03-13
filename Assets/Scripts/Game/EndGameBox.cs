using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameBox : MonoBehaviour {
    public StandaloneWeapon boxWeapon;
    public void Shoot() {
        StartCoroutine(boxWeapon.ShootAtPlayer());
    }
}
