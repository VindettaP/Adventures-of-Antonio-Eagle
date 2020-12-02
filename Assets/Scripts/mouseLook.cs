using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    public Transform playerModel;
    public bool firstPerson = false;
    float xRotation = 0f;
    public GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        if (!firstPerson)
            cam.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; 
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (firstPerson)
        {
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        if (!firstPerson)
        {
            //Debug.Log("Y: " + mouseY + " Rot x: " + transform.rotation.eulerAngles.x);
            if (!(mouseY < 0 && transform.rotation.eulerAngles.x >= 348.0f && transform.rotation.eulerAngles.x < 350.0f) &&
                !(mouseY > 0 && transform.rotation.eulerAngles.x >= 35.0f && transform.rotation.eulerAngles.x < 40.0f))
                transform.RotateAround(playerBody.transform.position, playerBody.transform.right, mouseY);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
