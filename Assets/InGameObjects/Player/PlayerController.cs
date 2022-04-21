using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 velocity2D;
    private Vector2 desiredVelocity2D;
    private Vector3 playerPerspectiveVelocity;
    private Vector3 playerPerspectiveDesiredVelocity;
    private float velocityY;
    private float cameraDistance;

    [Header ("Customize")]
    [Tooltip("Control player movement relative to its own forward direction")]
    [SerializeField] private bool usePlayersPerspective;

    [Range(0, 10)]
    [SerializeField] private float maxSpeed = 4;
    [Range(0, 100)]
    [SerializeField] private float acceleration = 10;
    [Range(0, 50)]
    [SerializeField] private float jumpForce = 5;

    [SerializeField] private InputData[] inputs;
    public enum PlayerAction { Jump, Shoot } // etc.

    [Header("Applied Components - Do not change")]
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject upperBody;
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask worldLayerMask;

    private void Awake()
    {
        cameraDistance = Vector3.Distance(transform.position, cam.transform.position);
    }

    void Update()
    {
        checkInput();
        move();
        updateCamera();
    }

    private void checkInput()
    {
        // check movement inputs
        if (Input.GetKey(KeyCode.W))
            updateDesiredVelocity2D(false, 1);
        else if (Input.GetKey(KeyCode.S))
            updateDesiredVelocity2D(false, -1);
        else
            updateDesiredVelocity2D(false, 0);

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            updateDesiredVelocity2D(true, 0); // if player tries to move left AND right at the same time ignore this input
        else if (Input.GetKey(KeyCode.A))
            updateDesiredVelocity2D(true, -1);
        else if (Input.GetKey(KeyCode.D))
            updateDesiredVelocity2D(true, 1);
        else
            updateDesiredVelocity2D(true, 0);

        for (int i = 0; i < inputs.Length; i++)
        {
            if(Input.GetKeyDown(inputs[i].key))
            {
                performInput(inputs[i].actionType);
            }
        }
    }

    private void performInput(PlayerAction action)
    {
        switch(action)
        {
            case PlayerAction.Jump:
                jump();
                break;
            case PlayerAction.Shoot:
                shoot();
                break;
            default:
                break;
        }
    }

    private void updateDesiredVelocity2D(bool Xaxis, float value)
    {
        value = Mathf.Clamp(value, -1, 1);

        if (Xaxis)
            desiredVelocity2D.x = value * maxSpeed;
        else
            desiredVelocity2D.y = value * maxSpeed;

        if (desiredVelocity2D.magnitude > maxSpeed)
            desiredVelocity2D = desiredVelocity2D.normalized * maxSpeed;
    }

    Ray ray;
    RaycastHit raycastHit;
    private void move()
    {
        // compute new velocity vector
        if(velocityY == 0)
        {
            if (velocity2D.x < desiredVelocity2D.x)
                velocity2D.x += acceleration * Time.deltaTime;
            else if (velocity2D.x > desiredVelocity2D.x)
                velocity2D.x -= acceleration * Time.deltaTime;

            if (velocity2D.y < desiredVelocity2D.y)
                velocity2D.y += acceleration * Time.deltaTime;
            else if (velocity2D.y > desiredVelocity2D.y)
                velocity2D.y -= acceleration * Time.deltaTime;

            if (velocity2D.magnitude > maxSpeed)
                velocity2D = velocity2D.normalized * maxSpeed;
        }
        
        // check for obstacles
        if (Physics.SphereCast(transform.position + new Vector3(0, 1, 0), model.transform.localScale.x * 0.5f, model.transform.forward, out raycastHit, 0.1f, worldLayerMask))
        {
            // obstacle in front
            velocity2D = Vector2.zero;
        }

        // check if player is airborne
        if (velocityY <= 0 && Physics.SphereCast(transform.position + new Vector3(0, 1, 0), model.transform.localScale.x * 0.5f, Vector3.down, out raycastHit, 0.5f, worldLayerMask))
        {
            // grounded
            velocityY = 0;
            transform.position = new Vector3(transform.position.x, raycastHit.point.y, transform.position.z);
        }
        else
        {
            // airborne
            velocityY -= 9.81f * Time.deltaTime;
        }

        // perform movement and rotation
        if(usePlayersPerspective)
        {
            playerPerspectiveVelocity = upperBody.transform.forward * velocity2D.y + upperBody.transform.right * velocity2D.x;
            playerPerspectiveDesiredVelocity = upperBody.transform.forward * desiredVelocity2D.y + upperBody.transform.right * desiredVelocity2D.x;

            transform.position += new Vector3(playerPerspectiveVelocity.x, velocityY, playerPerspectiveVelocity.z) * Time.deltaTime;
            if (new Vector3(playerPerspectiveDesiredVelocity.x, 0, playerPerspectiveDesiredVelocity.y) != Vector3.zero)
                model.transform.LookAt(model.transform.position + new Vector3(playerPerspectiveDesiredVelocity.x, 0, playerPerspectiveDesiredVelocity.z));
        }
        else
        {
            transform.position += new Vector3(velocity2D.x, velocityY, velocity2D.y) * Time.deltaTime;
            if (desiredVelocity2D != Vector2.zero)
                model.transform.LookAt(model.transform.position + new Vector3(desiredVelocity2D.x, 0, desiredVelocity2D.y));
        }
    }
    
    private void updateCamera()
    {
        // make player follow camera view
        Vector3 mousePosition = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraDistance)); 
        upperBody.transform.LookAt(new Vector3(mousePosition.x, upperBody.transform.position.y, mousePosition.z));

        // prevent camera from falling into the deeps with player
        if(cam.transform.position.y < 5)
            cam.transform.position = new Vector3(cam.transform.position.x, 5, cam.transform.position.z);
    }
    
    private void jump()
    {
        if (velocityY != 0)
            return;
        velocityY = 5;
    }

    private void shoot()
    {
        print("shoot");
    }
}
