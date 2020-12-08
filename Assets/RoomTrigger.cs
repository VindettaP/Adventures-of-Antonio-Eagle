using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public SphereCollider trigger;
    public GameObject wall;
    private Vector3 velocity = Vector3.zero;
    private bool triggered = false;
    private Vector3 activated;
    // Start is called before the first frame update
    void Start()
    {
        trigger = gameObject.GetComponent<SphereCollider>();
        activated = new Vector3(wall.transform.position.x, wall.transform.position.y - 5.075215f, wall.transform.position.z);
    }
    
    void Update(){
        if(triggered){
            if(wall.transform.position == activated){
                Destroy(gameObject);
            }
            wall.transform.position = Vector3.SmoothDamp(wall.transform.position, activated, ref velocity, 80f * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            triggered = true;
            gameObject.GetComponent<SphereCollider>().enabled = false;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
