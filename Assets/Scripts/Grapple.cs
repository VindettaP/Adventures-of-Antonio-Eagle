using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public LayerMask grappleAble;
    public Transform grappleTip, camera, player;
    public float grappleDistance = 100f;
    public float jointMaxMod = 0.8f;
    public float jointMinMod = 0.2f;
    public float jointSpring = 4.5f;
    public float jointDamper = 7f;
    public float jointMass = 4.5f;
    public bool grappling = false;
    public Vector3 grapplePoint;

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
        if (Input.GetMouseButtonDown(0))
        {
            StartGrapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopGrapple();
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, grappleDistance, grappleAble))
        {
            grapplePoint = hit.point;
            grappling = true;
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            lr.positionCount = 2;
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
