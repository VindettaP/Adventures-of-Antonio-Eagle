using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teach : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject tutorialCav;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void closeOnButton(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        tutorialCav.SetActive(false);
    }
}
