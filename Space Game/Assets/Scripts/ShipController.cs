using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{

    private float forwardSpeed = 20f, strifeSpeed = 20f, hoverSpeed = 25f;
    private float activeForwardSpeed, activeStrifeSpeed, activeHoverSpeed;
    private float forwardAcceleration = 30f, strifeAcceleration = 30f, hoverAcceleration = 3f;
    public float rotateSpeed = 90f;
    private Vector2 lookInput, screenCenter, mouseDistance;

    private void Start()
    {
        screenCenter.x = Screen.width / 2;
        screenCenter.y = Screen.height / 2;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        
        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;

        mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.y;
        mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        
        activeForwardSpeed = Mathf.Lerp(activeForwardSpeed,Input.GetAxisRaw("Vertical") * forwardSpeed, forwardAcceleration * Time.deltaTime);
        activeStrifeSpeed = Mathf.Lerp(activeStrifeSpeed,Input.GetAxisRaw("Horizontal") * strifeSpeed, strifeAcceleration * Time.deltaTime);
        activeHoverSpeed = Mathf.Lerp(activeHoverSpeed,Input.GetAxisRaw("Hover") * -hoverSpeed, hoverAcceleration * Time.deltaTime);
        
        Debug.Log("X mouse : " + mouseDistance.x + "Y Mouse: " +  -mouseDistance.y +  "StrifeSpeed: " + activeStrifeSpeed);

        transform.Rotate(-mouseDistance.y * rotateSpeed * Time.deltaTime, activeStrifeSpeed * Time.deltaTime, mouseDistance.x * rotateSpeed * Time.deltaTime,Space.Self);

        if (activeForwardSpeed > 0f)
        {
            transform.position += transform.up * -activeForwardSpeed * Time.deltaTime;
        }
        transform.position += transform.forward * -activeHoverSpeed * Time.deltaTime;

    }
}
