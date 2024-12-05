using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryPlane : AngryShapeTypes
{
    public bool isHalfspace;
    public override Shape GetShape()
    {
        if (isHalfspace)
        {
            return Shape.Halfspace;
        }
        return Shape.Plane;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 Normal()
    {
        return transform.up;
    }
}
