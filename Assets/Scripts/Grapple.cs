using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public LayerMask grappleAble;

    public LayerMask Enemy;
    public Transform grappleTip, cam, player;
    public float grappleDistance = 100f;
    public bool grappling = false;
    public Vector3 grapplePoint;
    public AudioSource a_source;
    public AudioClip grappleSound;
    public bool grappleUnlocked = false;

    private LineRenderer lr;
    

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (grappleUnlocked)
        {
            if (Input.GetMouseButtonDown(0) && Time.timeScale > 0)
            {
                StartGrapple();
            }
            else if (Input.GetMouseButtonUp(0) && Time.timeScale > 0)
            {
                StopGrapple();
            }
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, grappleDistance, Enemy))
        {
            if(hit.collider.gameObject.tag == "Enemy"){
                grapplePoint = hit.point;
                hit.collider.gameObject.GetComponent<EnemyAI>().enemyHealth = 0;
                float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
                lr.positionCount = 2;
                a_source.PlayOneShot(grappleSound);
            }
        }
        if (Physics.Raycast(cam.position, cam.forward, out hit, grappleDistance, grappleAble))
        {
            grapplePoint = hit.point;
            grappling = true;
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            lr.positionCount = 2;
            a_source.PlayOneShot(grappleSound);
        }
    }

    void DrawRope()
    {
        // Don't draw when no joint
        if (!grappling)
            return;
        lr.SetPosition(0, grappleTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    public void StopGrapple()
    {
        grappling = false;
        lr.positionCount = 0;
    }
}
