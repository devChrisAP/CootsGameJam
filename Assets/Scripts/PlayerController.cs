using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{

    //Public variables
    public float Speed = 10f;
    public float RotationSpeed = 25f;
    public float JumpForce = 10f;
    public float DashForce = 1f;
    public float DashCoolDownTime = 3f;

    //Physics stuff
    private float gravity = -9.81f;
    private float gravityScale = 1f;
    private Vector3 velocity;

    //Input management
    private CharacterController controller;
    private Vector3 rawInputMovement;
    private Vector3 rawInputCamera;
    private GameObject lookAtTarget;
    private GameObject modelDirection;
    public CinemachineVirtualCamera cam;
    public Animator animator;

    //Move variables
    private Vector3 angles;
    private Vector3 move;

    //Flags
    private bool CanceledJump = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        lookAtTarget = transform.Find("LookAtTarget").gameObject;
        modelDirection = transform.Find("CootsContainer").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(controller.isGrounded)
        {
            //Landed Reset grav scale
            gravityScale = 1f;
            CanceledJump = false;
        }

        //Check falling
        if(velocity.y<0)
        {
            CanceledJump = true;
            //If we reached the ground set fall speed to small negative number so grav does not increase
            if(controller.isGrounded)
            {
                velocity.y = -0.001f;
            }
        }

        //Recieve camera input and rotate the look target so view is rotated
        lookAtTarget.transform.rotation *= Quaternion.AngleAxis(rawInputCamera.x * RotationSpeed, Vector3.up);
        lookAtTarget.transform.rotation *= Quaternion.AngleAxis(rawInputCamera.z * RotationSpeed, Vector3.right);

        //Handle rotations that go over 360 degrees
        angles = lookAtTarget.transform.localEulerAngles;
        angles.z = 0;

        var angle = lookAtTarget.transform.localEulerAngles.x;

        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if (angle < 180 && angle > 89)
        {
            angles.x = 88;
        }

        ZoomInCam(angle);

        //transform rotation and apply changes
        lookAtTarget.transform.localEulerAngles = angles;

        //Calculate movement vector through rawinput, apply to target movement local transform(direction looking)
        move = rawInputMovement.x * transform.right + transform.forward * rawInputMovement.z ;

        //Apply horizontal movement vector
        controller.Move(move * Speed * Time.deltaTime);

        //Calculate gravity accel and add to velocity
        velocity.y += gravity * gravityScale * Time.deltaTime;

        //apply vertical movment vector
        controller.Move(velocity * Time.deltaTime);

        if (rawInputMovement != Vector3.zero)
        {
            //Set Player wrapper rotation to look target rotation (align to where camera is looking)
            transform.rotation = Quaternion.Euler(0, lookAtTarget.transform.rotation.eulerAngles.y, 0);
            //Reset cam but keep vertical angle
            lookAtTarget.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
        }
    }

    public void OnMainStickMove(InputAction.CallbackContext value)
    {
        Transform direction = transform.Find("MoveDirection");
        //ReadValue
        rawInputMovement = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);
        //Move forward position (raw input x * right, raw input z * forward + position - height difference)
        direction.position = rawInputMovement.x * transform.right + transform.forward * rawInputMovement.z + transform.position;
    }

    public void OnCamMovement(InputAction.CallbackContext value)
    {
        rawInputCamera = new Vector3(value.ReadValue<Vector2>().x, 0f, -value.ReadValue<Vector2>().y);
        if (value.canceled)
        {
            rawInputCamera = Vector3.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext value)
    {
        if (controller.isGrounded && value.started)
        {
            velocity.y = Mathf.Sqrt(JumpForce * -2f * gravity);
            gravityScale = 1f;


        }
        if (value.canceled)
        {

            if (!CanceledJump)
            {
                CanceledJump = true;
                velocity.y = 0f;
            }
            gravityScale = 3f;

        }
    }

    public void ZoomInCam(float angle)
    {
        float m;
        float b;
        if (angle < 180)
        {
            m = 3f / 40f;
            b = 5f;
            if (angle > 40) return;
        }
        else
        {
            m = 1f / 5f;
            b = -67f;
        }
        cam.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraDistance = angle * m + b;
    }
}
