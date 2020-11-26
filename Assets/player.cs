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
    public float velocity;
    public float max_velocity;
    public float acceleration = 10.0f;
    public float turn_speed = 500f;
    public float gravity = 9.8f;

    public string state;

    private Animator animation_controller;
    private CharacterController character_controller;
    private GameObject playerModel;
    private bool upKey;
    private bool downKey;
    private bool leftKey;
    private bool rightKey;
    private bool ctrlDown;
    private bool shiftDown;
    private bool spaceDown;
    private bool turning;
    private bool grounded;

    private bool tabDown;
    public GameObject fPerson, tPerson;
    // tiny helper to save time, if forward is true update forwards, else backwards
    // if turn is true, update rotation, if not do not
    void VelocityUpdate(bool forward)
    {
        if (forward)
        {
            velocity += acceleration;
            if (velocity > max_velocity)
                velocity = max_velocity;
        }
        else
        {
            velocity -= acceleration;
            if (velocity < - max_velocity)
                velocity = - max_velocity;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponentInChildren<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 0.0f;
        state = "idle";
        playerModel = GameObject.Find("PlayerModel");
        turning = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player model is grounded
        grounded = IsGrounded();

        // Check all keys first for convenience
        upKey = Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow);
        downKey = Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow);
        leftKey = Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow);
        rightKey = Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow);
        ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        spaceDown = Input.GetKey(KeyCode.Space);
        tabDown = Input.GetKey(KeyCode.Tab);

        // set state based on input
        /* in animation controller, states controlled by int
         * 0 = idle
         * 1 = walk forward
         * 3 = run forward
         * 4 = jumping
        */
        //Changes between regular camera to the other camera 
        if(tabDown){
            fPerson.SetActive(true);
            tPerson.SetActive(false);
        }
        else{
            tPerson.SetActive(true);
            fPerson.SetActive(false);
        }

        if (spaceDown && state != "jump")
            state = "jump";
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
                max_velocity = 0.0f;
                VelocityUpdate(true);
                break;
            case "forwardWalk":
                animation_controller.SetInteger("state", 1);
                max_velocity = walking_velocity;
                VelocityUpdate(true);
                break;
            case "jump":
                animation_controller.SetInteger("state", 4);
                max_velocity = walking_velocity;
                VelocityUpdate(true);
                break;
            case "run":
                animation_controller.SetInteger("state", 3);
                max_velocity = 2.0f * walking_velocity;
                VelocityUpdate(true);
                break;
            default:
                break;
        }

        // update movement direction based on keys
        float xdirection = 0.0f;
        float zdirection = 0.0f;
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
            {
                dir = "northEast";

            }
            else if (downKey)
            {
                dir = "southEast";
            }
        }
        else if (rightKey && !leftKey && (upKey || downKey))
        {
            // strafing right case
            if (upKey)
            {
                dir = "northWest";
            }
            else if (downKey)
            {
                dir = "southWest";
            }
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
        {
            dir = "none";
        }

        xdirection = Mathf.Sin(Mathf.Deg2Rad * playerModel.transform.rotation.eulerAngles.y);
        zdirection = Mathf.Cos(Mathf.Deg2Rad * playerModel.transform.rotation.eulerAngles.y);

        RotationUpdate(dir);
        PositionUpdate(xdirection, zdirection);
    }

    // uses a short raycast down to see if player model is on the ground
    bool IsGrounded()
    {
        return Physics.Raycast(playerModel.transform.position, -Vector3.up, 0.9f);
    }

    void PositionUpdate(float xDir, float zDir)
    {
        movement_direction = new Vector3(xDir, 0.0f, zDir);
        movement_direction.Normalize();

        if (!grounded)
            character_controller.Move(new Vector3(0, -gravity, 0) * Time.deltaTime);

        if (turning)
            character_controller.Move(0.3f * movement_direction * velocity * Time.deltaTime);
        else
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
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


        turning = false;

        if (state != "jump" && !dontMove)
        {
            if ((target + 10) > playerModel.transform.rotation.eulerAngles.y && (target - 0) < playerModel.transform.rotation.eulerAngles.y)
            {
                playerModel.transform.eulerAngles = new Vector3(0, target, 0);
            }
            else
            {
                turning = true;
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
