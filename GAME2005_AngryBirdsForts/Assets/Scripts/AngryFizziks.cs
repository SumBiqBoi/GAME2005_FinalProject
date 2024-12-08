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
            Vector3 prevPos = shape.transform.position;

            Vector3 Fg = GetGravityForce(shape);
            shape.netForce += Fg;

            if (!shape.isStatic)
            {
                Vector3 accelerationThisFrame = shape.netForce / shape.mass;

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
                if (shapeA.shapeTypes.GetShape() == AngrySquare.Shape.Square && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Square)
                {
                    collisionInfo = CollideSquares((AngrySquare)shapeA.shapeTypes, (AngrySquare)shapeB.shapeTypes);
                }
                if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Sphere && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Square)
                {
                    collisionInfo = CollideSphereSquare((AngrySphere)shapeA.shapeTypes, (AngrySquare)shapeB.shapeTypes);
                }
                else if (shapeA.shapeTypes.GetShape() == AngrySphere.Shape.Square && shapeB.shapeTypes.GetShape() == AngrySquare.Shape.Sphere)
                {
                    correctedShapeA = shapeB;
                    correctedShapeB = shapeA;
                    collisionInfo = CollideSphereSquare((AngrySphere)shapeB.shapeTypes, (AngrySquare)shapeA.shapeTypes);
                }

                if (collisionInfo.didCollide)
                {
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
        Vector3 displacementAToB = squareB.transform.position - squareA.transform.position;

        Vector2 halfExtentTotal = squareA.HalfExtent() + squareB.HalfExtent();

        if (Mathf.Abs(displacementAToB.x) > halfExtentTotal.x || Mathf.Abs(displacementAToB.y)  > halfExtentTotal.y)
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        float widthTotal = (squareA.width + squareB.width) / 2;
        float heightTotal = (squareA.height + squareB.height) / 2;

        Vector3 collisionNormalBToA;

        Vector3 mtv;

        float collisionNormalBToAX;
        float collisionNormalBToAY;

        if (Mathf.Abs(displacementAToB.x) > Mathf.Abs(displacementAToB.y))
        {
            if (displacementAToB.x < 0)
            {
                collisionNormalBToAX = displacementAToB.x + widthTotal;
            }
            else
            {
                collisionNormalBToAX = displacementAToB.x - widthTotal;
            }

            mtv = new Vector3(collisionNormalBToAX, 0, 0);
        }
        else
        {
            if (displacementAToB.y < 0)
            {
                collisionNormalBToAY = displacementAToB.y + heightTotal;
            }
            else
            {
                collisionNormalBToAY = displacementAToB.y - heightTotal;
            }

            mtv = new Vector3(0, collisionNormalBToAY, 0);   
        }
        collisionNormalBToA = mtv;

        squareA.transform.position += mtv * 0.5f;
        squareB.transform.position -= mtv * 0.5f;

        return new CollisionInfo(true, collisionNormalBToA);
    }

    public CollisionInfo CollideSphereSquare(AngrySphere sphere, AngrySquare square)
    {
        float clampedPointX = Mathf.Clamp(sphere.transform.position.x, square.transform.position.x - square.HalfExtent().x, square.transform.position.x + square.HalfExtent().x);
        float clampedPointY = Mathf.Clamp(sphere.transform.position.y, square.transform.position.y - square.HalfExtent().y, square.transform.position.y + square.HalfExtent().y);

        Vector3 squareToSphere = new Vector3(clampedPointX - sphere.transform.position.x, clampedPointY - sphere.transform.position.y, 0);
        Vector3 sphereRadiusSquareExtentTotal = new Vector3(sphere.radius + square.HalfExtent().x, sphere.radius + square.HalfExtent().y, 0);

        if (squareToSphere.x > sphereRadiusSquareExtentTotal.x || squareToSphere.y > sphereRadiusSquareExtentTotal.y)
        {
            return new CollisionInfo(false, Vector3.zero);
        }

        Vector3 mtv;

        float overlapX = clampedPointX - sphere.transform.position.x;
        float overlapY = clampedPointY - sphere.transform.position.y;

        if (overlapX > overlapY)
        {
            mtv = new Vector3(overlapX, 0, 0);
        }
        else
        {
            mtv = new Vector3(0, overlapY, 0);
        }

        sphere.transform.position += mtv;
        square.transform.position -= mtv;

        return new CollisionInfo(true, mtv);
    }
}
