using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaiCameraLook : MonoBehaviour
{
  [SerializeField] private float sensX, sensY;

  [SerializeField] private Transform cameraTransform;
  [SerializeField] private Transform orientation;


  float mouseX, mouseY;
  float multiplier = 0.1f;

  float xRotation, yRotation;

  private void Start()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
  }

  private void Update()
  {
    MyInput();
    cameraTransform.localRotation = Quaternion.Euler(xRotation, yRotation, cameraTransform.localRotation.z);
    orientation.rotation = Quaternion.Euler(0, yRotation, 0);
  }

  private void MyInput()
  {
    mouseX = Input.GetAxisRaw("Mouse X");
    mouseY = Input.GetAxisRaw("Mouse Y");

    yRotation += mouseX * sensX * multiplier;
    xRotation -= mouseY * sensY * multiplier;

    xRotation = Mathf.Clamp(xRotation, -90f, 90f);
  }
}
