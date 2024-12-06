using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AngryShapeTypes : MonoBehaviour
{
    public enum Shape
    {
        Sphere,
        Plane,
        Halfspace
    }
    public abstract Shape GetShape();
}