using System;using System.Collections;
using System.Collections.Generic;
using System.Linq;
using org.matheval;
using UnityEngine;
using Random = System.Random;

[Flags]
public enum EnemyAIProps {
    BackAndForth     = 0b_0000_0001,
    PeriodicShooting = 0b_0000_0010,
    ShootAtPlayer    = 0b_0000_0100,
    ShootRandomly    = 0b_0000_1000,
    SprayShoot       = 0b_0001_0000
}

public class EnemyAI : MonoBehaviour {
    [Header("GOs/Transforms")]
    [HideInInspector]
    public PlayerMovement pMvmt;
    [HideInInspector]
    public GridManager gMgr;
    [HideInInspector]
    public GridRenderer gr;

    public WeaponManager wMgr;
    public Transform enemyContainer;
    public Transform enemyGFX;
    public StandaloneWeapon boxWeapon;
    public ParticleSystem onDeathParticles;
    
    [Header("AI Properties")] 
    public List<Vector2> complexWaypoints;
    public List<Vector2> interpolatedComplexWaypoints;
    public List<Vector2> wsWaypoints = new List<Vector2>();
    public int enemyFunction;
    public EnemyAIProps props = EnemyAIProps.BackAndForth | EnemyAIProps.PeriodicShooting;

    float currPt;
    int ptIndx;

    public float timeBetweenFires = 1f;
    float tbf;
    int phases = 2;
    int phase = 0;

    [Header("Physical Properties")]
    public float movementSpeed;
    public float sizeMult = 1.5f;
    public bool canMove = true;
    public Health health;


    // Start is called before the first frame update
    void Start() {
        pMvmt = PlayerMovement.Instance;
        gMgr = GridManager.Instance;
        gr = GridRenderer.Instance;
    }

    float spray = 0f;
    int sprayInt = 0;
    bool sprayDirection = true;
    float sprayAngle = 160f; // degrees
    int sprayRate = 10; // number of bullets fired per sprayAngle degrees  
    bool canShootSpray = true;
    Quaternion Spray() {
        Vector3 refRotation = new Vector3();
        refRotation = enemyGFX.eulerAngles;
        refRotation = new Vector3(
            refRotation.x,
            refRotation.y,
            -(sprayAngle / 2 ) + sprayInt + 180f
        );

        if (pMvmt.Mod(sprayInt, sprayAngle / sprayRate) < 2f && canShootSpray) {
            canShootSpray = false;
            // Debug.Log(spray);
            wMgr.curr.Shoot();
            sprayInt = pMvmt.Mod(sprayInt + 1, Mathf.FloorToInt(sprayAngle));
        }

        // if (spray % sprayAngle < 1f) sprayDirection = !sprayDirection;
        
        return Quaternion.Euler(refRotation);
    }

    public List<Vector2> InitVect2Waypoints(List<Vector2> inputWpts, bool convertToWorldSpace) {
        List<Vector2> initWaypoints = new List<Vector2>();
        
        for (var i = 0; i < inputWpts.Count - 1; i++) {
            Vector2 wpt = inputWpts[i];
            Vector2 nextWpt = inputWpts[i + 1];
            // Debug.Log(wpt);
            // Debug.Log(nextWpt);

            List<Vector2> interpolated = gMgr.Interpolate2Endpoints( 
                wpt, 
                nextWpt , 
                (int)gMgr.precision);

            foreach (var pt in interpolated) {
                initWaypoints.Add(gMgr.WorldSpacePoint(pt));
            }
        }
    
        return initWaypoints;
    }

    void Destroy() {
        Destroy(gameObject);
    }

    Vector2 wsOrigin;
    bool initedPosition;
    bool initedScale;
    bool readyToUpdate;
    bool initedWpts;
    bool hadWpts;
    public bool useFunctionWithWaypoints;
    public bool isBoss;
    bool instantiatedOnDeathPfx;
    bool startedNext;
    
