using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PointList {
    public List<Vector2> points;

    public PointList(List<Vector2> pts) {
        points = pts;
    }

    public PointList() {
        points = new List<Vector2>();
    }

    public List<Vector2> ToVector2List() {
        return points;
    }
    
    public int Count => points.Count;

    public Vector2 this[int indx] {
        get => points[indx];
        set => points[indx] = value;
    }

    // public static implicit operator PointList(List<Vector2> pts) {
    //     PointList ptList = new PointList();
    //     ptList.points = pts;
    //     return ptList;
    // }
    
    public static explicit operator PointList(List<Vector2> pts) {
        PointList ptList = new PointList();
        ptList.points = pts;
        return ptList;
    }
}