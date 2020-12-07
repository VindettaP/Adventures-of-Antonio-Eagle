using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class player : MonoBehaviour
{
    public Vector3 movement_direction;
    public float walking_velocity = 5f;
    public Vector3 velocity;
    public float max_velocity;
    public float acceleration = 10.0f;
    public float turn_speed = 500f;
    public float gravity = 9.8f;
    public float grappleSpeed = 2.0f;
    public float grappleLength = 3f;
    public float dashLength = 0.5f;
    public float jumpLength = 1.0f;
    public float jumpStrength = 50f;
    public float timeBetweenJumps = 1f;
    public float drag = 0.1f;
    public float airDrag = 0.02f;
    public bool doubleJumpUnlocked = false;
    public bool grappleUnlocked = false;
    public bool dashUnlocked = false;

    public string state;

    private bool gravenable = true;
    private float gravconst;
    private Animator animation_controller;
    private CharacterController character_controller;
    private GameObject playerModel;
    private GameObject modelRender;
    private bool upKey;
    private bool downKey;
    private bool leftKey;
    private bool rightKey;
    private bool ctrlDown;
    private bool shiftDown;
    private bool spaceDown;

    private bool eDown;
    //private bool turning;
    private bool grounded;
    private Grapple grappleScript;
    private float jumpTime = 0;
    private float doubleJumpTime = 0;
    private bool jumping;
    private float jumpCooldown = 0;
    private GameObject cursor;

    private bool tabDown;
    private bool doubleJumped = false;
    private bool camerap;
    public GameObject fPerson, tPerson;
    public float dashstr;
    public int dashes;
    private bool dashing;
    private float dashTimeLeft;
    private float xDirDash = 0;
    private float zDirDash = 0;
    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponentInChildren<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = new Vector3(0, 0, 0);
        state = "idle";
        playerModel = GameObject.Find("PlayerModel");
        modelRender = GameObject.Find("BodySkin");
        cursor = GameObject.Find("AimingCursor");
        // disable model render in first person
        modelRender.GetComponent<SkinnedMeshRenderer>().enabled = false;
        //turning = false;
        jumping = false;
        jumpTime = jumpLength;
        doubleJumpTime = jumpLength;
        jumpCooldown = timeBetweenJumps;
        grappleScript = GameObject.Find("Grapple").GetComponent<Grapple>();
        camerap = false;
        gravconst = gravity;
        dashes = 0;
        dashing = false;
        timeBetweenJumps = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        // Unlock grapple if we get it
        grappleScript.grappleUnlocked = grappleUnlocked;

        // Check if player model is grounded
        grounded = IsGrounded();

        //Debug.Log("Grounded: " + grounded + " Jumping: " + jumping + " JumpTime: " + jumpTime + " State: " + state + " Velocity: " + velocity);

        if (gravenable)
            gravity = gravconst;
        else
            gravity = 0;

        if (jumping)
        {
            if (doubleJumped)
                doubleJumpTime -= Time.deltaTime;
            jumpTime -= Time.deltaTime; // decrease time in jump while we are jumping
        }
        else
            jumpCooldown -= Time.deltaTime;

        // Check all keys first for convenience
        upKey = Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow);
        downKey = Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow);
        leftKey = Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow);
        rightKey = Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow);
        ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        spaceDown = Input.GetKeyDown(KeyCode.Space);
        eDown = Input.GetKeyDown(KeyCode.E);
        tabDown = Input.GetKeyDown(KeyCode.Tab);

        // set state based on input
        /* in animation controller, states controlled by int
         * 0 = idle
         * 1 = walk forward
         * 3 = run forward
         * 4 = jumping
         * 5 = mid air
         * 6 = landing
        */
        //Changes between regular camera to the other camera 
        if(tabDown && camerap && Time.timeScale > 0)
        {
            fPerson.SetActive(true);
            tPerson.SetActive(false);
            camerap = false;
            timeBetweenJumps = 0.1f;

            // Enable aiming cursor in first person
            cursor.SetActive(true);

            // Set player model to face direction of first person camera when switching
            float y = fPerson.transform.eulerAngles.y;
            playerModel.transform.eulerAngles = new Vector3(playerModel.transform.eulerAngles.x, y, playerModel.transform.eulerAngles.z);

            // disable model render in first person
            modelRender.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else if (tabDown && !camerap && Time.timeScale > 0)
        {
            tPerson.SetActive(true);
            fPerson.SetActive(false);
            grappleScript.StopGrapple();
            camerap = true;
            timeBetweenJumps = 0.5f;

            // enable model render in third person
            modelRender.GetComponent<SkinnedMeshRenderer>().enabled = true;

            // disable aiming cursor in third person
            cursor.SetActive(false);
        }
        

        if (!grounded && doubleJumpUnlocked && spaceDown && !doubleJumped) // handle double jumping
        {
            doubleJumpTime = jumpLength; // just start the upward velocity again
            doubleJumped = true;
        }

        if (grappleScript.grappling)
        {
            state = "grappling";
            jumping = true;
        }
        else if (!grounded) // still in midair post jump
            state = "midAir";
        else if ((jumpTime < 0 && grounded && jumping) || (velocity.y < -0.6f && grounded)) // landed from a jump
        {
            state = "landing";
            jumping = false;
            doubleJumped = false;
            jumpCooldown = timeBetweenJumps;
            jumpTime = jumpLength; // reset jump timer
            velocity.y = -0.5f; // reset velocity
        }
        else if ((spaceDown && !jumping && jumpCooldown < 0) || (jumping && jumpTime > 0))
        {
            state = "jumpStart";
            jumping = true;
            //jumpTime = jumpLength; // reset jump timer
        }
        else if (shiftDown && (leftKey || rightKey || upKey || downKey))
            state = "run";
        else if (leftKey || rightKey || upKey || downKey)
            state = "forwardWalk";
        else
            state = "idle";

      
        // FSM for character behavior, also update velocity and handle turning
        switch (state)
        {
            case "idle":
                animation_controller.SetInteger("state", 0);
                max_velocity = walking_velocity;
                break;
            case "forwardWalk":
                animation_controller.SetInteger("state", 1);
                max_velocity = walking_velocity;
                break;
            case "jumpStart":
                animation_controller.SetInteger("state", 4);
                //max_velocity = walking_velocity;
                break;
            case "midAir":
                animation_controller.SetInteger("state", 5);
                //max_velocity = walking_velocity;
                break;
            case "landing":
                animation_controller.SetInteger("state", 6);
                max_velocity = walking_velocity;
                break;
            case "run":
                animation_controller.SetInteger("state", 3);
                max_velocity = 2.0f * walking_velocity;
                break;
            case "grappling":
                max_velocity = 2.0f * walking_velocity;
                break;
            default:
                break;
        }


        // dir is the direction to rotate based on movement
        string dir = "north";
        if ((upKey || downKey) && (!rightKey && !leftKey))
        {
            // case where no strafing, go straight, also do this if both keys are held
            if (downKey)
                dir = "south";
            else
                dir = "north";
        }
        else if (leftKey && !rightKey && (upKey || downKey))
        {
            // strafing left case
            if (upKey)
                dir = "northEast";
            else if (downKey)
                dir = "southEast";
        }
        else if (rightKey && !leftKey && (upKey || downKey))
        {
            // strafing right case
            if (upKey)
                dir = "northWest";
            else if (downKey)
                dir = "southWest";
        }
        else if (leftKey && !rightKey)
        {
            // moving left case
            dir = "east";
        }
        else if (rightKey && !leftKey)
        {
            // moving right case
            dir = "west";
        }
        else
            dir = "none";


        //DASHING
        if (dashUnlocked)
        {
            if (eDown && !grounded)
            {
                dashing = true;
                dashTimeLeft = dashLength;

                // Set direction for dash
                switch (dir)
                {
                    case "north":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        break;
                    case "south":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 180));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 180));
                        break;
                    case "east":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 270));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 270));
                        break;
                    case "west":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90));
                        break;
                    case "northEast":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 315));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 315));
                        break;
                    case "northWest":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45));
                        break;
                    case "southEast":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 225));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 225));
                        break;
                    case "southWest":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 135));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 135));
                        break;
                    case "none":
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        break;
                    default:
                        xDirDash = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        zDirDash = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                        break;
                }
            }
            if (dashing == true && dashTimeLeft > 0)
            {
                dashTimeLeft -= Time.deltaTime;
                velocity.x = dashstr * xDirDash;
                velocity.z = dashstr * zDirDash;
                velocity.y = 0;
            }
            if (dashTimeLeft <= 0 && dashing == true)
            {
                dashing = false;
                velocity.x *= 0.2f; 
                velocity.z *= 0.2f;
            }
        }

        Debug.Log("Velocity: " + velocity);

        if(!grounded){
            gravity = gravconst;
        }
        if(grounded){
            dashes = 0;
            dashing = false;
        }

        // update movement direction based on keys
        float xdirection = 0.0f;
        float zdirection = 0.0f;
        xdirection = Mathf.Sin(Mathf.Deg2Rad * playerModel.transform.rotation.eulerAngles.y);
        zdirection = Mathf.Cos(Mathf.Deg2Rad * playerModel.transform.rotation.eulerAngles.y);

        if (camerap) // handle 3rd person movement
        {
            if (state != "jump" && state != "midAir" && grounded) // don't rotate player if jumping
                RotationUpdate(dir);
            VelocityUpdate(xdirection, zdirection, dir);
            PositionUpdate(xdirection, zdirection);
        }
        else // handle 1st person movement (don't rotate body in 1st person)
        {
            VelocityUpdate(xdirection, zdirection, dir);
            PositionUpdate(xdirection, zdirection);
        }
    }

    // uses a short raycast down to see if player model is on the ground
    bool IsGrounded()
    {
        return Physics.Raycast(playerModel.transform.position, -Vector3.up, 0.1f);
    }

    void MoveTowardsGrapple()
    {
        Vector3 offset = grappleScript.grapplePoint - playerModel.transform.position;
        float distance = offset.magnitude;

        Vector3 force = Vector3.Normalize(offset) * (distance - grappleLength) * grappleSpeed;

        velocity.x += force.x;
        velocity.y += force.y / 1.5f;
        velocity.z += force.z;
    }

    void AirVelocityUpdate(string dir)
    {
        float xDir2 = 0;
        float zDir2 = 0;
        bool dontMove = false;
        switch (dir)
        {
            case "north":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y));
                break;
            case "south":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 180));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 180));
                break;
            case "east":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 270));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 270));
                break;
            case "west":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90));
                break;
            case "northEast":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 315));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 315));
                break;
            case "northWest":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45));
                break;
            case "southEast":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 225));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 225));
                break;
            case "southWest":
                xDir2 = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 135));
                zDir2 = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 135));
                break;
            case "none":
                dontMove = true;
                break;
            default:
                dontMove = true;
                break;
        }

        if (!dontMove)
        {
            Vector3 velocityDir = Vector3.Normalize(new Vector3(velocity.x, 0, velocity.z));
            if (Mathf.Abs(velocity.x) < Mathf.Abs((max_velocity + 2) * xDir2) || (Mathf.Sign(velocity.x) != Mathf.Sign(xDir2) && xDir2 != 0))
            {
                velocity.x += acceleration * xDir2;
                if (Mathf.Abs(velocity.x) > Mathf.Abs(max_velocity * xDir2))
                {
                    //velocity.x = (max_velocity * xDir2);
                    velocity.x += airDrag * velocityDir.x;
                }
            }

            if (Mathf.Abs(velocity.z) < Mathf.Abs((max_velocity + 2) * zDir2) || (Mathf.Sign(velocity.z) != Mathf.Sign(zDir2) && zDir2 != 0))
            {
                velocity.z += acceleration * zDir2;
                if (Mathf.Abs(velocity.z) > Mathf.Abs(max_velocity * zDir2))
                {
                    //velocity.z = (max_velocity * zDir2);
                    velocity.z += airDrag * velocityDir.z;
                }
            }
        }
    }

    void VelocityUpdate(float xDir, float zDir, string dir)
    {
        Vector3 velocityDir = Vector3.Normalize(new Vector3(velocity.x, 0, velocity.z));

        //Debug.Log("Player Model Rotation: " + playerModel.transform.rotation.eulerAngles + " Body rotation: " + transform.rotation.eulerAngles + " Velocity: " + velocity + " Dir: " + xDir + "," + zDir);
        // vector velocity update
        if ((upKey || downKey || leftKey || rightKey) && grounded && state != "grappling" && camerap)
        {
            if (Mathf.Abs(velocity.x) < Mathf.Abs((max_velocity + 2) * xDir) || (Mathf.Sign(velocity.x) != Mathf.Sign(xDir) && xDir != 0))
            {
                velocity.x += acceleration * xDir;
                if (Mathf.Abs(velocity.x) > Mathf.Abs(max_velocity * xDir))
                    velocity.x += drag * velocityDir.x;
            }

            if (Mathf.Abs(velocity.z) < Mathf.Abs((max_velocity + 2) * zDir) || (Mathf.Sign(velocity.z) != Mathf.Sign(zDir) && zDir != 0))
            {
                velocity.z += acceleration * zDir;
                if (Mathf.Abs(velocity.z) > Mathf.Abs(max_velocity * zDir))
                    velocity.z += drag * velocityDir.z;
            }
        }
        else if ((upKey || downKey || leftKey || rightKey) && state != "grappling")
        {
            AirVelocityUpdate(dir);
        }

        
        velocityDir = Vector3.Normalize(new Vector3(velocity.x, 0, velocity.z));  // re-update velocity
        float d = 0.0f;
        if (grounded)
            d = drag;
        else
            d = airDrag;

        d = d / 2;

        if (Mathf.Abs(velocity.x) > 0)
        {
            //if (Mathf.Abs(velocity.x) < d)  // set to 0 if we get close enough
            //    velocity.x = 0;
            //else
           // {
                if (grounded)
                    velocity.x -= drag * velocityDir.x;
                else
                    velocity.x -= airDrag * velocityDir.x;
            //}
        }
        if (Mathf.Abs(velocity.z) > 0)  
        {
            //if (Mathf.Abs(velocity.z) < d
            //    velocity.z = 0;
            //else
           // {
                if (grounded)
                    velocity.z -= drag * velocityDir.z;
                else
                    velocity.z -= airDrag * velocityDir.z;
            //}
        }

        // handle jumping
        if (jumping && (jumpTime > 0))
        {
            if (doubleJumped && (doubleJumpTime > 0))
                velocity.y = 2 * jumpStrength; // double jumped while jumping, go up faster
            else
                velocity.y = jumpStrength; // just single jump
        }
        else if (doubleJumped && (doubleJumpTime > 0))
        {
            velocity.y = jumpStrength; // just double jump
        }

        // add gravity
        if (!grounded)
            velocity.y -= gravity * Time.deltaTime;

        // Handle grappling gun
        if (state == "grappling")
        {
            MoveTowardsGrapple();
        }
    }

    void PositionUpdate(float xDir, float zDir)
    {
        //movement_direction = new Vector3(xDir, 0.0f, zDir);
        //movement_direction.Normalize();

        // Move based on final velocity
        character_controller.Move(velocity * Time.deltaTime);
    }

    // helper to rotate player model
    // if turn is true, update rotation, if not do not
    void RotationUpdate(string dir)
    {
        float target = 0;
        bool dontMove = false;
        switch (dir)
        {
            case "north":
                target = transform.rotation.eulerAngles.y;
                break;
            case "south":
                target = transform.rotation.eulerAngles.y + 180;
                break;
            case "east":
                target = transform.rotation.eulerAngles.y + 270;
                break;
            case "west":
                target = transform.rotation.eulerAngles.y + 90;
                break;
            case "northEast":
                target = transform.rotation.eulerAngles.y + 315;
                break;
            case "northWest":
                target = transform.rotation.eulerAngles.y + 45;
                break;
            case "southEast":
                target = transform.rotation.eulerAngles.y + 225;
                break;
            case "southWest":
                target = transform.rotation.eulerAngles.y + 135;
                break;
            case "none":
                dontMove = true;
                break;
            default:
                target = transform.rotation.eulerAngles.y;
                break;
        }
        target = target % 360;
        //target = target + tPerson.transform.eulerAngles.y;

        //Debug.Log("Player Model Rotation: " + playerModel.transform.rotation.eulerAngles + " Body rotation: " + transform.rotation.eulerAngles + " Velocity: " + velocity);

        //turning = false;

        if (state != "jump" && !dontMove)
        {
            if ((target + 1) > playerModel.transform.rotation.eulerAngles.y && (target - 1) < playerModel.transform.rotation.eulerAngles.y)
            {
                playerModel.transform.eulerAngles = new Vector3(0, target, 0);
            }
            else
            {
                //turning = true;
                if ((target - playerModel.transform.rotation.eulerAngles.y + 360) % 360 > 180)
                {
                    playerModel.transform.Rotate(new Vector3(0, -turn_speed * Time.deltaTime, 0));
                }
                else
                {
                    playerModel.transform.Rotate(new Vector3(0, turn_speed * Time.deltaTime, 0));
                }
            }
        }
    }
}