    public bool direction = true;
    // Update is called once per frame
    void Update() {
        if (pMvmt.pausedGame) return;
        
        if (gMgr.hasDrawn && !initedPosition) {
            initedPosition = true;
            wsOrigin = GridManager.gridPoints[new Vector2(0, 0)];

            // world space
            Vector2 startPt = gMgr.WorldSpacePoint(complexWaypoints[0]);
            // transform.position = new Vector3(startPt.x, startPt.y, -1f);
            transform.position = new Vector3(startPt.x, startPt.y, -3f);
        } else if (gMgr.hasDrawn && initedPosition && !initedScale) {
            initedScale = true;
            readyToUpdate = true;

            transform.localScale = new Vector3(gr.lineDist * sizeMult, gr.lineDist * sizeMult, 1);
        } else if (gMgr.hasDrawn && !initedWpts) {
            initedWpts = true;
            if (complexWaypoints.Count == 0) { // generate waypoints from enemylinepoints (i.e. given function)
                // get first point of given function
                Vector2 firstPt = gMgr.enemyLines[enemyFunction][0];
                
                // get last point of given function
                Vector2 lastPt = gMgr.enemyLines[enemyFunction][gMgr.enemyLines.Count - 1];

                // interpolate between from the given points
                complexWaypoints = InitVect2Waypoints(gMgr.enemyLines[enemyFunction].ToVector2List(), false);
            }
            else {
                hadWpts = true;
                
                // for (int i = 0; i < complexWaypoints.Count; i++) {
                    // convert each complex wpt to world space pos,
                    // add it to world space waypoints
                    // wsWaypoints.Add(gMgr.WorldSpacePoint(complexWaypoints[i]));
                // }
                wsWaypoints = InitVect2Waypoints(complexWaypoints, true);
                interpolatedComplexWaypoints = InitVect2Waypoints(complexWaypoints, false);
            }
        }

        if (health.IsDead() && !startedNext) {
            startedNext = true;
            // ParticleSystem onDeathPfxs = Instantiate(onDeathParticles, new Vector3(
            //     enemyGFX.position.x,
            //     enemyGFX.position.y
            //     -1.6f), Quaternion.identity);
            ParticleSystem onDeathPfxs = new ParticleSystem(); // "= new" so that i'm not yelled at 
            if (!instantiatedOnDeathPfx) {
                onDeathPfxs = Instantiate(onDeathParticles, enemyGFX);
                onDeathPfxs.Play();
            }

            int currLevel = LevelManager.Instance.currentLevel;
            if (currLevel != LevelManager.Instance.lastLevel) { // applies to all levels but last
                if (enemyContainer.childCount == 1) { // this is the last enemy
                    // show next level screen then destroy
                    UIManager.Instance.ShowNextLevelScreen(true);
                    PlayerMovement.Instance.hasWon = true;
                }
                
                Invoke(nameof(Destroy), onDeathPfxs.main.duration / onDeathPfxs.main.simulationSpeed);
            }


            if (currLevel == LevelManager.Instance.lastLevel) {
                if (enemyContainer.childCount == 1) {
                    // initiate endgame sequence.
                    Debug.Log("test");
                    pMvmt.canMove = false;
                    pMvmt.hasWon = true;
                    UIManager.Instance.ShowEndGameBox(true);
                    UIManager.Instance.endgameBoxAnimator.SetTrigger("comein");
                }
            }
        }

        if (readyToUpdate && gMgr.hasDrawn && !pMvmt.hasDied) {
            // boss phases
            // change props based on health 
            if (isBoss) {
                if (health.currHealth <= (health.maxHealth / phases) && phase == 0) {
                    // "change phase" (change props)
                    props &= ~EnemyAIProps.ShootAtPlayer;
                    props = props | EnemyAIProps.SprayShoot;
                }
            }
            
            // enemy rotation
            if (props.HasFlag(EnemyAIProps.ShootAtPlayer)) {
                Vector2 v1 = wMgr.curr.firePt.position;
                Vector2 v2 = pMvmt.playerGFX.position;

                Vector2 dir = v2 - v1;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                enemyGFX.rotation = Quaternion.Euler(
                    enemyGFX.rotation.eulerAngles.x,
                    enemyGFX.rotation.eulerAngles.y,
                    angle + 270f
                );
            } else if (isBoss && props.HasFlag(EnemyAIProps.SprayShoot)) {
                float rotationRate = 40f; // deg/sec
                if (sprayDirection) spray += (Time.deltaTime) * rotationRate;
                else spray -= (Time.deltaTime) * rotationRate;
                sprayInt = Mathf.FloorToInt(spray);
                
                enemyGFX.rotation = Spray();
                if (pMvmt.Mod(spray, sprayAngle / sprayRate) <= 1f) {
                    canShootSpray = true;
                    // spray %= sprayAngle; // if i used this i'd have to use special mod func that works w/ negatives
                    if ((sprayDirection) ? spray >= sprayAngle : spray <= 0) {
                        Debug.Log("hi there");
                        // spray = ((sprayDirection) ? 2 : sprayAngle - 2); // 2 and -2 prevent "backshooting"
                        sprayDirection = !sprayDirection;
                        Vector3 gfxEulers = enemyGFX.eulerAngles;
                        enemyGFX.rotation = Quaternion.Euler(
                            gfxEulers.x,
                            gfxEulers.y,
                            (((sprayDirection) ? 1f : -1f) * (sprayAngle / 2)) + 180f                
                        );
                    }
                
                    sprayInt = Mathf.FloorToInt(spray);
                    // spray = pMvmt.Mod(spray + 2f, sprayAngle);
                }
                sprayInt = Mathf.FloorToInt(spray);
            }
            
            // enemy movement
            if (canMove) {
                Vector2 ptToUse;
                List<Vector2> ptsToUse = gMgr.enemyLines[enemyFunction].ToVector2List();
                // ptsToUse = ptsToUse.Select(
                //     v =>
                //         GridManager.Instance.ComplexSpacePoint(v)
                // ).ToList();
                // if (!hadWpts) {
                // }
                if (hadWpts) {
                    ptsToUse = ptsToUse.FindAll(
                        v => v.x >= wsWaypoints[0].x && v.x <= wsWaypoints[wsWaypoints.Count-1].x);
                }
                
                // Debug.Log("pt 1");
                ptToUse = ptsToUse[Mathf.Min(ptIndx + 1, ptsToUse.Count - 1)];
                // if (!hadWpts) ptToUse = GridManager.Instance.WorldSpacePoint(ptToUse);
                // Debug.Log("pt 2");
                Vector2 toMoveTo = new Vector2();
                toMoveTo = pMvmt.PercentBetweenVectors(
                    pMvmt.Min(transform.position, ptToUse, wsOrigin), 
                    pMvmt.Max(transform.position, ptToUse, wsOrigin), currPt);

                // transform.position = new Vector3(toMoveTo.x, toMoveTo.y, -2);
                transform.position = new Vector3(toMoveTo.x, toMoveTo.y, -3);

                // float oldCurrPt = currPt;
                currPt += (1 / gMgr.precision) * movementSpeed * Time.deltaTime * ((direction) ? 1 : -1);
                if (currPt >= 1) {
                    currPt = 0;
                    ptIndx++;
                }

                if (currPt < 0) {
                    currPt = 1;
                    ptIndx--;
                }

                if (ptIndx >= ptsToUse.Count - 1) {
                    ptIndx = ptsToUse.Count - 1;
                    direction = false;
                }
                if (ptIndx <= 0) {
                    ptIndx = 0;
                    direction = true;
                }
            }

            // enemy shooting
            tbf += Time.deltaTime;
            if (tbf >= timeBetweenFires && !props.HasFlag(EnemyAIProps.SprayShoot)) {
                tbf = 0;
                
                // rotate?? (if shootRandomly)
                if (props.HasFlag(EnemyAIProps.ShootRandomly)) {
                    Quaternion rot = enemyGFX.transform.rotation; 
                    enemyGFX.transform.rotation = Quaternion.Euler(
                        rot.eulerAngles.x,
                        rot.eulerAngles.y,
                        (float)new Random().NextDouble() * 360 - 180);
                }
                
                wMgr.curr.Shoot();
            }
        }
    }
}
