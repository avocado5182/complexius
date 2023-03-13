using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using org.matheval;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance;
    void Awake() {
        Instance = this;
    }

    public int currentLevel;
    public int lastLevel = 8;
    
    [Header("Functions")]
    // string is function, Vector2 is bounds (minX, maxX)
    public List<string> playerFns;
    public List<string> playerFnNames;
    public List<Vector2> playerFnBounds;
    [Space]
    public List<string> enemyFns;
    public List<string> enemyFnNames;
    public List<Vector2> enemyFnBounds;
    
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
