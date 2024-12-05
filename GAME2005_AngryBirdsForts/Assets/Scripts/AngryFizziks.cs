using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryFizziks : MonoBehaviour
{
    public struct CollisionInfo
    {
        public bool didCollide;
        public Vector3 normal;

        public CollisionInfo(bool didCollide, Vector3 normal)
        {
            this.didCollide = didCollide;
            this.normal = normal;
        }
    }

    static AngryFizziks instance = null;
    public static AngryFizziks Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AngryFizziks>();
            }
            return instance;
        }
        
    }

    public List<AngryShapes> angryShapesList = new List<AngryShapes>();
    public float dT = 0.02f;

    public Vector3 gravityAcceleration = new Vector3(0, -10, 0);

    AngryShapes correctedShapeA;
    AngryShapes correctedShapeB;

    void FixedUpdate()
    {
        
    }

    public Vector3 GetGravityForce(AngryShapes shape)
    {
        return gravityAcceleration * shape.gravityScale * shape.mass;
    }

    public void KinematicShape()
    {
        foreach (AngryShapes shape in angryShapesList)
        {
            Vector3 prevPos = shape.transform.position;

            Vector3 Fg = GetGravityForce(shape);
            shape.netForce += Fg;

            Vector3 accelerationThisFrame = shape.netForce / shape.mass;

            shape.velocity += accelerationThisFrame * dT;
            Vector3 newPos = shape.transform.position + shape.velocity * dT;
            shape.transform.position = newPos;

            shape.netForce = Vector3.zero;
        }
    }

    public void CollisionShape()
    {
        for (int iA = 0; iA < angryShapesList.Count; iA++)
        {
            AngryShapes shapeA = angryShapesList[iA];
            
            for (int iB = 0; iB < angryShapesList.Count; iB++)
            {
                AngryShapes shapeB = angryShapesList[iB];

                correctedShapeA = shapeA;
                correctedShapeB = shapeB;

                if (shapeA == shapeB) continue;

                CollisionInfo collisionInfo = new CollisionInfo(false, Vector3.zero);

                if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Sphere && shapeB.shapeTypes.GetShape() == AngrySphere.Shape.Sphere)
                {
                    collisionInfo = CollideSpheres((AngrySphere)shapeA.shapeTypes, (AngrySphere)shapeB.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Sphere && shapeB.shapeTypes.GetShape() == AngrySphere.Shape.Plane)
                {
                    collisionInfo = CollideSpherePlane((AngrySphere)shapeA.shapeTypes, (AngryPlane)shapeB.shapeTypes);
                }
            }
        }
    }

    public CollisionInfo CollideSpheres(AngrySphere sphereA, AngrySphere sphereB)
    {
        Vector3 DisplacementBToA = sphereA.transform.position - sphereB.transform.position;
        float distance = DisplacementBToA.magnitude;
        float overlap = (sphereA.radius + sphereB.radius) / distance;

        if (overlap < 0)
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        Vector3 collisionNormalBToA;

        if (distance < 0.00001f)
        {
            collisionNormalBToA = Vector3.up;
        }
        else
        {
            collisionNormalBToA = (DisplacementBToA / distance);
        }

        Vector3 mtv = collisionNormalBToA * overlap;

        sphereA.transform.position += mtv * 0.5f;
        sphereB.transform.position -= mtv * 0.5f;

        return new CollisionInfo(true, collisionNormalBToA);
    }

    public CollisionInfo CollideSpherePlane(AngrySphere sphere, AngryPlane plane)
    {
        Vector3 planeToSphere = sphere.transform.position - plane.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSphere, plane.Normal());
        float distanceToPlane = Mathf.Abs(positionAlongNormal);

        float overlap = sphere.radius - positionAlongNormal;

        if (overlap < 0)
        {
            return new CollisionInfo(true, plane.Normal());
        }

        Vector3 mtv = plane.Normal() * overlap;
        sphere.transform.position += mtv;

        return new CollisionInfo(true, plane.Normal());
    }
}
