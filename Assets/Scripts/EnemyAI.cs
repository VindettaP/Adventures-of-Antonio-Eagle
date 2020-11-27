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

    //Attacking: Add stuff, dunno if we want to add shotting or what
    
    //States
    public float sight, aggroRange;
    //Will need to add something if enemy will shoot something
    public bool playerInSight; 

    void Awake()
    {
        player = GameObject.Find("PlayerBody").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        playerInSight = Physics.CheckSphere(transform.position, sight, whatisplayer);

        if(!playerInSight) patrolling();
        if(playerInSight) chasePlayer();
    }

    private void patrolling(){
        if(!walkStartSet) SearchWalkPoint();

        if (walkStartSet)
            agent.SetDestination(walkStart);
        
        Vector3 distancetoWalkStart = transform.position - walkStart;

        if(distancetoWalkStart.magnitude < 1f){
            walkStartSet = false;
        }
    }

    private void chasePlayer(){
        agent.SetDestination(player.position);
    }

    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkRange, walkRange);
        float randomX = Random.Range(-walkRange, walkRange);

        walkStart = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkStart, -transform.up, 2f, ground))
            walkStartSet = true;
    }
}
