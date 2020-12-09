using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask ground, whatisplayer;
    public float enemyHealth;

    //Idle
    public Vector3 walkStart;
    bool walkStartSet;
    public float walkRange;
    public GameObject sightSound;
    public GameObject deathSound;

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

        if(enemyHealth == 0){
            Debug.Log("playing sound");
            deathSound.GetComponent<AudioSource>().Play();
            animation_controller.SetBool("dead", true);
            agent.isStopped = true;
        }

        if(!playerInSight){
            animation_controller.SetBool("aggrod", false);
            patrolling();
        } 
        if(playerInSight) {
            //sightSound.GetComponent<AudioSource>().Play();
            animation_controller.SetBool("aggrod", true);
            chasePlayer();
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

    private void chasePlayer(){
        agent.speed = (vel*1.3f);
        agent.SetDestination(player.position);
    }

}


