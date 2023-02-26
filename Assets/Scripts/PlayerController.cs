using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    //Public variables
    public int Health = 3;
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
    private Vector3 PrevRawInputMovement;
    private Vector3 rawInputCamera;
    private GameObject lookAtTarget;
    private GameObject modelDirection;
    public CinemachineVirtualCamera cam;
    public Animator animator;

    //Move variables
    public GameObject Checkpoint;
    private Vector3 angles;
    private Vector3 move;
    private RaycastHit hit;
    private RaycastHit shadowRayHit;
    public GameObject Shadow;
    public GameObject ShadowRealModel;
    public float raydistance;

    private bool isKnockback = false;
    public float knockbackDuration = 0.2f;
    public float knockbackForce =2f;
    private float knockbackTimer;

    private bool isInvincible = false;
    public float invincibleDuration = 0.5f;
    private float invincicbleTimer;

    //Flags
    private bool CanceledJump = false;
    private float DashCooldown;
    private Vector3 Dash;
    private bool DiveBool = false;
    private bool DiveGround = false;
    [SerializeField] private UnityEvent Dashed;
    [SerializeField] private UnityEvent DashedEnd;
    private bool JumpBool = false;
    private bool DoubleJumpBool = false;
    private bool WallDetection = false;
    private bool WallJumpState = false;
    private bool Dead = false;

    //health
    public GameObject HP3;
    public GameObject HP2;
    public GameObject HP1;

    //AnimationStates
    private string currentState;
    const string Idle = "Idle";
    const string Run = "Run";
    const string JumpStart = "JumpStart";
    const string RiseAir = "RiseAir";
    const string RiseToFall = "RiseToFall";
    const string FallAir = "FallAir";
    const string Dive = "Dive";
    const string RollOut = "RollOut";
    const string WallGrind = "WallGrind";
    const string WallJump = "WallJump";
    const string LedgeGrab = "LedgeGrab";
    const string LedgeLift = "LedgeLift";
    const string DoubleJumpStart = "DoubleJumpStart";
    const string DoubleJumpSpin = "DoubleJumpSpin";




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
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out shadowRayHit, Mathf.Infinity))
        {
            if(shadowRayHit.distance > 1)
            {
                Shadow.SetActive(true);
                Shadow.transform.position = shadowRayHit.point + new Vector3(0f,0.01f,0f);
                ShadowRealModel.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                Shadow.SetActive(false);
                ShadowRealModel.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
        //Debug.Log(controller.isGrounded);
        if(Physics.Raycast(transform.position,modelDirection.transform.TransformDirection(Vector3.forward), out hit,raydistance))
        {
            Debug.DrawRay(transform.position, modelDirection.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Hit");
            WallDetection = true;
        }
        else
        {
            Debug.DrawRay(transform.position, modelDirection.transform.TransformDirection(Vector3.forward) * raydistance, Color.white);
            //Debug.Log("no hit");
            WallDetection = false;
        }
        //Debug.Log(WallJumpState);
        //CheckHealth();
    }

    private void FixedUpdate()
    {
        if(controller.isGrounded)
        {
            //Landed Reset grav scale
            gravityScale = 1f;
            CanceledJump = false;
            DashCooldown = DashCoolDownTime;
            
            if (velocity.y<0)
            {
                JumpBool = false;
                DoubleJumpBool = false;
                if(DiveBool)
                {
                //ChangeAnimationState(RollOut);
                DiveGround = true;
                rawInputMovement = Vector3.zero;

                }
            }
        }

        //WallJumpState
        if(JumpBool && WallDetection)
        {
            WallJumpState = true;
        }

        //Check falling
        if(velocity.y<0)
        {
            CanceledJump = true;
            if (CheckCurrentAnimation(RiseAir)) ChangeAnimationState(RiseToFall);
            if (CheckCurrentAnimation(Idle) && velocity.y <-1) ChangeAnimationState(FallAir);
            if (CheckCurrentAnimation(Run) && velocity.y < -1) ChangeAnimationState(FallAir);
            //If we reached the ground set fall speed to small negative number so grav does not increase
            if (controller.isGrounded)
            {
                velocity.y = -0.001f;
                WallJumpState = false;

                if(CheckCurrentAnimation(RiseToFall)
                    || CheckCurrentAnimation(FallAir) 
                    || CheckCurrentAnimation(RiseAir)
                    || CheckCurrentAnimation(JumpStart)
                    || CheckCurrentAnimation(DoubleJumpStart)
                    || CheckCurrentAnimation(DoubleJumpSpin))
                {
                    ChangeAnimationState(Idle);
                }
            }
        }

        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
        }
        if (knockbackTimer <= 0)
        {
            isKnockback = false;
        }

        if (isInvincible)
        {
            invincicbleTimer -= Time.deltaTime;
        }
        if (invincicbleTimer <= 0)
        {
            isInvincible = false;
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

        //Dash
        DashCooldown -= Time.deltaTime;
        if (DashCooldown >= 0)
        {
            DashCooldown = 0;
            Dash = Vector3.zero;
        }

        //transform rotation and apply changes
        lookAtTarget.transform.localEulerAngles = angles;

        //Calculate movement vector through rawinput, apply to target movement local transform(direction looking)
        move = rawInputMovement.x * transform.right + transform.forward * rawInputMovement.z + Dash;
        /*
        if (move != Vector3.zero
            && !CheckCurrentAnimation(Dive)
            && !CheckCurrentAnimation(RiseAir)
            && !CheckCurrentAnimation(JumpStart)) animator.SetInteger("Run",1);
            */

        if (isKnockback)
        {
            //Overwrite input to hurt direction behind character model
            move = -modelDirection.transform.forward * knockbackForce;
            //Debug.Log("knockback");
        }

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
            animator.SetInteger("Run", 1);
        }
        else
        {
            animator.SetInteger("Run", 0);
        }
        if (Dead) Death();
    }

    public void OnMainStickMove(InputAction.CallbackContext value)
    {
        Transform direction = transform.Find("MoveDirection");
        PrevRawInputMovement = new Vector3(value.ReadValue<Vector2>().x, 0f, value.ReadValue<Vector2>().y);
        if (DiveGround)
        {
            direction.localPosition = Vector3.zero;
            return;
        }
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
        if(JumpBool && !DoubleJumpBool && !DiveBool && value.started)
        {
            FindAnyObjectByType<AudioManager>().Play("Jump");
            velocity.y = Mathf.Sqrt(JumpForce * -2f * gravity);
            gravityScale = 1f;
            DoubleJumpBool = true;
            ChangeAnimationState(DoubleJumpStart);
        }
        
        if (controller.isGrounded && value.started && !DiveBool)
        {
            FindAnyObjectByType<AudioManager>().Play("Jump");
            velocity.y = Mathf.Sqrt(JumpForce * -2f * gravity);
            gravityScale = 1f;
            JumpBool = true;
            ChangeAnimationState(JumpStart);
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

    public void OnDash(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            if(DiveGround)
            {
                Transform direction = transform.Find("MoveDirection");
                ChangeAnimationState(RollOut);
                DiveGround = false;
                DiveBool = false;
                DashedEnd.Invoke();
                rawInputMovement = PrevRawInputMovement;
                direction.position = rawInputMovement.x * transform.right + transform.forward * rawInputMovement.z + transform.position;
                return;
            }
            Dash = modelDirection.transform.forward * DashForce;
            if (DiveBool) return;
            if(controller.isGrounded)
            {
                //Debug.Log("roll");
                velocity.y = Mathf.Sqrt(0.5f * -2 * gravity);
            }
            Dashed.Invoke();
            ChangeAnimationState(Dive);
            DiveBool = true;
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

    public void Hurt()
    {
        isInvincible = true;
        isKnockback = true;
        knockbackTimer = knockbackDuration;
        invincicbleTimer = invincibleDuration;
        Health -= 1;
        CheckHealth();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<HurtBox>())
        {
            if (!isInvincible) Hurt();
        }
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        animator.Play(newState);
    }

    bool CheckCurrentAnimation(string animationToCheck)
    {
        AnimatorClipInfo[] clipInfo;
        clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length == 0) return false;

            if (clipInfo[0].clip.name == animationToCheck)
            {
                return true;
            }
            return false;

    }

    public void CheckHealth()
    {
        if(Health == 3)
        {
            HP3.GetComponent<Image>().color = Color.white;
            HP2.GetComponent<Image>().color = Color.white;

        }
        else if(Health == 2)
        {
            HP3.GetComponent<Image>().color = Color.black;
            HP2.GetComponent<Image>().color = Color.white;

        }
        else if(Health == 1)
        {
            HP3.GetComponent<Image>().color = Color.black;
            HP2.GetComponent<Image>().color = Color.black;

        }
        else if(Health == 0)
        {
            Dead = true;
        }
    }
    void Death()
    {
        gameObject.transform.position = Checkpoint.transform.position + new Vector3(0f,0.65f,0f);
        HP3.GetComponent<Image>().color = Color.white;
        HP2.GetComponent<Image>().color = Color.white;
        Health = 3;
        Dead = false;
    }
}
