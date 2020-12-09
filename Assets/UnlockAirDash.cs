using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAirDash : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject unlockDash;
    public GameObject tutorialCav;
    void Start(){
        tutorialCav.SetActive(false);
    }
    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            unlockDash.GetComponent<AudioSource>().Play();
            other.GetComponent<player>().dashUnlocked = true;
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
