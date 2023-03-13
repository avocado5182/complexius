using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using org.matheval;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class GridManager : MonoBehaviour {
    public static GridManager Instance;

    public GridRenderer gr;

    [Range(10, 100)] public float precision = 200f; 

    public Color playerLineColor;
    public Color enemyLineColor;

    // both WORLD SPACE
    // public List<Vector2> playerLinePoints;
    public List<PointList> playerLines;
    public List<PointList> enemyLines;
    // key will be complex plane point, value will be world space point
    // SO that defined complex functions later on will be easily convertable to grid 
    [HideInInspector] public static Dictionary<Vector2, Vector2> gridPoints;
    
    void Awake() {
        if (Instance == null) Instance = this;
        if (gr == null) gr = GetComponent<GridRenderer>();
        gridPoints = new Dictionary<Vector2, Vector2>();
    }

    // bounds being x values
    public Vector2[] EndpointsOfMxPlusBLine(float m, float b, float leftBound, float rightBound) {
        Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
        
        float ComplexToWorld = gr.lineDist;
        float worldLeftBound = leftBound * ComplexToWorld;
        float worldRightBound = rightBound * ComplexToWorld;

        float lineY = (b * ComplexToWorld);
        lineY += wsOrigin.y;
        
        Vector2 ep1 = new Vector2(worldLeftBound, (worldLeftBound * m) + lineY);
        Vector2 ep2 = new Vector2(worldRightBound, (worldRightBound * m) + lineY);

        return new[] { ep1, ep2 };
    }

    public Vector2 WorldSpacePoint(Vector2 point) {
        Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
        Vector2 retPoint = new Vector2();
        retPoint.x = point.x * gr.lineDist;
        retPoint.y = point.y * gr.lineDist;
        retPoint += wsOrigin;
        return retPoint;
    }
    
    public Vector2 ComplexSpacePoint(Vector2 point) {
        // Vector2 retPoint = new Vector2();
        // retPoint.x = point.x * gr.lineDist;
        // retPoint.y = point.y * gr.lineDist;
        // retPoint += wsOrigin;
        // return retPoint;
        Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
        Vector2 retPoint = point;
        retPoint -= wsOrigin;
        retPoint.y /= gr.lineDist;
        retPoint.x /= gr.lineDist;
        return retPoint;
    }

    public List<Vector2> Interpolate2Endpoints(Vector2 ep1, Vector2 ep2, int numOfPoints) {
        Vector2 diff = ep2 - ep1;
        List<Vector2> ret = new List<Vector2>();
        for (int i = 0; i < numOfPoints; i++) {
            ret.Add((diff * i / numOfPoints) + ep1);
        }

        return ret;
    }

    public bool hasDrawn;
    bool drawnDebounce;    
    // Update is called once per frame
    void Update()
    {
        if (gr.hasRendered && !drawnDebounce && !hasDrawn) {
            drawnDebounce = true;
            float lineThickness = 2f;
            
            for (int i = 0; i < LevelManager.Instance.playerFns.Count; i++) {
                string currFn = LevelManager.Instance.playerFns[i];
                string currFnName = LevelManager.Instance.playerFnNames[i];
                
                Vector2 currFnBounds = LevelManager.Instance.playerFnBounds[i];
                
                List<Vector2> complexPts = DrawFunction(
                    currFnBounds.x, // (min X) 
                    currFnBounds.y, // (max X)
                    args => {
                        string function = currFn;
                        Expression expr = new Expression(function);
                        float x = args[0];

                        return (float)expr.Bind("x", x).Eval<decimal>();
                    }, 
                    new [] { 0f }, 
                    playerLineColor,
                    currFnName,
                    gr.gridLineThickness * lineThickness
                );

                PointList complexPtList = new PointList(complexPts);
                playerLines.Add(complexPtList);
            
                for (int j = 0; j < complexPtList.Count; j++) {
                    Vector2 pt = complexPtList[j];
                    Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
                    Vector2 retPoint = new Vector2();
                    retPoint.x = pt.x * gr.lineDist;
                    retPoint.y = pt.y * gr.lineDist;
                    retPoint += wsOrigin;
                    pt = retPoint;
                    playerLines[i][j] = pt;
                }
            }
            
            // Vector2[] pEndpoints = EndpointsOfMxPlusBLine(0f, -4f, -12f, 12f);
            // // playerLinePoints = pEndpoints.ToList();
            // playerLines = Interpolate2Endpoints(pEndpoints[0], pEndpoints[pEndpoints.Length - 1], Mathf.FloorToInt(precision));
            // gr.DrawLineBetweenTwoPoints(
            //     pEndpoints[0], 
            //     pEndpoints[pEndpoints.Length - 1], playerLineColor, 
            //     gr.gridLineThickness * lineThickness, 
            //     true, 
            //     "Player Line",
            //     "PlayerLine");
            
            // float xMax = (gr.gridCols - 1) / 2f;
            // List<Vector2> complexPts = DrawFunction(
            //     -xMax, 
            //     xMax,
            //     args => {
            //         float x = args[0]; 
            //         return Mathf.Sin(x);
            //     }, 
            //     new [] { 0f }, 
            //     Color.magenta);
            
            // playerLinePoints = complexPts;

            // for (int i = 0; i < playerLinePoints.Count; i++) {
            //     Vector2 pt = playerLinePoints[i];
            //     Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
            //     Vector2 retPoint = new Vector2();
            //     retPoint.x = pt.x * gr.lineDist;
            //     retPoint.y = pt.y * gr.lineDist;
            //     retPoint += wsOrigin;
            //     pt = retPoint;
            //     playerLinePoints[i] = pt;
            // }
            
            for (int i = 0; i < LevelManager.Instance.enemyFns.Count; i++) {
                string currFn = LevelManager.Instance.enemyFns[i];
                string currFnName = LevelManager.Instance.enemyFnNames[i];
                
                Vector2 currFnBounds = LevelManager.Instance.enemyFnBounds[i];

                // float oldPrecision = precision; // workaround
                // precision = 2; // workaround
                List<Vector2> complexPts = DrawFunction(
                    currFnBounds.x, // (min X) 
                    currFnBounds.y, // (max X)
                    args => {
                        string function = currFn;
                        Expression expr = new Expression(function);
                        float x = args[0];

                        return (float)expr.Bind("x", x).Eval<decimal>();
                    }, 
                    new [] { 0f }, 
                    enemyLineColor,
                    currFnName,
                    gr.gridLineThickness * lineThickness
                );
                // precision = oldPrecision; // workaround

                PointList complexPtList = new PointList(complexPts);
                enemyLines.Add(complexPtList);
            
                for (int j = 0; j < complexPtList.Count; j++) {
                    Vector2 pt = complexPtList[j];
                    Vector2 wsOrigin = gridPoints[new Vector2(0, 0)];
                    Vector2 retPoint = new Vector2();
                    retPoint.x = pt.x * gr.lineDist;
                    retPoint.y = pt.y * gr.lineDist;
                    retPoint += wsOrigin;
                    pt = retPoint;
                    enemyLines[i][j] = pt;
                }
            }

            hasDrawn = true;
        }
    }

    public delegate float Function(float[] args);

    public List<Vector2> DrawFunction(float leftBound, float rightBound, Function func, float[] arguments, Color color, string functionName="New Function", float lineThickness=0, bool draw = true) {
        if (lineThickness == 0) lineThickness = gr.gridLineThickness;
        
        float[] args = arguments;
        List<Vector2> points = new List<Vector2>();
        
        Transform transformParent = new GameObject().transform;
        transformParent.gameObject.name = functionName;
        transformParent.SetParent(gr.generatedLineContainer);
        
        float width = rightBound - leftBound;
        float step = (width / precision);
        // add points to List<Vector2> points
        for (int i = 0; i < Mathf.RoundToInt(precision); i++) {
            Vector2 complexPoint = new Vector2();
            float x = leftBound + (step * i);
            args[0] = x;
            
            complexPoint.x = x;

            complexPoint.y = func(args);

            float rightX = (gr.gridCols - 1) / 2f;
            float leftX = rightX * -1f;
            float topY = (gr.gridRows - 1f) / 2f + 1f;
            float bottomY = topY * -1f - 1f;
            
            if (complexPoint.y >= topY || complexPoint.y <= bottomY ||
                complexPoint.x >= rightX || complexPoint.x <= leftX) continue;
            
            float lastX = x - step;
            if (i == 0 && x > -((gr.gridCols - 1) / 2f)) lastX = x + step;
            else if (i == 0) lastX = x;
            // if (i == 0) Debug.Log(lastX);
            // float lastY = (x == leftBound) ? Compute(x) : Compute(lastX);
            args[0] = lastX;
            float lastY = func(args);
            Vector2 lastPos = new Vector2(lastX, lastY);
            Vector2 worldLastPos = WorldSpacePoint(lastPos);
            Vector2 worldCurrPos = WorldSpacePoint(complexPoint);
            
            points.Add(complexPoint);

            if (draw) {
                gr.DrawLineBetweenTwoPoints(
                    worldCurrPos, 
                    worldLastPos, 
                    color, 
                    lineThickness
                ).transform.SetParent(transformParent);
                // Debug.Log($"i: {i}");
                args[0] = x + step;
                if (i == Mathf.RoundToInt(precision) - 1 || 
                    func(args) >= topY) { // last iteration, draw line from lastXPos to nextPos
                    float nextX = x + step;
                    args[0] = nextX;
                    float nextY = func(args);
                    Vector2 nextPos = new Vector2(nextX, nextY);
                    Vector2 worldNextPos = WorldSpacePoint(nextPos);
                    points.Add(nextPos);
                    
                    GameObject newLine = gr.DrawLineBetweenTwoPoints(
                        worldCurrPos, 
                        worldNextPos, 
                        color, 
                        lineThickness);
                    newLine.name = "bla bla bla";
                    newLine.transform.SetParent(transformParent);
                }
            }
        }
        
        return points;
    }
}
