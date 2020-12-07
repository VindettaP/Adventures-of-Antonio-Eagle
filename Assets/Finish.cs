using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    // Start is called before the first frame update
    private Collider end;
    public GameObject endScreen;
    void Awake()
    {
        end = gameObject.GetComponent<BoxCollider>();
        endScreen.SetActive(false);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other){
        if(other.name == "PlayerBody"){
            endScreen.SetActive(true);
            Destroy(other.gameObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
    }
}
