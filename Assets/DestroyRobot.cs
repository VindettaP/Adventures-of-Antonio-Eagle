using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyRobot : MonoBehaviour
{

    public GameObject deathSound;

    public void playSound(){
        deathSound.GetComponent<AudioSource>().Play();
    }
    // Start is called before the first frame update
    public void DestroyMe(){
        Destroy(gameObject);
    }
}
