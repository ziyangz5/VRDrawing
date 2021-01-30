using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class HeadsetLaser : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject laserCaster;
    public GameObject cursor;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {

        float default_dist = 100f;

        Ray raycast = new Ray(laserCaster.transform.position, laserCaster.transform.forward);
        RaycastHit hit;
        //int layer_mask = LayerMask.GetMask("Canvas");
        bool bHit = Physics.Raycast(raycast, out hit);
        Vector3 endPosition = laserCaster.transform.position + (laserCaster.transform.forward * default_dist);
        if (!bHit)
        {
        }
        if (bHit && hit.distance < 100f)
        {
            endPosition = hit.point;
        }

        if (bHit)
        {
            if (hit.transform.tag == "UI")//TODO: General UI Interaction Interface
            {
                cursor.SetActive(true);
                cursor.transform.position = hit.point;
                hit.transform.SendMessage("Hover");
                if (SteamVR_Actions.default_InteractUI.GetState(SteamVR_Input_Sources.LeftHand))
                {
                    hit.transform.SendMessage("Press");
                }

            }
            else
            {
                cursor.SetActive(false);
            }

        }
        else
        {
            cursor.SetActive(false);
        }


    }
}
