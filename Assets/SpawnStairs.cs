using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnStairs : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stairs;
    void Start()
    {
        stairs.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(GameObject.FindGameObjectsWithTag("Enemy").Length == 0){
            stairs.gameObject.SetActive(true);
        }
    }
}
