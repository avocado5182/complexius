using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance;
    void Awake() {
        if (Instance == null) Instance = this;
    }

    [Header("Managers")]
    public GridRenderer gr;
    public GridManager gMgr;
    public WeaponManager wMgr;
    public Health health;

    [Header("GOs/Transforms")] 
    public Transform playerGFX;
    public AudioMixerGroup sfxMixer;
    public ParticleSystem onDeathParticles;
    
    [Header("Movement Variables")]
    public float movementSpeed = 2f;

    float currPt;
    int ptIndx;

    [Header("Other Variables")]
    [Tooltip("This value will be multiplied by the lineDist to get the player size.")]
    public float sizeMult = 1.5f;
    public bool canMove = true;
    public bool hasWon = false;
    Vector2 wsOrigin;
    public int playerFunction;
    public bool startInMiddle = false;
    public bool pausedGame = false;

    // percent: 0-1
    public Vector2 PercentBetweenVectors(Vector2 v1, Vector2 v2, float percent) {
        if (v1.x < 0 || v2.x < 0) {
            Vector2 diff = v1 - v2;
            return v2 + (percent * diff);
        }
        else {
            return Vector2.Lerp(v1, v2, percent);
        }
    }

    // "min vector" is one closer to the origin
    public Vector2 Min(Vector2 v1, Vector2 v2, Vector2 origin) {
        float d1 = Vector2.Distance(v1, origin);
        float d2 = Vector2.Distance(v2, origin);
        return (d2 >= d1) ? v1 : v2;
    }
    
    // "max vector" is one farther from the origin
    public Vector2 Max(Vector2 v1, Vector2 v2, Vector2 origin) {
        float d1 = Vector2.Distance(v1, origin);
        float d2 = Vector2.Distance(v2, origin);
        return (d1 >= d2) ? v1 : v2;
    }
    
    public int Mod(int k, int n) {
        return ((k %= n) < 0) ? k + n : k;
    }
    
    public float Mod(float k, float n) {
        return ((k %= n) < 0) ? k + n : k;
    }
    
    #region fade in keys

    // inOrOut: true = fade in, false = fade out
    void FadeKey(MaskableGraphic img, TMP_Text txt, out bool keyDown, bool inOrOut) {
        keyDown = inOrOut;
        UIManager uiMgr = UIManager.Instance;

        float startAlpha = (inOrOut) ? 1f : 0.5f;
        float endAlpha = (inOrOut) ? 0.5f : 1f;
        StartCoroutine(uiMgr.FadeGraphic(img, uiMgr.keyFadeTime, startAlpha, endAlpha));
        StartCoroutine(uiMgr.FadeGraphic(txt, uiMgr.keyFadeTime, startAlpha, endAlpha));
    }
    
    void FadeOutAKey() {
        // aKeyDown = false;
        UIManager uiMgr = UIManager.Instance;
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.aKey, uiMgr.keyFadeTime, 0.5f, 1f));
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.aKeyText, uiMgr.keyFadeTime, 0.5f, 1f));
        FadeKey(uiMgr.aKey, uiMgr.aKeyText, out aKeyDown, false);
    }
    
    void FadeInAKey() {
        // aKeyDown = true;
        UIManager uiMgr = UIManager.Instance;
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.aKey, uiMgr.keyFadeTime, 1f, 0.5f));
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.aKeyText, uiMgr.keyFadeTime, 1f, 0.5f));
        FadeKey(uiMgr.aKey, uiMgr.aKeyText, out aKeyDown, true);
    }
    
    void FadeOutDKey() {
        // dKeyDown = false;
        UIManager uiMgr = UIManager.Instance;
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.dKey, uiMgr.keyFadeTime, 0.5f, 1f));
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.dKeyText, uiMgr.keyFadeTime, 0.5f, 1f));
        FadeKey(uiMgr.dKey, uiMgr.dKeyText, out dKeyDown, false);
    }
    
    void FadeInDKey() {
        // dKeyDown = true;
        UIManager uiMgr = UIManager.Instance;
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.dKey, uiMgr.keyFadeTime, 1f, 0.5f));
        // StartCoroutine(uiMgr.FadeGraphic(uiMgr.dKeyText, uiMgr.keyFadeTime, 1f, 0.5f));
        FadeKey(uiMgr.dKey, uiMgr.dKeyText, out dKeyDown, true);
    }

    void FadeOutWKey() {
        UIManager uiMgr = UIManager.Instance;
        FadeKey(uiMgr.wKey, uiMgr.wKeyText, out wKeyDown, false);
    }
    
    void FadeInWKey() {
        UIManager uiMgr = UIManager.Instance;
        FadeKey(uiMgr.wKey, uiMgr.wKeyText, out wKeyDown, true);
    }
    
    void FadeOutSKey() {
        UIManager uiMgr = UIManager.Instance;
        FadeKey(uiMgr.sKey, uiMgr.sKeyText, out sKeyDown, false);
    }
    
    void FadeInSKey() {
        UIManager uiMgr = UIManager.Instance;
        FadeKey(uiMgr.sKey, uiMgr.sKeyText, out sKeyDown, true);
    }

    #endregion
    
    // void Destroy() {
    //     Destroy(gameObject);
    // }
    
    bool initedPosition;
    bool initedScale;
    bool readyToUpdate;
    
    bool aKeyDown;
    bool dKeyDown;
    bool wKeyDown;
    bool sKeyDown;

    bool vertical1Down;
    bool verticalneg1Down;
    

    [HideInInspector] public bool hasDied;
    // Update is called once per frame
    void Update() {
        if (hasDied) return;
        if (pausedGame) return;
        
        // initialization
        if (gMgr.hasDrawn && !initedPosition) {
            initedPosition = true;
            wsOrigin = GridManager.gridPoints[new Vector2(0, 0)];

            // world space
            List<Vector2> pts = gMgr.playerLines[playerFunction].ToVector2List();
            int startIndx = (startInMiddle) ? Mathf.FloorToInt(pts.Count / 2f) : 0;
            Vector2 startPt = pts[startIndx];
            ptIndx = startIndx;
            // transform.position = new Vector3(startPt.x, startPt.y, -1f);
            transform.position = new Vector3(startPt.x, startPt.y, -3f);
        } else if (gMgr.hasDrawn && initedPosition && !initedScale) {
            initedScale = true;
            readyToUpdate = true;

            transform.localScale = new Vector3(gr.lineDist * sizeMult, gr.lineDist * sizeMult, 1);
        }

        // on died
        if (health.IsDead() && !hasDied) {
            hasDied = true;
            
            // ParticleSystem onDeathPfxs = Instantiate(onDeathParticles, new Vector3(
            //     playerGFX.position.x,
            //     playerGFX.position.y
            //     -1.6f), Quaternion.identity);
            ParticleSystem onDeathPfxs = Instantiate(onDeathParticles, playerGFX);
            onDeathPfxs.Play();

            // Invoke(nameof(Destroy), onDeathPfxs.main.duration / onDeathPfxs.main.simulationSpeed);
            
            
            UIManager.Instance.ShowRetryScreen(true);
            
            // turn off movement
            canMove = false;
            
            // disable sprite renderer (of gfx) and collider
            GetComponent<Collider2D>().enabled = false;
            playerGFX.GetComponent<SpriteRenderer>().enabled = false;
            
            // destroy all weapons
            foreach (Weapon weapon in wMgr.instWeapons) {
                Destroy(weapon.gameObject);
            }

            return;
        }

        // rotating/positioning
        if (readyToUpdate && !hasWon) {
            // first lets do mouse rotation
            Vector2 worldMousePos = gr.cam.ScreenToWorldPoint(Input.mousePosition);
            // worldMousePos = new Vector2(worldMousePos.x - transform.position.x, worldMousePos.y - transform.position.y);
            // Transform currFirePt = wMgr.curr.firePt;
            Transform currFirePt = transform;
            float dist = Vector2.Distance(worldMousePos, currFirePt.position); 
            worldMousePos = new Vector2(
                worldMousePos.x - currFirePt.position.x, 
                worldMousePos.y - currFirePt.position.y);

            float angle = Mathf.Atan2(worldMousePos.x, worldMousePos.y) * Mathf.Rad2Deg;
            
            playerGFX.rotation = Quaternion.Euler(
                0, 
                0,
                ((dist <= gr.lineDist * sizeMult * 2) ? playerGFX.rotation.eulerAngles.z : -angle)
           );
            
            // then we can do player movement (:D)
            if (canMove && gMgr.hasDrawn) {
                float horInput = Input.GetAxisRaw("Horizontal");
                float verInput = Input.GetAxisRaw("Vertical");
                // Debug.Log(verInput);

                // UIManager uiMgr = UIManager.Instance;
                if (LevelManager.Instance.currentLevel == 1) {
                    if (horInput == 1f && !dKeyDown) {
                        // fade in d key
                        FadeInDKey();
                        // fade out a key
                        if (aKeyDown) FadeOutAKey(); 
                    } else if (horInput == -1f && !aKeyDown) {
                        // fade in a key
                        FadeInAKey();
                        // fade out d key
                        if (dKeyDown) FadeOutDKey(); 
                    } else if (horInput == 0f) {
                        // fade out both keys
                        if (aKeyDown) FadeOutAKey();
                        if (dKeyDown) FadeOutDKey();
                    }
                } else if (LevelManager.Instance.currentLevel == 5) {
                    if (verInput == 1f && !wKeyDown) {
                        // fade in w key
                        FadeInWKey();
                        // fade out s key
                        if (sKeyDown) FadeOutSKey(); 
                    } else if (verInput == -1f && !sKeyDown) {
                        // fade in s key
                        FadeInSKey();
                        // fade out w key
                        if (wKeyDown) FadeOutWKey(); 
                    }  else if (verInput == 0f) {
                        // fade out both keys
                        if (wKeyDown) FadeOutWKey();
                        if (sKeyDown) FadeOutSKey();
                    }
                }
                
                // get vertical input,
                // if 1  then playerFunction = (playerFunction + 1) % playerFns.Count
                // if -1 then playerFunction = (playerFunction - 1) % playerFns.Count
                
                // if (verInput == 1f && wKeyDown) {
                //     playerFunction = Mod(playerFunction + 1, LevelManager.Instance.playerFns.Count);
                // } else if (verInput == -1f && sKeyDown) {
                //     playerFunction = Mod(playerFunction - 1, LevelManager.Instance.playerFns.Count);
                // }

                if (verInput == 1f && !vertical1Down) {
                    vertical1Down = true;
                    verticalneg1Down = false;
                    playerFunction = Mod(playerFunction + 1, LevelManager.Instance.playerFns.Count);
                } else if (verInput == -1f && !verticalneg1Down) {
                    verticalneg1Down = true;
                    vertical1Down = false;
                    playerFunction = Mod(playerFunction - 1, LevelManager.Instance.playerFns.Count);
                } else if (verInput == 0) {
                    vertical1Down = false;
                    verticalneg1Down = false;
                }

                // List<Vector2> pts = gMgr.playerLinePoints;
                // Vector2 ptToUse;
                // ptToUse = pts[Mathf.Min(ptIndx + 1, pts.Count - 1)];
                //
                // Vector2 toMoveTo = PercentBetweenVectors(
                //     Min(transform.position, ptToUse, wsOrigin), 
                //     Max(transform.position, ptToUse, wsOrigin), currPt);
                // transform.position = new Vector3(toMoveTo.x, toMoveTo.y, -2);
                
                Vector2 pt;
                List<Vector2> pts = gMgr.playerLines[playerFunction].ToVector2List();
                pt = pts[Mathf.Min(ptIndx + 1, pts.Count - 1)];

                Vector2 toMoveTo = PercentBetweenVectors(
                    Min(transform.position, pt, wsOrigin), 
                    Max(transform.position, pt, wsOrigin), currPt);
                // transform.position = new Vector3(toMoveTo.x, toMoveTo.y, -2);
                transform.position = new Vector3(toMoveTo.x, toMoveTo.y, -3);

                // float oldCurrPt = currPt;
                currPt += (1 / gMgr.precision) * movementSpeed * Time.deltaTime * horInput;
                if (currPt >= 1) {
                    currPt = 0;
                    ptIndx++;
                }

                if (currPt < 0) {
                    currPt = 1;
                    ptIndx--;
                }

                if (ptIndx >= pts.Count - 1) ptIndx = pts.Count - 1;
                if (ptIndx <= 0) ptIndx = 0;
            }
        }
    }
}
