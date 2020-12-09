using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitTriiger : MonoBehaviour
{
    public GameObject stairs;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody" && other.gameObject.GetComponent<player>().grappleUnlocked){
            Destroy(stairs);
            Destroy(gameObject);
        }
    }
}
