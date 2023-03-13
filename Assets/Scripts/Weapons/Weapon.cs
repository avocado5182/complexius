using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Weapon : MonoBehaviour {
	[Header("GOs/Transforms")] public GameObject enemyObj;
	public EnemyAI enemyAI;
	public GameObject bulletPrefab;
	public Transform firePt;
	public ParticleSystem impactParticles;
	public ParticleSystem muzzleFlashParticles;
	[Space] public bool useSound;
	public AudioClip bulletImpactSound;
	public AudioSource audioSrc;
	public AudioClip bulletShootSound;

	[Header("Weapon Variables")] public WeaponType type = WeaponType.Single;
	public Sprite weaponPreview;
	public bool hasWeaponSlot = false;
	public WeaponSlot weaponSlot;

	public bool side = true; // true is player weapon, false is enemy weapon
	public int bursts;
	public int currBurst;
	public Vector3 spawnLocalPos;
	public float damage;
	public float fireSpeed = 50f;
	public float fireCD = 0.1f;

	public int currAmmo;
	public int maxAmmo = 100;

	public float reloadTime = 1f;
	public bool isReloading = false;
	[HideInInspector] public bool canFire = true;

	void Start() {
		if (!side) enemyAI = enemyObj.GetComponent<EnemyAI>();
		audioSrc = GetComponent<AudioSource>();
		if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
		audioSrc.outputAudioMixerGroup = PlayerMovement.Instance.sfxMixer;
		audioSrc.playOnAwake = false;
		audioSrc.loop = false;
	}

	public void Shoot() {
		if (canFire && !isReloading) {
			Transform gfxToUse = (side)
				? PlayerMovement.Instance.playerGFX
				: enemyAI.enemyGFX;
			GameObject bulletObj = Instantiate(bulletPrefab, new Vector3(
				firePt.position.x,
				firePt.position.y,
				-1.5f), gfxToUse.rotation);
			ParticleSystem mFlashObj = Instantiate(muzzleFlashParticles, new Vector3(
				firePt.position.x,
				firePt.position.y,
				-1.6f), Quaternion.identity);
			mFlashObj.Play();

			if (useSound) {
				audioSrc.clip = bulletShootSound;
				audioSrc.Play();
			}

			bulletObj.GetComponent<Bullet>().weapon = this;
			// bulletObj.transform.SetParent(gunObj.transform);

			currAmmo--;
		}
	}

	public IEnumerator Reload() {
		yield return new WaitForSeconds(reloadTime);
		if (currBurst == 0) currBurst = bursts;
		currAmmo = maxAmmo;
		if (type == WeaponType.Burst) currBurst--;
		canFire = true;
		isReloading = false;
	}
}