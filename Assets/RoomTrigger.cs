using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider trigger;
    public GameObject wall;
    // Start is called before the first frame update
    void Start()
    {
        trigger = gameObject.GetComponent<BoxCollider>();
        
    }

    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            float step = 200 * Time.deltaTime;
            Vector3 activated = new Vector3(wall.transform.position.x, wall.transform.position.y + 6.64f, wall.transform.position.z);
            wall.transform.position = Vector3.MoveTowards(wall.transform.position, activated, step); 
            Destroy(this);
        }
    }
}
