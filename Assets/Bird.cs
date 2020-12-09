﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bird : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask ground, whatisplayer;
    public float enemyHealth;
    public GameObject screech;

    //Idle
    public Vector3 walkStart;
    bool walkStartSet;
    public float walkRange;

    private Animator animation_controller;
    private float vel;

    //Attacking: Add stuff, dunno if we want to add shooting or what
    
    //States
    public float sight, aggroRange;
    //Will need to add something if enemy will shoot something
    public bool playerInSight; 

    void Awake()
    {
        player = GameObject.Find("PlayerBody").transform;
        agent = GetComponent<NavMeshAgent>();
        vel = agent.speed;
        animation_controller = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInSight = Physics.CheckSphere(transform.position, sight, whatisplayer.value);

        if(playerInSight) {
            screech.GetComponent<AudioSource>().Play();
            turnAround();
            playerInSight = false;
        }
    }

    private void patrolling(){
        if (walkStartSet){
            agent.speed = vel;
            agent.SetDestination(walkStart);
        }
        if(!walkStartSet) searchWalkPoint();
  
        Vector3 distancetoWalkStart = transform.position - walkStart;

        if(distancetoWalkStart.magnitude < 1f){
            walkStartSet = false;
        }
    }
    private void searchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkRange, walkRange);
        float randomX = Random.Range(-walkRange, walkRange);
        walkStart = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkStart, -transform.up, 2f, ground))
            walkStartSet = true;
    }

    private void turnAround(){
        Vector3 dirToPlayer = gameObject.transform.position - player.transform.position;
        Vector3 newPos = gameObject.transform.position + dirToPlayer; 
        agent.speed = (vel*1.3f);
        agent.SetDestination(newPos);
    }
}
