using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryLauncher : MonoBehaviour
{
    [SerializeField] GameObject angryShapeSphere;
    [SerializeField] GameObject angryShapeSquare;
    [SerializeField] GameObject angryShapeStaticSphere;
    [SerializeField] GameObject angryShapeStaticSquare;

    public float launchSpeed = 1;
    public Vector3 startPosition;

    private bool switchToSquare = false;

    void Start()
    {
        startPosition = new Vector3(transform.position.x, transform.position.y + transform.localScale.y, transform.position.z);
        angryShapeSphere.transform.position = startPosition;
    }

    void Update()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;

        Vector3 launchShape = angryShapeSphere.transform.position - mouse;
        Debug.DrawLine(mouse, angryShapeSphere.transform.position, Color.cyan);

        if (Input.GetMouseButtonDown(0))
        {
            if (!switchToSquare)
            {
                angryShapeStaticSphere.SetActive(true);
                angryShapeStaticSquare.SetActive(false);
                AngryShapes shapes = Instantiate(angryShapeSphere).GetComponent<AngryShapes>();
                shapes.transform.position = angryShapeSphere.transform.position;
                shapes.velocity = launchShape * launchSpeed;
            }
            if (switchToSquare)
            {
                angryShapeStaticSphere.SetActive(false);
                angryShapeStaticSquare.SetActive(true);
                AngryShapes shapes = Instantiate(angryShapeSquare).GetComponent<AngryShapes>();
                shapes.transform.position = angryShapeSphere.transform.position;
                shapes.velocity = launchShape * launchSpeed;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            switchToSquare = !switchToSquare;
            angryShapeStaticSphere.SetActive(!switchToSquare);
            angryShapeStaticSquare.SetActive(switchToSquare);
        }
        Debug.Log(switchToSquare);
    }
}
