using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockDoubleJump : MonoBehaviour
{
    public GameObject unlockJump;
    // Start is called before the first frame update
    public GameObject tutorialCav;
    void Start()
    {
        tutorialCav.SetActive(false);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.name == "PlayerBody"){
            unlockJump.GetComponent<AudioSource>().Play();
            other.GetComponent<player>().doubleJumpUnlocked = true;
            tutorialCav.SetActive(true);
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            Invoke("DisableText", 5f);//invoke after 5 seconds
        }
    }
       void DisableText()
   { 
      tutorialCav.SetActive(false); 
   }    
}
