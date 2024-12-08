using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngrySquare : AngryShapeTypes
{
    public float width = 1;
    public float height = 1;
    public float depth = 1;

    public Vector2 halfExtent;


    public override Shape GetShape()
    {
        return Shape.Square;
    }

    public void UpdateScale()
    {
        transform.localScale = new Vector3(width, height, depth);
    }

    public void OnValidate()
    {
        UpdateScale();
    }

    private void Update()
    {
        UpdateScale();
    }
    public Vector2 HalfExtent()
    {
        return halfExtent = new Vector2(width / 2, height / 2);
    }
}
