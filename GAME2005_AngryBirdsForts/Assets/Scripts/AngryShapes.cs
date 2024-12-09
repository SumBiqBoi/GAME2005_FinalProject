using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AngryShapes : MonoBehaviour
{
    public AngryShapeTypes shapeTypes = null;

    public float mass = 1;
    public float gravityScale = 0;
    public float grippiness = 0.5f;
    public float bounciness = 0.5f;
    public float toughness = 100000;

    public Vector3 velocity = Vector3.zero;
    public Vector3 netForce = Vector3.zero;

    public bool isStatic = false;
    public bool isPig = false;
    void Start()
    {
        shapeTypes = GetComponent<AngryShapeTypes>();
        AngryFizziks.Instance.angryShapesList.Add(this);
    }
}
