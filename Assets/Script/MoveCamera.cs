using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MoveCamera : MonoBehaviour {

    public float rotateSpeed = 3.5f;
    public Vector3 centroid;
    public float translateSpeed = 2f;
    public GameObject Canvas;
    public bool freeze;
    public float x, y;
    public float XYSpeed;
    public float ScrollSpeed;
    public Vector3 distance;
    public Vector3 ObjCentroid;
    public Vector3 CCDistance;
    Quaternion rotationEuler;

    private void Start()
    {
        centroid = new Vector3(0f,0f,0f);
        freeze = true;
        x = 0f;
        y = 0f;
        XYSpeed = 50;
        ScrollSpeed = 5000;
        distance = new Vector3(0,0,-500);
        rotationEuler = Quaternion.identity;
    }

    void Update ()
    {

       
        if (!Canvas.GetComponent<UserInterface>().Pause)
        {
            if (Input.GetKeyDown(KeyCode.F))
                freeze = !freeze;
            if (!freeze)
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                    translateSpeed *= 2f;
                if (Input.GetKeyUp(KeyCode.LeftShift))
                    translateSpeed /= 2f;

                if (Input.GetMouseButtonDown(1))
                {
                    CCDistance = centroid - transform.position;
                }
                if (Input.GetMouseButton(1))
                {
                    transform.Rotate(-Input.GetAxis("Mouse Y") * rotateSpeed, Input.GetAxis("Mouse X") * rotateSpeed, 0f);
                    transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
                    transform.Translate(Input.GetAxis("Horizontal") * translateSpeed, 0f, Input.GetAxis("Vertical") * translateSpeed, Space.Self);
                    centroid = GetComponent<Transform>().position + Vector3.Magnitude(CCDistance) * GetComponent<Transform>().forward;
                    
                }
               
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    x += Input.GetAxis("Mouse X") * XYSpeed * Time.deltaTime;
                    y -= Input.GetAxis("Mouse Y") * XYSpeed * Time.deltaTime;
                    if (x > 360)
                        x -= 360;
                    else if (x < 0)
                        x += 360;
                    rotationEuler = Quaternion.Euler(y, x, 0);
                    GetComponent<Transform>().position = rotationEuler * distance + centroid;
                    GetComponent<Transform>().LookAt(centroid);
                  
                }
                if (Input.GetAxis("Mouse ScrollWheel")!=0)
                {
                    distance.z -= Input.GetAxis("Mouse ScrollWheel") *ScrollSpeed* Time.deltaTime;
                   // Debug.Log(distance);
                }
                if (Input.GetMouseButtonDown(2))
                {
                    CCDistance = centroid - transform.position; 
                }
                if (Input.GetMouseButton(2))
                {
                    transform.Translate(-Input.GetAxis("Mouse X") * XYSpeed * Time.deltaTime, -Input.GetAxis("Mouse Y") * XYSpeed * Time.deltaTime, 0f, Space.Self);
                    centroid = transform.position + CCDistance;
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    centroid = ObjCentroid;
                    GetComponent<Transform>().position = rotationEuler * distance + centroid;
                }
                GetComponent<Transform>().position = rotationEuler * distance + centroid;
                GetComponent<Transform>().LookAt(centroid);
            }
        }  
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(centroid, 5);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Vector3.zero, 5);
    }
    public void changeSpeed(Slider slider)
    {
        translateSpeed = (1-slider.value)*(3.5f)+(slider.value*20f);
    }
   public void changeScaleSpeed(Slider slider)
    {
        ScrollSpeed= (1 - slider.value) * (5000) + (slider.value * 50000);
    }
   
}
