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
    public float ridiculouslySmallNumber = 0.00001f;

    public Vector3 gravityAcceleration = new Vector3(0f, -10f, 0f);

    AngryShapes correctedShapeA;
    AngryShapes correctedShapeB;

    void FixedUpdate()
    {
        CollisionShape();
        KinematicShape();
    }

    public Vector3 GetGravityForce(AngryShapes shape)
    {
        return gravityAcceleration * shape.gravityScale * shape.mass;
    }

    public void KinematicShape()
    {
        foreach (AngryShapes shape in angryShapesList)
        {
            Vector3 Fg = GetGravityForce(shape);
            shape.netForce += Fg;

            if (!shape.isStatic)
            {
                Vector3 accelerationThisFrame = shape.netForce / shape.mass;

                Vector3 momentum = shape.velocity * shape.mass;
                if (momentum.magnitude < 0.01f)
                {
                    shape.velocity = Vector3.zero;
                }

                shape.velocity += accelerationThisFrame * dT;
                Vector3 newPos = shape.transform.position + shape.velocity * dT;
                shape.transform.position = newPos;

                Debug.DrawRay(shape.transform.position, shape.velocity, Color.white);
                Debug.DrawRay(shape.transform.position, Fg, Color.magenta);
            }
            else
            {
                shape.velocity = Vector3.zero;
            }

            shape.netForce = Vector3.zero;
        }
    }

    public void CollisionShape()
    {
        for (int iA = 0; iA < angryShapesList.Count; iA++)
        {
            AngryShapes shapeA = angryShapesList[iA];
            
            for (int iB = iA + 1; iB < angryShapesList.Count; iB++)
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
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Plane && shapeB.shapeTypes.GetShape() == AngrySphere.Shape.Sphere)
                {
                    correctedShapeA = shapeB;
                    correctedShapeB = shapeA;
                    collisionInfo = CollideSpherePlane((AngrySphere)shapeB.shapeTypes, (AngryPlane)shapeA.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySquare.Shape.Square && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Square)
                {
                    collisionInfo = CollideSquares((AngrySquare)shapeA.shapeTypes, (AngrySquare)shapeB.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Sphere && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Square)
                {
                    collisionInfo = CollideSphereSquare((AngrySphere)shapeA.shapeTypes, (AngrySquare)shapeB.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Square && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Sphere)
                {
                    correctedShapeA = shapeB;
                    correctedShapeB = shapeA;
                    collisionInfo = CollideSphereSquare((AngrySphere)shapeB.shapeTypes, (AngrySquare)shapeA.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySquare.Shape.Square && shapeB.shapeTypes.GetShape() == AngrySphere.Shape.Plane)
                {
                    collisionInfo = CollideSquarePlane((AngrySquare)shapeA.shapeTypes, (AngryPlane)shapeB.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Plane && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Square)
                {
                    correctedShapeA = shapeB;
                    correctedShapeB = shapeA;
                    collisionInfo = CollideSquarePlane((AngrySquare)shapeB.shapeTypes, (AngryPlane)shapeA.shapeTypes);
                }
                if (collisionInfo.didCollide)
                {
                    if (shapeA.isPig)
                    {
                        CollidePigShape(shapeA, shapeB);
                    }

                    Vector3 Fg = GetGravityForce(correctedShapeA);

                    float gravityDotNormal = Vector3.Dot(Fg, collisionInfo.normal);
                    Vector3 gravityProjectedNormal = collisionInfo.normal * gravityDotNormal;
                    Vector3 Fn = -gravityProjectedNormal;

                    correctedShapeA.netForce += Fn;
                    correctedShapeB.netForce -= Fn;

                    // Friction
                    Vector3 velARelativeToB = correctedShapeB.velocity - correctedShapeA.velocity;
                    float velDotNormal = Vector3.Dot(velARelativeToB, collisionInfo.normal);
                    Vector3 velProjectedNormal = collisionInfo.normal * velDotNormal;

                    Vector3 velARelativeToBProjectedOntoPlane = velARelativeToB - velProjectedNormal;

                    if (velARelativeToBProjectedOntoPlane.sqrMagnitude > ridiculouslySmallNumber)
                    {
                        float coefficientOfFriction = Mathf.Clamp01(correctedShapeA.grippiness * correctedShapeB.grippiness);
                        float frictionMagnitude = Fn.magnitude * coefficientOfFriction;

                        Vector3 Ff = velARelativeToBProjectedOntoPlane.normalized * frictionMagnitude;

                        correctedShapeA.netForce += Ff;
                        correctedShapeB.netForce -= Ff;
                    }

                    // Bounciness
                    float velBRelativeToADotNormal = velDotNormal * -1;

                    if (velBRelativeToADotNormal < -1)
                    {
                        float restitution;

                        if (velBRelativeToADotNormal > -0.5f)
                        {
                            restitution = 0;
                        }
                        else
                        {
                            restitution = Mathf.Clamp01(correctedShapeA.bounciness * correctedShapeB.bounciness);
                        }

                        float deltaV1D = (1.0f + restitution) * velBRelativeToADotNormal;

                        float impulse1D = deltaV1D * correctedShapeA.mass * correctedShapeB.mass / (correctedShapeA.mass + correctedShapeB.mass);

                        Vector3 impulse3D = collisionInfo.normal * impulse1D;

                        correctedShapeA.velocity += -impulse3D / correctedShapeA.mass;
                        correctedShapeB.velocity +=  impulse3D / correctedShapeB.mass;
                    }
                }
            }
        }
    }

    public CollisionInfo CollideSpheres(AngrySphere sphereA, AngrySphere sphereB)
    {
        Vector3 DisplacementBToA = sphereA.transform.position - sphereB.transform.position;
        float distance = DisplacementBToA.magnitude;
        float overlap = (sphereA.radius + sphereB.radius) - distance;

        if (overlap < 0)
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        Vector3 collisionNormalBToA;

        if (distance < ridiculouslySmallNumber)
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

        if (plane.isHalfspace)
        {
            float overlapHalfspace = sphere.radius - positionAlongNormal;
            
            if (overlapHalfspace < 0)
            {
                return new CollisionInfo(false, Vector3.zero);
            }

            Vector3 mtvHalfspace = plane.Normal() * overlapHalfspace;
            sphere.transform.position += mtvHalfspace;
        }
        else
        {
            float overlap = sphere.radius - distanceToPlane;

            if (overlap < 0)
            {
                return new CollisionInfo(false, plane.Normal());
            }

            Vector3 mtv = plane.Normal() * overlap;
            sphere.transform.position += mtv;
        }
        return new CollisionInfo(true, plane.Normal());
    }

    public CollisionInfo CollideSquares(AngrySquare squareA, AngrySquare squareB)
    {
        float squareAMinX = squareA.transform.position.x - squareA.HalfExtent().x;
        float squareAMaxX = squareA.transform.position.x + squareA.HalfExtent().x;
        float squareAMinY = squareA.transform.position.y - squareA.HalfExtent().y;
        float squareAMaxY = squareA.transform.position.y + squareA.HalfExtent().y;

        float squareBMinX = squareB.transform.position.x - squareB.HalfExtent().x;
        float squareBMaxX = squareB.transform.position.x + squareB.HalfExtent().x;
        float squareBMinY = squareB.transform.position.y - squareB.HalfExtent().y;
        float squareBMaxY = squareB.transform.position.y + squareB.HalfExtent().y;

        
        if ((squareAMinX > squareBMaxX || squareAMaxX < squareBMinX) || (squareAMinY > squareBMaxY || squareAMaxY < squareBMinY))
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        float AMinXSubBMaxX = Mathf.Abs(squareAMinX - squareBMaxX);
        float AMaxXSubBMinX = Mathf.Abs(squareAMaxX - squareBMinX);
        float AMinYSubBMaxY = Mathf.Abs(squareAMinY - squareBMaxY);
        float AMaxYSubBMinY = Mathf.Abs(squareAMaxY - squareBMinY);

        float XResult = AMinXSubBMaxX < (squareA.HalfExtent().x + squareB.HalfExtent().x) ? squareBMaxX - squareAMinX : squareBMinX - squareAMaxX;
        float YResult = AMinYSubBMaxY < (squareA.HalfExtent().y + squareB.HalfExtent().y) ? squareBMaxY - squareAMinY : squareBMinY - squareAMaxY;

        Vector3 mtv = Mathf.Abs(XResult) < Mathf.Abs(YResult) ? new Vector3(XResult, 0, 0) : new Vector3(0, YResult, 0);

        squareA.transform.position += mtv;
        squareB.transform.position -= mtv;

        return new CollisionInfo(true, mtv);
    }

    public CollisionInfo CollideSphereSquare(AngrySphere sphere, AngrySquare square)
    {
        float clampedPointX = Mathf.Clamp(sphere.transform.position.x, square.transform.position.x - square.HalfExtent().x, square.transform.position.x + square.HalfExtent().x);
        float clampedPointY = Mathf.Clamp(sphere.transform.position.y, square.transform.position.y - square.HalfExtent().y, square.transform.position.y + square.HalfExtent().y);

        Vector3 sphereToClampedPointDistance = new Vector3(clampedPointX - sphere.transform.position.x, clampedPointY - sphere.transform.position.y, 0);
        Vector3 sphereToSquareDistance = square.transform.position - sphere.transform.position;
        Vector3 sphereRadiusSquareExtentTotal = new Vector3(sphere.radius + square.HalfExtent().x, sphere.radius + square.HalfExtent().y, 0);

        if (Mathf.Abs(sphereToSquareDistance.x) > sphereRadiusSquareExtentTotal.x || Mathf.Abs(sphereToSquareDistance.y) > sphereRadiusSquareExtentTotal.y)
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        Vector3 mtv;

        float overlapX;
        float overlapY;

        if (Mathf.Abs(sphereToSquareDistance.x) > Mathf.Abs(sphereToSquareDistance.y))
        {
            if (sphereToSquareDistance.x < 0)
            {
                overlapX = sphereToClampedPointDistance.x + sphere.radius;
            }
            else
            {
                overlapX = sphereToClampedPointDistance.x - sphere.radius;
            }

            mtv = new Vector3(overlapX, 0, 0);
        }
        else
        {
            if (sphereToSquareDistance.y < 0)
            {
                overlapY = sphereToClampedPointDistance.y + sphere.radius;
            }
            else
            {
                overlapY = sphereToClampedPointDistance.y - sphere.radius;
            }

            mtv = new Vector3(0, overlapY, 0);
        }

        sphere.transform.position += mtv * 0.5f;
        square.transform.position -= mtv * 0.5f;

        return new CollisionInfo(true, mtv);
    }

    public CollisionInfo CollideSquarePlane(AngrySquare square, AngryPlane plane)
    {
        Vector3 planeToSquare = square.transform.position - plane.transform.position;
        float positionAlongNormal = Vector3.Dot(planeToSquare, plane.Normal());
        float distanceToPlane = Mathf.Abs(positionAlongNormal);

        if (plane.isHalfspace)
        {
            float overlapHalfspace = square.height / 2 - positionAlongNormal;

            if (overlapHalfspace < 0)
            {
                return new CollisionInfo(false, Vector3.zero);
            }

            Vector3 mtvHalfspace = plane.Normal() * overlapHalfspace;
            square.transform.position += mtvHalfspace;
        }
        else
        {
            float overlap = square.height / 2 - distanceToPlane;

            if (overlap < 0)
            {
                return new CollisionInfo(false, plane.Normal());
            }

            Vector3 mtv = plane.Normal() * overlap;
            square.transform.position += mtv;
        }
        return new CollisionInfo(true, plane.Normal());
    }

    public void CollidePigShape(AngryShapes pig, AngryShapes shapes)
    {
        Vector3 pigMomentum = pig.velocity * pig.mass;
        Vector3 shapeMomentum = shapes.velocity * shapes.mass;

        if (pigMomentum.magnitude + shapeMomentum.magnitude >= pig.toughness)
        {
            AngryFizziks.Instance.angryShapesList.Remove(pig);
            Destroy(pig.gameObject);
        }
    }
}
