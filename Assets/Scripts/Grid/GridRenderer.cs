using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using org.matheval;
using UnityEngine;
using UnityEngine.Serialization;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GridRenderer : MonoBehaviour {
    public static GridRenderer Instance;
    void Awake() {
        if (Instance == null) Instance = this;
    }
    

    public Camera cam;

    public Transform gridLineContainer;
    public Transform gridPointContainer;

    public Transform generatedLineContainer;
    
    // width/height dominance, if height > width, height dominant else width dominant
    // if height dominant, use gridRows and make it variable gridCols,
    //          otherwise, use gridCols and make it variable gridRows
    public int gridRows;
    public int gridCols;
    public float gridLineThickness = 0.05f; // world units
    public float ptThickness = 2f;

    public Color gridBgColor   = Color.HSVToRGB(0f, 0f, 0.9f);
    public Color gridLineColor = Color.HSVToRGB(0f, 0f, 0.8f);
    public Color gridPointColor = Color.blue;
    public Color gridAxisColor = Color.black;
    public Color gridOriginColor = Color.black;
    
    public Sprite gridLineSprite;
    public Material gridSpriteMat;
    [HideInInspector] public float lineDist = 0f;

    public Sprite gridPtSprite;
    
    int lastScreenWidth;
    int lastScreenHeight;

    public float screenHeight = 0f;
    public float screenWidth = 0f;

    public bool renderAllPoints = false;
    public bool renderFunctionEndpoints = true;
    public bool renderOrigin = false;
    public bool drawGridLines = true;
    public bool colorAxes = true;
    
    [HideInInspector] public bool hasRendered = false;

    // Start is called before the first frame update
    void Start() {
        if (cam == null) cam = Camera.main;

        if (gridLineContainer.childCount > 0) {
            foreach (Transform child in gridLineContainer) {
                Destroy(child.gameObject);
            }
        }
        
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        cam.backgroundColor = gridBgColor;
        RenderGridLines();
        if (renderAllPoints) RenderGridPoints();
        
        // ok now do everything that requires points
        if (colorAxes) RenderAxes();
        if (renderOrigin) RenderOrigin();
        if (renderFunctionEndpoints) RenderFunctionEndpoints();
        
        hasRendered = true;
    }
    
    #region everything that requires points

    void RenderFunctionEndpoints() {
        for (int i = 0; i < LevelManager.Instance.playerFns.Count; i++) {
            string currFn = LevelManager.Instance.playerFns[i];
            string currFnName = LevelManager.Instance.playerFnNames[i];
                
            Vector2 currFnBounds = LevelManager.Instance.playerFnBounds[i];
                
            List<Vector2> complexPts = GridManager.Instance.DrawFunction(
                currFnBounds.x, // (min X) 
                currFnBounds.y, // (max X)
                args => {
                    string function = currFn;
                    Expression expr = new Expression(function);
                    float x = args[0];

                    return (float)expr.Bind("x", x).Eval<decimal>();
                }, 
                new [] { 0f }, 
                Color.white,
                currFnName,
                gridLineThickness,
                false
            );

            PointList complexPtList = new PointList(complexPts);
            PointList worldSpacePtList = complexPtList;
            worldSpacePtList.points = worldSpacePtList.points.Select(
                v => 
                    GridManager.Instance.WorldSpacePoint(v) 
                    // GridManager.gridPoints[new Vector2(0, 0)]
                ).ToList();

            GameObject firstPtObj = DrawPoint(
                worldSpacePtList[0],
                Vector2.one * gridLineThickness * ptThickness,
                Color.white,
                true);

            float step = (currFnBounds.y - currFnBounds.x) / GridManager.Instance.precision;
            // float lastX = worldSpacePtList[worldSpacePtList.Count - 1].x;
            float lastX = (GridManager.Instance.precision * step) + currFnBounds.x;
            Vector2 lastPos = new Vector2(lastX, (float) (new Expression(currFn)).Bind("x", lastX).Eval<decimal>());
            
            // Vector2 lastPt = new Vector2(lastXPos, );
            Vector2 lastPt = GridManager.Instance.WorldSpacePoint(lastPos);
            GameObject lastPtObj = DrawPoint(
                lastPt,
                Vector2.one * gridLineThickness * ptThickness,
                Color.white,
                true);

            BoxCollider2D firstPtCollider = firstPtObj.AddComponent<BoxCollider2D>();
            BoxCollider2D lastPtCollider = lastPtObj.AddComponent<BoxCollider2D>();
        }
        
        for (int i = 0; i < LevelManager.Instance.enemyFns.Count; i++) {
            string currFn = LevelManager.Instance.enemyFns[i];
            string currFnName = LevelManager.Instance.enemyFnNames[i];
                
            Vector2 currFnBounds = LevelManager.Instance.enemyFnBounds[i];
            
            List<Vector2> complexPts = GridManager.Instance.DrawFunction(
                currFnBounds.x, // (min X) 
                currFnBounds.y, // (max X)
                args => {
                    string function = currFn;
                    Expression expr = new Expression(function);
                    float x = args[0];

                    return (float)expr.Bind("x", x).Eval<decimal>();
                }, 
                new [] { 0f }, 
                Color.white,
                currFnName,
                gridLineThickness,
                false
            );

            PointList complexPtList = new PointList(complexPts);
            PointList worldSpacePtList = complexPtList;
            worldSpacePtList.points = worldSpacePtList.points.Select(
                v => 
                    GridManager.Instance.WorldSpacePoint(v) 
                    // GridManager.gridPoints[new Vector2(0, 0)]
                ).ToList();

            GameObject firstPtObj = DrawPoint(
                worldSpacePtList[0],
                Vector2.one * gridLineThickness * ptThickness,
                Color.white,
                true);

            float step = (currFnBounds.y - currFnBounds.x) / GridManager.Instance.precision;
            // float lastX = worldSpacePtList[worldSpacePtList.Count - 1].x;
            float lastX = (GridManager.Instance.precision * step) + currFnBounds.x;
            Vector2 lastPos = new Vector2(lastX, (float) (new Expression(currFn)).Bind("x", lastX).Eval<decimal>());
            
            // Vector2 lastPt = new Vector2(lastXPos, );
            Vector2 lastPt = GridManager.Instance.WorldSpacePoint(lastPos);
            GameObject lastPtObj = DrawPoint(
                lastPt,
                Vector2.one * gridLineThickness * ptThickness,
                Color.white,
                true);

            BoxCollider2D firstPtCollider = firstPtObj.AddComponent<BoxCollider2D>();
            BoxCollider2D lastPtCollider = lastPtObj.AddComponent<BoxCollider2D>();
        }
    }
    
    void RenderOrigin() {
        Vector2 originWSPos = GridManager.gridPoints[new Vector2(0, 0)];
        DrawPoint(originWSPos, Vector2.one * gridLineThickness * 2f, gridOriginColor, false);
    }

    void RenderAxes() {
        Vector2 originWSPos = GridManager.gridPoints[new Vector2(0, 0)];

        float x1 = -screenWidth / 2f;
        float x2 = -x1;
        float y1 = originWSPos.y;

        float x3 = originWSPos.x;
        float y2 = -screenHeight / 2f;
        float y3 = -y2;
        
        DrawLineBetweenTwoPoints(new Vector2(x1, y1), new Vector2(x2, y1), gridAxisColor, gridLineThickness, false, "Axis Line");
        DrawLineBetweenTwoPoints(new Vector2(x3, y2), new Vector2(x3, y3), gridAxisColor, gridLineThickness, false, "Axis Line");
    }

    // (world space points)
    public GameObject DrawLineBetweenTwoPoints(
        Vector2 wsp1, 
        Vector2 wsp2, 
        Color lineColor, 
        float lineThickness,
        bool addCollider = false, 
        string nameOfLine = "New Line",
        string lineTag = "Line") {
        GameObject lineObj = new GameObject(nameOfLine);
        lineObj.transform.SetParent(generatedLineContainer);

        // add tag to obj
        lineObj.tag = lineTag;
        
        // scale the line
        float dist = Vector2.Distance(wsp1, wsp2);
        lineObj.transform.localScale = new Vector2(dist, lineThickness);

        // add collider
        if (addCollider) {
            BoxCollider2D collider = lineObj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            // collider.size = lineObj.transform.localScale;
            collider.size = Vector2.one;
            collider.offset = Vector2.zero;
        }
        
        // position the line
        float MiddleOf2Floats(float f1, float f2) {
            float min = Mathf.Min(f1, f2);
            float max = Mathf.Max(f1, f2);
            float difference = Mathf.Abs(min - max);
            
            float mid = ((difference / 2f) + min);
            return mid;
        }

        Vector2 midpoint = new Vector2(
            MiddleOf2Floats(wsp1.x, wsp2.x), 
            MiddleOf2Floats(wsp1.y, wsp2.y));
        
        lineObj.transform.position = new Vector3(
            midpoint.x,
            midpoint.y,
            -1f);
        
        // rotate the line
        // float acos = Mathf.Acos((wsp2.x - wsp1.x) / dist);
        float acos = Mathf.Acos((wsp2.x - wsp1.x) / dist);
        float angle = acos * ((wsp1.y >= wsp2.y) ? -1f : 1f) * Mathf.Rad2Deg; 
        lineObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // add sprite renderer to the line (p.m. just for color)
        SpriteRenderer sr = lineObj.AddComponent<SpriteRenderer>();
        sr.sprite = gridLineSprite;
        sr.material = gridSpriteMat;
                             // (this v being the default)
        sr.color = lineColor == Color.clear ? gridPointColor : lineColor;

        return lineObj;
    }

    public GameObject DrawPoint(Vector2 worldPos, Vector2 size, Color color, bool tooltip = false) {
        if (color == Color.clear) color = gridPointColor;
        bool useComplexPoint = true;
        Vector2 complexPoint = new Vector2();
        // if (useComplexPoint) complexPoint = GridManager.gridPoints.FirstOrDefault(pt => pt.Value == worldPos).Key;
        if (useComplexPoint) complexPoint = GridManager.Instance.ComplexSpacePoint(worldPos);

        GameObject gridPtObj = new GameObject((useComplexPoint) ? $"{complexPoint.x} + {complexPoint.y}i" : "New Point");
        gridPtObj.transform.SetParent(gridPointContainer);
        gridPtObj.transform.position = new Vector3(worldPos.x, worldPos.y, -2);
        gridPtObj.transform.localScale = size;

        GridPoint gridPt = gridPtObj.AddComponent<GridPoint>();
        gridPt.useTooltip = tooltip;
        gridPt.position = complexPoint;
            
        SpriteRenderer sr = gridPtObj.AddComponent<SpriteRenderer>();
        sr.sprite = gridPtSprite;
        sr.material = gridSpriteMat;
        sr.color = color;

        BoxCollider2D ptCollider = gridPtObj.AddComponent<BoxCollider2D>();
        ptCollider.size = Vector2.one * 4;
        
        return gridPtObj;
    }
    
    public void RenderGridPoints() { // just to test functionality
        foreach (var gridPoint in GridManager.gridPoints) {
            DrawPoint(
                gridPoint.Value, 
                Vector2.one * gridLineThickness * 2, 
                (gridPoint.Key.x == 0 || gridPoint.Key.y == 0) ? Color.red : gridPointColor);
        }
    }
    
    #endregion
    
    public void RenderGridLines() {
        // we could use either Screen.width or lastScreenWidth but screen.width probably easier to read

        Vector3 topRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        screenWidth = topRight.x * 2;
        screenHeight = topRight.y * 2;

        float minScreenDim = Mathf.Min(screenWidth, screenHeight);
        float maxScreenDim = Mathf.Min(screenHeight, screenWidth);

        Debug.Log(new Vector2(screenWidth, screenHeight));

        if (Screen.height > Screen.width) { // height dominant, use ~~gridRows~~ gridCols?????
            // lineDist = screenHeight / gridRows;
            // gridCols = Mathf.CeilToInt(screenWidth / lineDist);
            lineDist = screenWidth / (gridCols - 1);
            gridRows = Mathf.FloorToInt(screenHeight / lineDist);
            if (gridRows % 2 == 0) gridRows += 1; // make it odd
        }
        else { // width dominant, use gridCols
            lineDist = screenWidth / (gridCols - 1);
            gridRows = Mathf.FloorToInt(screenHeight / lineDist);
            if (gridRows % 2 == 0) gridRows += 1; // make it odd
        }
        //
        // Debug.Log("--");
        Debug.Log(lineDist);
        Debug.Log(new Vector2(gridCols, gridRows));

        for (int i = 0; i < gridCols; i++) {
            for (int j = 0; j < gridRows; j++) {
                // if (colorAxes && (j == (gridRows + 1) / 2 || i == (gridCols - 1) / 2)) continue;
                int gridIndx = i * gridCols + j;
                
                Vector2 scale = new Vector2(
                    (j == 0) ? gridLineThickness : lineDist,
                    (j == 0) ?      screenHeight : gridLineThickness 
                );
                
                Vector2 pos = new Vector2(
                    (lineDist * i - (screenWidth / 2f)) + ((j == 0) ? 0 : lineDist / 2f),
                    (j == 0) ? 0 : lineDist * (j - 1) - (screenHeight / 2f)
                );

                if (drawGridLines) {
                    GameObject gridLine = MakeGridLine(
                        pos, 
                        scale, 
                        new Vector2(screenWidth, screenHeight), 
                        gridIndx
                    );
                }
                
                // add point to gridPoints
                AddPointToGridPoints(new Vector2(pos.x - (lineDist / 2), pos.y), i, j, screenWidth, screenHeight);
                
                // add missing line from top (bc j == 0 lines are vertical)
                if (gridIndx == gridRows - 1 && drawGridLines) {
                    Vector2 topLinePos = new Vector2(0, lineDist * (gridRows - 1) - screenHeight / 2);
                    Vector2 topLineScale = new Vector2(screenWidth, gridLineThickness);
                    GameObject topLine = MakeGridLine(
                        topLinePos, 
                        topLineScale, 
                        new Vector2(screenWidth, screenHeight), 
                        gridIndx + 1
                    );
                }
            }
        }
    }

    void AddPointToGridPoints(Vector2 pos, int i, int j, float screenWidth, float screenHeight) {
        // make sure to have all levels w/ odd gridRows or gridCols (code makes the other odd)
        // so that there can be 2 middle axes for the origin (0 + 0i)

        // if j == 0, effective worldspace point will be (lineDist * i, screenHeight / 2)
        // compared to (lineDist * i, lineDist * (j - 1) - screenHeight / 2) for j != 0
        
        // indx will be the function's i to avoid confusion
        // u = indx
        // v = (jndx == 0) ? (gridRows - 1) / 2 : jndx - 1 - ((gridRows - 1) / 2) 
        // sign = (jndx == 0) ? 1 : -1
        // v = ((jndx == 0) ? 0 : jndx - 1) - (sign * ((gridRows - 1) / 2))
        // ^ * i
        
        // int sign = (j == 0) ? 1 : -1;

        float u = i - ((gridCols - 1) / 2);
        // float v = ((j == 0) ? gridRows - 1 : j - 1) + (sign * ((gridRows - 1) / 2));
        float v = j - 1 - ((gridRows - 1) / 2f);

        if (j == 0) {
            v = (gridRows - 1) / 2f;
            pos.x += lineDist / 2f;
            // pos.y = screenHeight / 2f - ((gridRows % 2 == 0) ? 0 : lineDist / 2f);
            pos.y = -(screenHeight / 2) + (lineDist * (gridRows - 1));
        }
        
        Vector2 cpp = new Vector2(u, v);
        GridManager.gridPoints.Add(cpp, pos);
    }
    
    GameObject MakeGridLine(Vector2 pos, Vector2 scale, Vector2 screenSize, int gridIndx = 0) {
        float screenWidth = screenSize.x;
        float screenHeight = screenSize.y;
        GameObject gridLine = new GameObject($"gridline {gridIndx}");
        gridLine.transform.SetParent(gridLineContainer);

        gridLine.transform.localScale = scale;
        gridLine.transform.position = pos;
        
        SpriteRenderer sr = gridLine.AddComponent<SpriteRenderer>();
        sr.sprite = gridLineSprite;
        sr.material = gridSpriteMat;
        
        // // issue with this is lines are done before points :/
        // Vector2 complexPoint = GridManager.gridPoints.First(pt => pt.Value == pos).Key;
        // Debug.Log(complexPoint);
        // sr.color = (complexPoint.x == 0 || complexPoint.y == 0) ? gridAxisColor : gridLineColor;
        sr.color = gridLineColor;

        return gridLine;
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight) {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            RenderGridLines(); // rerender on resolution changes
        }
    }
}
