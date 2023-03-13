using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponManager : MonoBehaviour {
    public static WeaponManager Instance;
    public List<KeyCode> fire1;
    public LayerMask uiLayerMask;

    void Awake() {
        if (Instance == null) Instance = this;
    }

    public PlayerMovement playerMovement;
    public EnemyAI enemyAI;

    public List<Weapon> weapons;
    public List<Weapon> instWeapons;
    public int currWeapon = -1;
    [HideInInspector] public Weapon curr;
    [HideInInspector] public float fireCD;

    public TMP_Text ammoText;
    
    int Wrap(int val, int min, int max) {
        int ret = val; // 0
        if (ret > max) ret -= max + 1; 
        else if (ret < min) ret = max - ((min + ret) + 1);

        return ret;
    }

    bool IsMouseOverUI() {
        return EventSystem.current.IsPointerOverGameObject();
    }

    void Start() {
        uiLayerMask = LayerMask.NameToLayer("UI");
        
        Transform gfxToUse = (playerMovement != null) ? playerMovement.playerGFX : enemyAI.enemyGFX;
        for (int i = 0; i < weapons.Count; i++) {
            Weapon newWeapon = Instantiate(weapons[i], gfxToUse);
            if (!newWeapon.side) newWeapon.enemyObj = gameObject;
            newWeapon.transform.localPosition = newWeapon.spawnLocalPos;
            newWeapon.transform.rotation = Quaternion.identity;
            newWeapon.gameObject.SetActive(i == currWeapon);
            newWeapon.currAmmo = newWeapon.maxAmmo;
            newWeapon.currBurst = newWeapon.bursts;
            instWeapons.Add(newWeapon);
        }
    }

    public bool initedWeapon = false;
    // Update is called once per frame
    void Update() {
        if (PlayerMovement.Instance.pausedGame) return;
        bool updated = false;
        
        if (currWeapon == -1 && !initedWeapon) {
            initedWeapon = true;
            currWeapon = 0;
            curr = instWeapons[currWeapon];
            if (curr.side) {
                UpdateAmmoText(curr);
                if (UIManager.Instance.useWeaponSlots) UpdateWeaponSlots(currWeapon);
            }
            
            fireCD = 0f;
            
            for (int i = 0; i < instWeapons.Count; i++) {
                instWeapons[i].gameObject.SetActive(false);
            }
            
            curr.gameObject.SetActive(true);
        }

        if (curr.side) if (playerMovement.hasWon) return;
        
        // shooting
        bool fire1Down = false;
        bool fire1Held = false;
        bool fire1Up = false;
        
        foreach (KeyCode key in fire1) {
            bool down = Input.GetKeyDown(key);
            bool held = Input.GetKey(key);
            bool up = Input.GetKeyUp(key);
            if (curr.type == WeaponType.Single ? down : held) {
                // // Checking both WeaponType and Input.GetKey/Down/Up w/ this combination might cause issues
                // if (curr.type == WeaponType.Single && down) fire1Down = true; 
                // if (curr.type != WeaponType.Single && held) fire1Held = true;
                
                fire1Down = down;
                fire1Held = held;
                if (LevelManager.Instance.currentLevel == 1) {
                    if (key == fire1[0]) { // to be main fire1, Mouse0
                        // set left mouse img alpha to 1 (opaque)
                        Color lMouseImgColor = UIManager.Instance.leftMouseImage.color;
                        UIManager.Instance.leftMouseImage.color = new Color32(
                            (byte)(lMouseImgColor.r * 256 - 1),
                            (byte)(lMouseImgColor.g * 256 - 1),
                            (byte)(lMouseImgColor.b * 256 - 1),
                            255
                        );
                    } else if (key == fire1[1]) { // to be alt fire1, Space
                        UIManager uiMgr = UIManager.Instance;
                        StartCoroutine(uiMgr.FadeGraphic(uiMgr.spaceKey,     uiMgr.keyFadeTime, 1f, 0.5f));
                        StartCoroutine(uiMgr.FadeGraphic(uiMgr.spaceKeyText, uiMgr.keyFadeTime, 1f, 0.5f));
                    }
                } 
            } else if (up) {
                fire1Up = true;
                if (LevelManager.Instance.currentLevel == 1) {
                    if (key == fire1[0]) { // to be main fire1, Mouse0
                        // set left mouse img alpha to 0 (transparent)
                        Color lMouseImgColor = UIManager.Instance.leftMouseImage.color;
                        UIManager.Instance.leftMouseImage.color = new Color32(
                            (byte)(lMouseImgColor.r * 256 - 1),
                            (byte)(lMouseImgColor.g * 256 - 1),
                            (byte)(lMouseImgColor.b * 256 - 1),
                            0
                        );
                    } else if (key == fire1[1]) { // to be alt fire1, Space
                        UIManager uiMgr = UIManager.Instance;
                        StartCoroutine(uiMgr.FadeGraphic(uiMgr.spaceKey,     uiMgr.keyFadeTime, 0.5f, 1f));
                        StartCoroutine(uiMgr.FadeGraphic(uiMgr.spaceKeyText, uiMgr.keyFadeTime, 0.5f,  1f));
                    }
                }
            }
        }
        
        if (curr.currAmmo == 0 && 
            // ((curr.currBurst > 1 && curr.type == WeaponType.Burst) || curr.type != WeaponType.Burst) && 
            !curr.isReloading) {
            curr.canFire = false;
            curr.isReloading = true;
            if (curr.side) ammoText.text = "reloading...";
            StartCoroutine(curr.Reload());
        }

        if (!curr.isReloading) {
            UpdateCurrWeapon(out updated);

            fireCD += Time.deltaTime;
        
            if ((curr.type == WeaponType.Single ? fire1Down : fire1Held) && 
                fireCD >= curr.fireCD && 
                curr.currAmmo > 0 &&
                curr.canFire && curr.side && !playerMovement.hasDied &&
                !IsMouseOverUI()) {
                fireCD = 0;
                curr.Shoot();
            }
        }
    }

    void UpdateAmmoText() {
        ammoText.text = $"{curr.currAmmo} / {curr.maxAmmo}";
    }
    
    void UpdateAmmoText(Weapon weapon) {
        if (weapon.type == WeaponType.Burst) {
            ammoText.text = $"{weapon.currAmmo + (weapon.maxAmmo * (weapon.currBurst - 1))} / {weapon.maxAmmo * weapon.bursts}";            
        }
        else {
            ammoText.text = $"{weapon.currAmmo} / {weapon.maxAmmo}";
        }
    }

    void UpdateWeaponSlot(Weapon wpn, float alpha) {
        // instantiate weapon slot prefab if !wpn.hasWeaponSlot
        // set alpha of it, based on "pivot"
        // pivot calculated by indx of weapon in instWeapons
        WeaponSlot slot;
        if (!wpn.hasWeaponSlot) {
            wpn.hasWeaponSlot = true;
            slot = Instantiate(UIManager.Instance.weaponSlotPrefab, UIManager.Instance.weaponSlotContainer);
            slot.weaponImage.sprite = wpn.weaponPreview;
            wpn.weaponSlot = slot;
        }
        else slot = wpn.weaponSlot;

        slot.ShowWithAlpha(alpha);
    }

    void UpdateWeaponSlots(int pivot) {
        for (int i = 0; i < instWeapons.Count; i++) {
            float alpha = Mathf.Abs(i - pivot) / (float)(instWeapons.Count + 1);
            Debug.Log($"curr: {currWeapon}, i: {i}, alpha: {alpha}");
            UpdateWeaponSlot(instWeapons[i], alpha);
        }
    }
    
    void UpdateCurrWeapon(out bool updated) {
        updated = false;
        if (curr.side) {
            if (weapons.Count > 1) {
                if (Mathf.Abs(Input.mouseScrollDelta.y) >= 0.1) { // change weapon on mouse scroll
                    currWeapon += (Input.mouseScrollDelta.y < 0) ? -1 : 1;
                    currWeapon = Wrap(currWeapon, 0, instWeapons.Count - 1);
                    updated = true;
                }

                for (int i = 0; i < 8; i++) { // change weapon on number press
                    if (Input.GetKeyDown((i + 1).ToString()) && i < instWeapons.Count) {
                        currWeapon = i;
                        updated = true;
                    }
                }
            }

            if (currWeapon == -1) {
                currWeapon = 0;
                updated = true; 
            }
        
            curr = instWeapons[currWeapon];
            if (curr.side) UpdateAmmoText(curr);
            if (updated && weapons.Count > 1) {
                fireCD = 0f;
            
                for (int i = 0; i < instWeapons.Count; i++) {
                    instWeapons[i].gameObject.SetActive(false);
                    UpdateWeaponSlots(currWeapon);
                }
            
                curr.gameObject.SetActive(true);
            }
        }
    }
}