using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask grappleable;
    public Transform tip, cam, player;

    private float maxDistance = 100f;
    private SpringJoint joint;

    void Awake(){
        lr = GetComponent<LineRenderer>();
    }

    void Update(){
        Debug.Log(Time.timeScale);
        if(Input.GetMouseButtonDown(1) && Time.timeScale > 0)
        {
            StartGrapple();
        }

        if(Input.GetMouseButtonUp(1) && Time.timeScale > 0)
        {
            StopGrapple();
        }
    }

    void LateUpdate(){
        drawRope();
    }

    void StartGrapple(){
        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxDistance)){
            grapplePoint = hit.point;
            joint =player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            //distance grapple will try to keep from grapple point
            joint.maxDistance = distanceFromPoint * .8f;
            joint.maxDistance = distanceFromPoint * .25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
        }
    }

    void drawRope(){
        //if no join, return
        if(!joint) return;
        lr.SetPosition(0, tip.position);
        lr.SetPosition(1, grapplePoint);
    }

    void StopGrapple(){
        lr.positionCount = 0;
        Destroy(joint);
    }

}
