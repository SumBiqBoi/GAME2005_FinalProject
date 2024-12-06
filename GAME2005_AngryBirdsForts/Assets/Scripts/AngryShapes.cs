using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AngryShapes : MonoBehaviour
{
    public AngryShapeTypes shapeTypes = null;

    public float mass = 1;
    public float gravityScale = 0;
    public float grippyness = 0.5f;

    public Vector3 velocity = Vector3.zero;
    public Vector3 netForce = Vector3.zero;

    public bool isStatic = false;
    void Start()
    {
        shapeTypes = GetComponent<AngryShapeTypes>();
        AngryFizziks.Instance.angryShapesList.Add(this);
    }
}
