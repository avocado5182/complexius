using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Weapon weapon;
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start() {
        // rb.velocity = transform.up * weapon.fireSpeed;
        rb.AddForce(transform.up * weapon.fireSpeed, ForceMode2D.Impulse);
        Destroy(gameObject, 4f);
    }

    bool pausedVelocity = false;
    void Update() {
        if (PlayerMovement.Instance.pausedGame && !pausedVelocity) {
            pausedVelocity = true;
            rb.velocity = Vector2.zero;
        } else if (!PlayerMovement.Instance.pausedGame && pausedVelocity) {
            pausedVelocity = false;
            rb.AddForce(transform.up * weapon.fireSpeed, ForceMode2D.Impulse);            
        }
    }

    public void OnShot(Collider2D other) {
        // destroy bullet, add boom
        ParticleSystem boom = Instantiate(weapon.impactParticles, transform);
        // boom.transform.position = new Vector3(
        //     boom.transform.position.x,
        //     boom.transform.position.y,
        //     -1);
        
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        rb.velocity = Vector2.zero;

        Health health = other.gameObject.GetComponent<Health>();
        if (health != null) {
            health.currHealth -= weapon.damage;

            if (weapon.useSound) {
                weapon.audioSrc.clip = weapon.bulletImpactSound;
                weapon.audioSrc.Play();
            }
        }

        Invoke(nameof(Destroy), boom.main.duration / boom.main.simulationSpeed);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (weapon.side) { // player weapon
            if ((other.CompareTag("Enemy") || other.CompareTag("EnemyBullet")) && !other.CompareTag("BossBullet")) {
                OnShot(other);
            }
        }
        else { // enemy weapon
            if (other.CompareTag("Player") || (other.CompareTag("PlayerBullet") && !weapon.enemyAI.isBoss)) {
                OnShot(other);
            }
        }
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
