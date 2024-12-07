using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngryLauncher : MonoBehaviour
{
    [SerializeField] GameObject angryShape;

    public float launchSpeed = 1;
    public Vector3 startPosition;

    void Start()
    {
        startPosition = new Vector3(transform.position.x, transform.position.y + transform.localScale.y, transform.position.z);
        angryShape.transform.position = startPosition;
    }

    void Update()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0.0f;

        Vector3 launchShape = angryShape.transform.position - mouse;
        Debug.DrawLine(mouse, angryShape.transform.position, Color.cyan);

        if (Input.GetMouseButtonDown(0))
        {
            AngryShapes shapes = Instantiate(angryShape).GetComponent<AngryShapes>();
            shapes.transform.position = angryShape.transform.position;
            shapes.velocity = launchShape * launchSpeed;
        }
    }
}
