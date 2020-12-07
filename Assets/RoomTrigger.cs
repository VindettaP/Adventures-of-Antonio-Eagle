using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public SphereCollider trigger;
    public GameObject wall;
    private Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        trigger = gameObject.GetComponent<SphereCollider>();
    }

    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            float step = 500 * Time.deltaTime;
            Vector3 activated = new Vector3(wall.transform.position.x, wall.transform.position.y - 5.075215f, wall.transform.position.z);
            wall.transform.position = Vector3.MoveTowards(wall.transform.position, activated, step); 
            Destroy(gameObject);
        }
    }
}
