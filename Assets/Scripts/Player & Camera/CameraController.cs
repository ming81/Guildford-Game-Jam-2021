using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bleak.Controller;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50.0f;
    public Transform playerTransformTarget;
    private Transform focusPoint;
    [SerializeField] [Range(0.01f, 1f)]
    private float smoothSpeed = 0.25f;
    [SerializeField] private Vector3 mainCameraPos;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] Camera mainCamera;
    public Vector3 desiredPosition;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        SmoothCam();
        CameraRotate();
    }

    void SmoothCam()
    {
        desiredPosition = playerTransformTarget.position;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        if (Input.GetKeyDown(KeyCode.K))
        {
            mainCamera.transform.localPosition = new Vector3(0, 7, -6);
            mainCamera.transform.localEulerAngles = new Vector3(45, 0, 0);
            mainCamera.fieldOfView = 35;
            smoothSpeed = 0.15f;
        }
    }

    void CameraRotate()
    {
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton5))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton4))
        {
            transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);
        }
    }
}
