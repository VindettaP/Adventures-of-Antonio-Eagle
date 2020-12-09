using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ouchiebox : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject checkpoint;
    public GameObject playerPrefab;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
{
if (other.gameObject.tag == "Player") {
        Debug.Log("aa");
    //GameObject newplayer = Instantiate(playerPrefab, checkpoint.transform.position, checkpoint.transform.rotation);
    //Destroy (other.gameObject);
    other.gameObject.transform.position = checkpoint.transform.position;

    }
    }

} 

