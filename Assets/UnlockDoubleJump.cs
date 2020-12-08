using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockDoubleJump : MonoBehaviour
{
    public GameObject unlockJump;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.name == "PlayerBody"){
            unlockJump.GetComponent<AudioSource>().Play();
            other.GetComponent<player>().doubleJumpUnlocked = true;
        }
    }
}
