using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAirDash : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){

    }
    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            other.GetComponent<player>().dashUnlocked = true;
        }
    }
}
