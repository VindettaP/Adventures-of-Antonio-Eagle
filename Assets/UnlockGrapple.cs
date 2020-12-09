using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockGrapple : MonoBehaviour
{
    public SphereCollider trigger;
    public GameObject wall;
    private Vector3 velocity = Vector3.zero;
    public GameObject unlockGrapple;
    public GameObject close;
    public GameObject tutorialCav;
    // Start is called before the first frame update

    Vector3 activated;
    private bool triggered;
    void Start()
    {
        trigger = gameObject.GetComponent<SphereCollider>();
        triggered = false;
        activated = new Vector3(wall.transform.position.x, wall.transform.position.y - 8.55f, wall.transform.position.z);
    }

    void Update(){
        if(triggered){
            if(wall.transform.position == activated){
                Destroy(gameObject);
            }
            wall.transform.position = Vector3.SmoothDamp(wall.transform.position, activated, ref velocity, 100f * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            triggered = true;
            close.GetComponent<AudioSource>().Play();
            unlockGrapple.GetComponent<AudioSource>().Play();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<SphereCollider>().enabled = false;
            other.GetComponent<player>().grappleUnlocked = true;
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
