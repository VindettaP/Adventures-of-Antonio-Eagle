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
    player cs = other.gameObject.GetComponent<player>();
    bool djunlock = cs.doubleJumpUnlocked;
    bool grunlock = cs.grappleUnlocked;
    bool daunlock = cs.dashUnlocked;
    Destroy (other.gameObject);
    GameObject newPlayer = Instantiate(playerPrefab, checkpoint.transform.position, checkpoint.transform.rotation);
    newPlayer.GetComponent<player>().doubleJumpUnlocked = djunlock;
    newPlayer.GetComponent<player>().grappleUnlocked = grunlock;
    newPlayer.GetComponent<player>().dashUnlocked = daunlock;
}
}

} 

