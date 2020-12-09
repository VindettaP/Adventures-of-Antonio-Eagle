using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotFootSteps : MonoBehaviour
{
    private AudioSource sound;
    // Start is called before the first frame update
    void Start()
    {
        sound = gameObject.GetComponent<AudioSource>();
    }

    public void FootStep(){
        sound.Play();
    }
}
    
