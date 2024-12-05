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
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
